using AutoMapper;
using MexicanRestaurant.Application.Helpers;
using MexicanRestaurant.Core.Interfaces;
using MexicanRestaurant.Core.Models;
using MexicanRestaurant.Core.Specifications;
using MexicanRestaurant.Views.Shared;
using MexicanRestaurant.WebUI.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MexicanRestaurant.Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IRepository<Product> _products;
        private readonly IRepository<Category> _categories;
        private readonly IRepository<Ingredient> _ingredients;
        private readonly IImageService _imageService;
        private readonly IMapper _mapper;

        public ProductService(IRepository<Product> products, IRepository<Category> categories, IRepository<Ingredient> ingredients, IWebHostEnvironment webHostEnvironment, IImageService imageService, IMapper mapper)
        {
            _products = products;
            _categories = categories;
            _ingredients = ingredients;
            _imageService = imageService;
            _mapper = mapper;
        }

        public async Task<IEnumerable<Product>> GetAllProductsAsync()
        {
            return await _products.GetAllAsync(new QueryOptions<Product>
            {
                Includes = nameof(Product.Category)
            });
        }

        public async Task<Product> GetProductByIdAsync(int id)
        {
            return await _products.GetByIdAsync(id, new QueryOptions<Product>
            {
                Includes = "ProductIngredients.Ingredient, Category"
            });
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
                    throw new Exception("Product not found during update.");

                if (existingProduct != null)
                {
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
        }

        public async Task DeleteProductAsync(int id)
        {
            var product = await GetProductByIdAsync(id);

            if (product == null) return;

            await _imageService.DeleteImageAsync(product.ImageUrl, "images");
            await _products.DeleteAsync(id);
        }

        public async Task<List<SelectListItem>> GetCategorySelectListAsync()
        {
            var categories = await _categories.GetAllAsync();
            return categories.Select(c => new SelectListItem
            {
                Value = c.CategoryId.ToString(),
                Text = c.Name
            }).ToList();
        }

        public async Task<IEnumerable<Ingredient>> GetAllIngredientsAsync()
        {
            return await _ingredients.GetAllAsync();
        }

        public async Task<ProductListViewModel> GetPagedProductsAsync(FilterOptionsViewModel filter, PaginationInfo pagination)
        {
            var options = new QueryOptions<Product>
            {
                Includes = nameof(Product.Category),
                PageNumber = pagination.CurrentPage,
                PageSize = pagination.PageSize,
                Where = ProductFilteringHelper.BuildFilter(filter.SearchTerm, filter.SelectedCategoryId),
                OrderByWithFunc = ProductFilteringHelper.BuildOrderBy(filter.SortBy)
            };
            var allProducts = await _products.GetAllAsync(options);
            var mappedProducts = _mapper.Map<IEnumerable<ProductViewModel>>(allProducts);
            var countOptions = new QueryOptions<Product>
            {
                Where = options.Where,
                Includes = nameof(Product.Category),
                DisablePaging = true
            };
            var totalProducts = (await _products.GetAllAsync(countOptions)).Count();
            var categories = await GetCategorySelectListAsync();

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
    }
}
