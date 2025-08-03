using AutoMapper;
using AutoMapper.QueryableExtensions;
using MexicanRestaurant.Application.Helpers;
using MexicanRestaurant.Core.Interfaces;
using MexicanRestaurant.Core.Models;
using MexicanRestaurant.Core.Specifications;
using MexicanRestaurant.Views.Shared;
using MexicanRestaurant.WebUI.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace MexicanRestaurant.Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IRepository<Product> _products;
        private readonly ISharedLookupService _sharedLookupService;
        private readonly IImageService _imageService;
        private readonly IPaginatedProductFetcher _paginatedProductFetcher;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuditLogHelper _auditLogHelper;
        private readonly IMapper _mapper;

        public ProductService(IRepository<Product> products, ISharedLookupService sharedLookupService, IWebHostEnvironment webHostEnvironment, IImageService imageService, IPaginatedProductFetcher paginatedProductFetcher, IHttpContextAccessor httpContextAccessor, IAuditLogHelper auditLogHelper, IMapper mapper)
        {
            _products = products;
            _sharedLookupService = sharedLookupService;
            _imageService = imageService;
            _paginatedProductFetcher = paginatedProductFetcher;
            _httpContextAccessor = httpContextAccessor;
            _auditLogHelper = auditLogHelper;
            _mapper = mapper;
        }

        public async Task<int> GetTotalProductsAsync()
        {
            return await _products.Table.CountAsync();
        }

        public async Task<Product> GetExistingProductByIdAsync(int id)
        {
            return await _products.GetByIdAsync(id, new QueryOptions<Product>
            {
                Includes = "ProductIngredients.Ingredient, Category",
                Where = p => p.ProductId == id
            });
        }

        public async Task<ProductViewModel?> GetProductViewModelByIdAsync(int id)
        {
            var query = _products.Table
                .AsNoTracking()
                .Where(p => p.ProductId == id);

            var product = await query.ProjectTo<ProductViewModel>(_mapper.ConfigurationProvider).FirstOrDefaultAsync();
            return product;
        }

        public async Task AddOrUpdateProductAsync(Product product, int[] ingredientIds, string existingImageUrl)
        {
            var isCreate = product.ProductId == 0;

            if (product.ImageFile != null)
            {
                if (!string.IsNullOrEmpty(existingImageUrl))
                    await _imageService.DeleteImageAsync(existingImageUrl, "images");

                product.ImageUrl = await _imageService.UploadImageAsync(product.ImageFile, "images");
            }
            else
                product.ImageUrl = existingImageUrl ?? product.ImageUrl;

            if (isCreate)
            {
                product.ProductIngredients = ingredientIds.Select(id => new ProductIngredient { IngredientId = id }).ToList();
                await _products.AddAsync(product);
            }
            else
            {
                var existingProduct = await _products.GetByIdAsync(product.ProductId, new QueryOptions<Product>
                {
                    Includes = "ProductIngredients",
                });

                if (existingProduct is null)
                    throw new ProductNotFoundException("Product not found during update.");

                existingProduct.Name = product.Name;
                existingProduct.Description = product.Description;
                existingProduct.Price = product.Price;
                existingProduct.Stock = product.Stock;
                existingProduct.CategoryId = product.CategoryId;
                existingProduct.ImageUrl = product.ImageUrl;

                existingProduct?.ProductIngredients?.Clear();
                foreach (var id in ingredientIds)
                {
                    existingProduct?.ProductIngredients?.Add(new ProductIngredient { IngredientId = id });
                }

                if (existingProduct?.ProductIngredients == null)
                    throw new ApplicationException("Product ingredients could not be updated.");

                await _products.UpdateAsync(existingProduct);
            }

            var httpContext = _httpContextAccessor.HttpContext ?? throw new InvalidOperationException("HTTP context is not available for logging.");
            await _auditLogHelper.LogActionAsync(httpContext, isCreate ? "Create" : "Update", "Product", product.ProductId.ToString(), $"Product: {product.Name}, Price: {product.Price}, Stock: {product.Stock}");
        }

        public async Task DeleteProductAsync(int id)
        {
            var product = await _products.Table
                .Where(p => p.ProductId == id)
                .Select(p => new { p.ProductId, p.Name, p.ImageUrl }).FirstOrDefaultAsync();
            if (product == null) throw new ProductNotFoundException($"Cannot delete: product with ID {id} not found.");

            var httpContext = _httpContextAccessor.HttpContext ?? throw new InvalidOperationException("HTTP context is not available for logging.");

            await _imageService.DeleteImageAsync(product.ImageUrl, "images");
            await _products.DeleteAsync(id);
            await _auditLogHelper.LogActionAsync(httpContext, "Delete", "Product", id.ToString(), $"Deleted product: {product.Name}");
        }

        public async Task<ProductListViewModel> GetPagedProductsAsync(FilterOptionsViewModel filter, PaginationInfo pagination)
        {
            var (allProducts, totalProducts) = await _paginatedProductFetcher.GetPagedProductsAsync(filter, pagination);
            var categories = await _sharedLookupService.GetCategorySelectListAsync();

            return new ProductListViewModel
            {
                Products = allProducts,
                Filter = new FilterOptionsViewModel
                {
                    SearchTerm = filter.SearchTerm,
                    SelectedCategoryId = filter.SelectedCategoryId,
                    SortBy = filter.SortBy,
                    Categories = categories
                },
                Pagination = new PaginationInfo
                {
                    CurrentPage = pagination.CurrentPage,
                    PageSize = pagination.PageSize,
                    TotalPages = (int)Math.Ceiling((double)totalProducts / pagination.PageSize)
                },
            };
        }
    }
}
