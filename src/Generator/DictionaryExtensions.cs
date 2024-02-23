using System.Collections.Generic;

namespace Generator;

internal static class DictionaryExtensions
{
    internal static void AddOrAppend<TKey, TValue>(this Dictionary<TKey, HashSet<TValue>> dictionary, TKey key, TValue value)
    {
        if (dictionary.TryGetValue(key, out var list))
        {
            list.Add(value);
        }
        else
        {
            dictionary.Add(key, [value]);
        }
    }
}