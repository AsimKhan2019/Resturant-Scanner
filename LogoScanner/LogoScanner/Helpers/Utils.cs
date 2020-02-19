using System;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using Xamarin.Forms.Internals;

namespace LogoScanner.Helpers
{
    public class Utils
    {
        // method to get field from json object
        public static string GetRestaurantField(JObject json, string field)
        {
            if (json[field].Type == JTokenType.Null || string.IsNullOrEmpty(json[field].ToString()))
                return "No Set " + field;
            else if (json[field] is JArray)
            {
                StringBuilder builder = new StringBuilder();
                JToken[] arr = json[field].ToArray();

                foreach (string a in arr)
                {
                    builder.Append(a);
                    if (arr.IndexOf(a) != arr.Count() - 1) builder.Append(", ");
                }

                return builder.ToString();
            }
            else
            {
                return json[field].ToString();
            }
        }

        // method to get field from json object and produce a string with symbols which are repeated i number of times
        public static string GetRestaurantField(JObject json, string field, string symbol, int i)
        {
            if (json[field].Type == JTokenType.Null || string.IsNullOrEmpty(json[field].ToString()))
                return "No Set " + field;
            else
                return String.Concat(Enumerable.Repeat(symbol, i));
        }
    }
}
