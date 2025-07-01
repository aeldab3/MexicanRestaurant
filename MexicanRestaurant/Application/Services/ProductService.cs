using MexicanRestaurant.Core.Extensions;
using MexicanRestaurant.Core.Interfaces;
using MexicanRestaurant.Core.Models;
using MexicanRestaurant.Core.Specifications;
using MexicanRestaurant.WebUI.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq.Expressions;

namespace MexicanRestaurant.Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IRepository<Product> _products;
        private readonly IRepository<Category> _categories;
        private readonly IRepository<Ingredient> _ingredients;
        private readonly IImageService _imageService;

        public ProductService(IRepository<Product> products, IRepository<Category> categories, IRepository<Ingredient> ingredients, IWebHostEnvironment webHostEnvironment, IImageService imageService)
        {
            _products = products;
            _categories = categories;
            _ingredients = ingredients;
            _imageService = imageService;
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
                product.ImageUrl = existingImageUrl;

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

        public async Task<ProductListViewModel> GetPagedProductsAsync(int pageNumber, int pageSize, string searchTerm, int? categoryId, string sortBy)
        {
            var options = new QueryOptions<Product>
            {
                Includes = nameof(Product.Category),
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            // Combine Where expressions
            Expression<Func<Product, bool>> filter = p => true;

            if (!string.IsNullOrEmpty(searchTerm))
                filter = filter.AndAlso(p =>
                    p.Name.ToLower().Contains(searchTerm.ToLower()) ||
                    p.Description.ToLower().Contains(searchTerm.ToLower()));

            if (categoryId.HasValue && categoryId.Value > 0)
                filter = filter.AndAlso(p => p.CategoryId == categoryId.Value);

            if (filter != null)
                options.Where = filter;

            if (!string.IsNullOrEmpty(sortBy))
            {
                options.OrderBy = sortBy switch
                {
                    "name_asc" => p => p.Name,
                    "name_desc" => p => p.Name,
                    "price_asc" => p => p.Price,
                    "price_desc" => p => p.Price,
                    _ => p => p.Name
                };

                if (sortBy == "name_desc" || sortBy == "price_desc")
                    options.IsDescending = true;
            }
            else
                options.OrderBy = p => p.Name;

            var allProducts = await _products.GetAllAsync(options);
            var countOptions = new QueryOptions<Product>
            {
                Where = options.Where,
                Includes = nameof(Product.Category),
                PageNumber = 0,
                PageSize = 0
            };
            var totalProducts = (await _products.GetAllAsync(countOptions)).Count();
            var categories = await GetCategorySelectListAsync();

            return new ProductListViewModel
            {
                Products = allProducts.ToList(),
                CurrentPage = pageNumber,
                TotalPages = (int)Math.Ceiling((double)totalProducts / pageSize),
                SearchTerm = searchTerm,
                SelectedCategoryId = categoryId,
                SortBy = sortBy,
                Categories = categories
            };
        }
    }
}
