using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace YoutubePlayer.Extensions
{
    static class JsonExtensions
    {
        public static bool IsValidField(this JObject jobject, string field, JTokenType type)
        {
            return jobject != null && jobject.ContainsKey(field) && jobject[field].Type == type;
        }

        public static bool IsValidField<T>(this JObject jobject, string field, JTokenType type, T value)
        {
            return jobject != null && jobject.ContainsKey(field) && jobject[field].Type == type && EqualityComparer<T>.Default.Equals(jobject[field].Value<T>(), value);
        }

        public static T ToValidValue<T>(this JToken jvalue, JTokenType type, T defaultValue)
        {
            return jvalue != null && jvalue.Type == type ? jvalue.Value<T>() : defaultValue;
        }
    }
}
