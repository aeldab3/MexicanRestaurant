using AutoMapper;
using MexicanRestaurant.Application.Helpers;
using MexicanRestaurant.Core.Interfaces;
using MexicanRestaurant.Core.Models;
using MexicanRestaurant.Core.Specifications;
using MexicanRestaurant.Views.Shared;
using MexicanRestaurant.WebUI.ViewModels;

namespace MexicanRestaurant.Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IRepository<Product> _products;
        private readonly ISharedLookupService _sharedLookupService;
        private readonly IImageService _imageService;
        private readonly IPaginatedProductFetcher _paginatedProductFetcher;
        private readonly ILogger<ProductService> _logger;

        public ProductService(IRepository<Product> products, ISharedLookupService sharedLookupService, IWebHostEnvironment webHostEnvironment, IImageService imageService, IPaginatedProductFetcher paginatedProductFetcher, ILogger<ProductService> logger)
        {
            _products = products;
            _sharedLookupService = sharedLookupService;
            _imageService = imageService;
            _paginatedProductFetcher = paginatedProductFetcher;
            _logger = logger;
        }

        public async Task<Product> GetProductByIdAsync(int id)
        {
            try 
            { 
                return await _products.GetByIdAsync(id, new QueryOptions<Product>
                {
                    Includes = "ProductIngredients.Ingredient, Category"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching product with id: {id}");
                throw new ProductNotFoundException($"Error retrieving product with ID {id}: {ex.Message}");
            }
        }

        public async Task<Product> GetExistingProductByIdAsync(int id)
        {
            return await _products.GetByIdAsync(id, new QueryOptions<Product>
            {
                Includes = "ProductIngredients.Ingredient, Category",
                Where = p => p.ProductId == id
            });
        }

        public async Task AddOrUpdateProductAsync(Product product, int[] ingredientIds, string existingImageUrl)
        {
            try 
            { 
                if (product.ImageFile != null)
                {
                    if (!string.IsNullOrEmpty(existingImageUrl))
                        await _imageService.DeleteImageAsync(existingImageUrl, "images");

                    product.ImageUrl = await _imageService.UploadImageAsync(product.ImageFile, "images");
                }
                else
                    product.ImageUrl = existingImageUrl ?? product.ImageUrl;

                if (product.ProductId == 0)
                {
                    product.ProductIngredients = ingredientIds.Select(id => new ProductIngredient { IngredientId = id }).ToList();
                    await _products.AddAsync(product);
                }
                else
                {
                    var existingProduct = await _products.GetByIdAsync(product.ProductId, new QueryOptions<Product>
                    {
                        Includes = "ProductIngredients",
                        Where = p => p.ProductId == product.ProductId
                    });
                    if (existingProduct == null)
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
                        existingProduct.ProductIngredients.Add(new ProductIngredient { IngredientId = id  });
                    }
                    await _products.UpdateAsync(existingProduct);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error adding/updating product with id: {product.ProductId}");
                throw new ProductNotFoundException($"Error processing product with ID {product.ProductId}: {ex.Message}");
            }
        }

        public async Task DeleteProductAsync(int id)
        {
            var product = await GetProductByIdAsync(id);

            if (product == null) return;

            await _imageService.DeleteImageAsync(product.ImageUrl, "images");
            await _products.DeleteAsync(id);
        }

        public async Task<ProductListViewModel> GetPagedProductsAsync(FilterOptionsViewModel filter, PaginationInfo pagination)
        {
            try
            { 
                var (mappedProducts, totalProducts) = await _paginatedProductFetcher.GetPagedProductsAsync(filter, pagination);
                var categories = await _sharedLookupService.GetCategorySelectListAsync();

                return new ProductListViewModel
                {
                    Products = mappedProducts,
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching paginated products");
                throw new ProductNotFoundException($"Error fetching paginated products: {ex.Message}");
            }
        }
    }
}
