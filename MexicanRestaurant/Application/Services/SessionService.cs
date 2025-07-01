using MexicanRestaurant.Core.Interfaces;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MexicanRestaurant.Application.Services
{
    public class SessionService : ISessionService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public SessionService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        public void Set<T>(string key, T value)
        {
            var json = JsonSerializer.Serialize(value);
            _httpContextAccessor.HttpContext?.Session.SetString(key, json);
        }

        public T Get<T>(string key)
        {
            var json = _httpContextAccessor.HttpContext?.Session.GetString(key);
            return string.IsNullOrEmpty(json) ? default : JsonSerializer.Deserialize<T>(json);
        }
        public void Remove(string key)
        {
            _httpContextAccessor.HttpContext?.Session.Remove(key);
        }
    }
}
