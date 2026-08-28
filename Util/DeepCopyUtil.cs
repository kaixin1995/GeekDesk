using Newtonsoft.Json;
using System;

namespace GeekDesk.Util
{
    public static class DeepCopyUtil
    {
        public static T DeepCopy<T>(T obj)
        {
            if (obj == null)
                return default;

            string json = JsonConvert.SerializeObject(obj, Formatting.None);
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}
