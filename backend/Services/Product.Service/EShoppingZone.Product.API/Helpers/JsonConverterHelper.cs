using Newtonsoft.Json;

namespace EShoppingZone.Product.API.Helpers
{
    public static class JsonConverterHelper
    {
        public static string SerializeObject<T>(T obj)
        {
            return JsonConvert.SerializeObject(obj);
        }
        
        public static T DeserializeObject<T>(string json)
        {
            if (string.IsNullOrEmpty(json))
                return Activator.CreateInstance<T>();
                
            return JsonConvert.DeserializeObject<T>(json) ?? Activator.CreateInstance<T>();
        }
        
        public static Dictionary<int, double> DeserializeRatings(string json)
        {
            if (string.IsNullOrEmpty(json))
                return new Dictionary<int, double>();
                
            return JsonConvert.DeserializeObject<Dictionary<int, double>>(json) ?? new Dictionary<int, double>();
        }
        
        public static Dictionary<int, string> DeserializeReviews(string json)
        {
            if (string.IsNullOrEmpty(json))
                return new Dictionary<int, string>();
                
            return JsonConvert.DeserializeObject<Dictionary<int, string>>(json) ?? new Dictionary<int, string>();
        }
        
        public static List<string> DeserializeImages(string json)
        {
            if (string.IsNullOrEmpty(json))
                return new List<string>();
                
            return JsonConvert.DeserializeObject<List<string>>(json) ?? new List<string>();
        }
        
        public static Dictionary<string, string> DeserializeSpecifications(string json)
        {
            if (string.IsNullOrEmpty(json))
                return new Dictionary<string, string>();
                
            return JsonConvert.DeserializeObject<Dictionary<string, string>>(json) ?? new Dictionary<string, string>();
        }
    }
}