using System;
using System.Web;
using System.Web.Caching;

/// <summary>
/// Simple class the helps Cache 
/// </summary>
public class XCacheTool
{
    private static readonly object CacheLock = new object();

    public static T GetOrAdd<T>(string key, Func<T> valueFactory, int expirationSeconds = 1800,bool clearCache = false)
    {
        var data = (T)HttpRuntime.Cache[key];
        if (data != null && !clearCache) return data;
        lock (CacheLock)
        {
            data = (T)HttpRuntime.Cache[key];
            if (data != null && !clearCache) return data;
            data = valueFactory.Invoke();
            var expirationTime = DateTime.Now.AddSeconds(expirationSeconds);
            HttpRuntime.Cache.Insert(key, data, null, expirationTime, Cache.NoSlidingExpiration);
        }

        return data;
    }
}