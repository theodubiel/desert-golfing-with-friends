using System.Collections.Generic;

public class GlobalCache
{
    private static Dictionary<string, Dictionary<string, object>> cache = new Dictionary<string, Dictionary<string, object>>();
    public static T Get<T>(string id, T fallback = default(T)) {
        var className = typeof(T).FullName;
        if (cache.ContainsKey(className)) {
            var classCache = cache[className];
            if (classCache.ContainsKey(id)) {
                return (T)classCache[id];
            }
        }
        return fallback;
    }

    public static void Set<T>(string id, T data) {
        var className = typeof(T).FullName;
        if (!cache.ContainsKey(className)) {
            cache.Add(className, new Dictionary<string, object>());
        }
        cache[className].Remove(id);
        cache[className].Add(id, data);
    }

    public static void Clear() {
        cache.Clear();
    }
}
