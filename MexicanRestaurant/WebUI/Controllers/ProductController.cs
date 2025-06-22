using MexicanRestaurant.Core.Interfaces;
using MexicanRestaurant.Core.Models;
using MexicanRestaurant.Core.Specifications;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MexicanRestaurant.WebUI.Controllers
{
    public class ProductController : Controller
    {
        private readonly IRepository<Product> _products;
        private readonly IRepository<Category> _categories;
        private readonly IRepository<Ingredient> _ingredients;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ProductController(IRepository<Product> products, IRepository<Category> categories, IRepository<Ingredient> ingredients, IWebHostEnvironment webHostEnvironment)
        {
            _products = products;
            _categories = categories;
            _ingredients = ingredients;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var products = await _products.GetAllAsync(new QueryOptions<Product>
            {
                Includes = nameof(Product.Category)
            });
            return View(products);
        }

        [HttpGet]
        public async Task<IActionResult> AddEdit(int id)
        {
            var categories = await _categories.GetAllAsync();
            ViewBag.Categories = categories.Select(c => new SelectListItem
            {
                Value = c.CategoryId.ToString(),
                Text = c.Name
            }).ToList();

            ViewBag.Ingredients = await _ingredients.GetAllAsync();

            if (id == 0)
            {
                ViewBag.Operation = "Add";
                return View(new Product());
            }
            else
            {
                Product product = await _products.GetByIdAsync(id, new QueryOptions<Product>
                {
                    Includes = "ProductIngredients.Ingredient, Category",
                    Where = p => p.ProductId == id
                });
                ViewBag.Operation = "Edit";
                return View(product);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddEdit(Product product, int[] ingredientIds, string ExistingImageUrl)
        {
            var categories = await _categories.GetAllAsync();
            ViewBag.Categories = categories.Select(c => new SelectListItem
            {
                Value = c.CategoryId.ToString(),
                Text = c.Name
            }).ToList();

            ViewBag.Ingredients = await _ingredients.GetAllAsync();

            if (product.CategoryId == 0)
            {
                ModelState.AddModelError("CategoryId", "Please select a category.");
            }

            if (ModelState.IsValid)
            {
                if (product.ImageFile != null)
                {
                    var fileName = Path.GetFileNameWithoutExtension(product.ImageFile.FileName);
                    var extension = Path.GetExtension(product.ImageFile.FileName);
                    product.ImageUrl = $"{fileName}_{DateTime.Now:yyyyMMddHHmmssfff}{extension}";
                    var path = Path.Combine(_webHostEnvironment.WebRootPath, "images", product.ImageUrl);
                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await product.ImageFile.CopyToAsync(stream);
                    }
                }
                else
                {
                    product.ImageUrl = ExistingImageUrl;
                }

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
                return RedirectToAction("Index", "Product");
            }

            ViewBag.Categories = (await _categories.GetAllAsync()).Select(c => new SelectListItem
            {
                Value = c.CategoryId.ToString(),
                Text = c.Name
            }).ToList();

            ViewBag.Ingredients = await _ingredients.GetAllAsync();

            ViewBag.Operation = product.ProductId == 0 ? "Add" : "Edit";

            return View(product);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var product = await _products.GetByIdAsync(id, new QueryOptions<Product>
            {
                Includes = "ProductIngredients.Ingredient, Category"
            });

            if (product == null)
                return NotFound();

            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _products.GetByIdAsync(id, new QueryOptions<Product>
            {
                Includes = "ProductIngredients.Ingredient, Category"
            });

            if (product == null)
                return NotFound();

            if (!string.IsNullOrEmpty(product.ImageUrl))
            {
                var imagePath = Path.Combine(_webHostEnvironment.WebRootPath, "images", product.ImageUrl);
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }
            await _products.DeleteAsync(id);
            return RedirectToAction("Index");
        }
    }
}
