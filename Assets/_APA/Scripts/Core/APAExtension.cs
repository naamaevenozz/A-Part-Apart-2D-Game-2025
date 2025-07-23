using System.Collections.Generic;
using System.Linq;
using Random = UnityEngine.Random;

namespace APA.Core
{
    public static class APAExtension
    {
        public static T GetRandomFromArray<T>(this T[] array)
        {
            int random = Random.Range(0, array.Length);
            if (array.Length == 0) return default;

            return array[random];
        }
        public static T GetRandomFromList<T>(this List<T> list)
        {
            int random = Random.Range(0, list.Count);
            if (list.Count == 0) return default;

            return list[random];
        }
        public static KeyValuePair<TKey, TValue> GetRandomFromDictionary<TKey, TValue>(this Dictionary<TKey, TValue> dictionary)
        {
            if (dictionary.Count == 0) return default;

            int randomIndex = Random.Range(0, dictionary.Count);
            var randomElement = dictionary.ElementAt(randomIndex);

            return randomElement;
        }
    }
}
