namespace MexicanRestaurant.Application.Helpers
{
    public class IngredientNotFoundException : Exception
    {
        public IngredientNotFoundException(string message) : base(message) { }
    }
}
