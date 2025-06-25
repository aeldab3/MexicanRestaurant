using MexicanRestaurant.Core.Interfaces;
using MexicanRestaurant.Core.Models;
using MexicanRestaurant.Core.Specifications;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MexicanRestaurant.Core.Services
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
                    Includes = "ProductIngredients"
                });

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
                        existingProduct?.ProductIngredients?.Add(new ProductIngredient { IngredientId = id });
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
    }
}
