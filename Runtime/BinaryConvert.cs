using System.Text;
using Newtonsoft.Json;

namespace CLabs.Utility {
    public static class BinaryConvert {
        public static byte[] ToBytes<T>(this T @object) {
            return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(@object));
        }

        public static byte[] ToBytes<T>(this string json) {
            return Encoding.UTF8.GetBytes(json);
        }

        public static string ToJson(this byte[] bytes) {
            return Encoding.UTF8.GetString(bytes);
        }
        
        public static T ToObject<T>(this byte[] bytes) {
            return JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(bytes));
        }
    }
}