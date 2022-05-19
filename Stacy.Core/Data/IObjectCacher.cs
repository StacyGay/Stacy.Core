using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Stacy.Core.Data
{
    public interface IObjectCacher
    {
        /// <summary>
        /// Sets item to be cached with a certain key, if key is a complex object it will be serialized into json
        /// Can bet identified by an application and bag for manual cache clearing
        /// Timeout is absolute so further grabs from cache wont affect the current timeout
        /// </summary>
        /// <param name="key">String or complex object to define the cache result key</param>
        /// <param name="value">Value to be cached</param>
        /// <param name="timeout">Timeout in seconds to hold the cache.  Default 10 minutes</param>
        /// <param name="bag">Current bag identifier for manual cache clearing. Default: empty string</param>
        /// <param name="application">Current application identifier for manual cache clearing. Default: application</param>
        /// <param name="sliding">Use sliding expiration instead of absolute</param>
        void Set(dynamic key, object value, long timeout = 600, string bag = null, string application = "system", bool sliding = false);

        /// <summary>
        /// Attempts to get object from cache, if the generated key does not exist then run func to generate and store cached result
        /// </summary>
        /// <param name="parameters">Parameters passed to generator function, used to generate cache key</param>
        /// <param name="func">Function to generate initial cached result, used to generate cache key</param>
        /// <param name="timeout">Timeout in seconds to hold the cache.  Default 10 minutes</param>
        /// <param name="bag">Current bag identifier for manual cache clearing. Default: empty string</param>
        /// <param name="application">Current application identifier for manual cache clearing. Default: application</param>
        TResult GetOrSet<TParam, TResult>(TParam parameters, Func<TParam, TResult> func, long timeout = 600, string bag = null, string application = "system");

        /// <summary>
        /// Attempts to get object from cache, if the generated key does not exist then run func to generate and store cached result
        /// </summary>
        /// <param name="func">Function to generate initial cached result, used to generate cache key</param>
        /// <param name="timeout">Timeout in secons to hold the cache.  Default 10 minutes</param>
        /// <param name="bag">Current bag identifier for manual cache clearing. Default: empty string</param>
        /// <param name="application">Current application identifier for manual cache clearing. Default: application</param>
        TResult GetOrSet<TResult>(Func<TResult> func, long timeout = 600, string bag = null, string application = "system");

        T Get<T>(dynamic key);

        /// <summary>
        /// Clears sections of cache, if both bag and application are left blank all cache will be cleared
        /// </summary>
        /// <param name="bag">Current "bag" name of cache</param>
        /// <param name="application">Application name of cache bags, if bag is not defined the entire application cache will be cleared</param>
        Dictionary<string, bool> ClearCache(string bag = null, string application = "");

        /// <summary>
        /// Returns the assigned Guid string for the given bag.  For use with ensuring cache has been cleared
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        string GetBagId(string bag);

        /// <summary>
        /// Clears sections of cache on each server, if both bag and application are left blank all cache will be cleared
        /// </summary>
        /// <param name="bag">Current "bag" name of cache</param>
        /// <param name="webnodes">Webnodes to clear</param>
        Task<bool> ClearCacheOnServers(string bag, List<string> webnodes = null);

        /// <summary>
        /// Get list of current policy bags
        /// </summary>
        /// <returns></returns>
        Dictionary<string, List<string>> GetBags();

        /// <summary>
        /// Caches a query for value given in cacheTimer in seconds
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cnn"></param>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <param name="database"></param>
        /// <param name="transaction"></param>
        /// <param name="buffered"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="commandType"></param>
        /// <param name="cacheTimer">Timespan to cache query in seconds</param>
        /// <returns></returns>
        IList<T> CachedQuery<T>(string sql, dynamic param = null, string database = "master",
            IDbTransaction transaction = null, bool buffered = true,
            int? commandTimeout = null, CommandType? commandType = null, int cacheTimer = 600);

        /// <summary>
        /// Caches a query for value given in cacheTimer in seconds (async)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cnn"></param>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <param name="database"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="commandType"></param>
        /// <param name="cacheTimer">Timespan to cache query in seconds</param>
        /// <returns></returns>
        Task<IList<T>> CachedQueryAsync<T>(string sql, dynamic param = null, string database = "master",
            IDbTransaction transaction = null,
            int? commandTimeout = null, CommandType? commandType = null, int cacheTimer = 600);
    }
}