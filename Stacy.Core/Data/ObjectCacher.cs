using System;
using System.Collections.Concurrent;
using System.Runtime.Caching;
using System.Globalization;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using MoreLinq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.IO;
using Stacy.Core.Types;

namespace Stacy.Core.Data
{
    public class SignaledChangeEventArgs : EventArgs
    {
        public string Name { get; private set; }
        public SignaledChangeEventArgs(string name = null) { this.Name = name; }
    }

    /// <summary>
    /// Cache change monitor that allows an app to fire a change notification
    /// to all associated cache items.
    /// </summary>
    public class SignaledChangeMonitor : ChangeMonitor
    {
        // Shared across all SignaledChangeMonitors in the AppDomain
        private static event EventHandler<SignaledChangeEventArgs> Signaled;

        private readonly string _name;
        private readonly string _uniqueId = Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture);

        public override string UniqueId => _uniqueId;

        public SignaledChangeMonitor(string name = null)
        {
            _name = name;
            // Register instance with the shared event
            Signaled += OnSignalRaised;
            InitializationComplete();
        }

        public static void Signal(string name = null)
        {
            // Raise shared event to notify all subscribers
            Signaled?.Invoke(null, new SignaledChangeEventArgs(name));
        }

        protected override void Dispose(bool disposing)
        {
            Signaled -= OnSignalRaised;
        }

        private void OnSignalRaised(object sender, SignaledChangeEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(e.Name) || string.Compare(e.Name, _name, true) == 0)
            {
                System.Diagnostics.Debug.WriteLine(
                    _uniqueId + " " + _name + " notifying cache of change.", "SignaledChangeMonitor");
                // Cache objects are obligated to remove entry upon change notification.
                OnChanged(null);
            }
        }
    }

    public class ObjectCacher : IObjectCacher
    {
        private readonly ObjectCache _cache;
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, bool>> _names;

        protected readonly string BagKey = "CacheBags";
        private readonly IDataSource _dataSource;

        public ObjectCacher(IDataSource dataSource)
        {
            _dataSource = dataSource;
            _cache = MemoryCache.Default;

            _names = _cache.Get(BagKey) as ConcurrentDictionary<string, ConcurrentDictionary<string, bool>>;
            if (_names != null) return;

            _names = new ConcurrentDictionary<string, ConcurrentDictionary<string, bool>>();
            _cache.Set(BagKey, _names, new DateTimeOffset(DateTime.Today.AddDays(7)));
        }

        /// <summary>
        /// Clears sections of cache, if both bag and application are left blank all cache will be cleared
        /// </summary>
        /// <param name="bag">Current "bag" name of cache</param>
        /// <param name="application">Application name of cache bags, if bag is not defined the entire application cache will be cleared</param>
        public virtual Dictionary<string, bool> ClearCache(string bag = null, string application = "")
        {
            var result = new Dictionary<string, bool>();
            bag = GetBag(bag);

            if (string.IsNullOrEmpty(bag) && string.IsNullOrEmpty(application))
            {
                SignaledChangeMonitor.Signal();
                return result;
            }

            if (!string.IsNullOrEmpty(bag))
            {
                var bags = bag.Split(',').ToList();
                foreach (var b in bags)
                {
                    for (var i = 0; i < 3; i++)
                    {
                        var bagId = GetBagId(b);
                        SignaledChangeMonitor.Signal(b);
                        var newBagId = GetBagId(b);

                        if ((result[b] = !string.Equals(bagId, newBagId)))
                            break;
                    }
                }
            }                
            else if (!string.IsNullOrEmpty(application))
            {
                SignaledChangeMonitor.Signal(application);
            }

            return result;
        }

        /// <summary>
        /// Clears sections of cache on each server, if both bag and application are left blank all cache will be cleared
        /// </summary>
        /// <param name="bag">Current "bag" name of cache</param>
        /// <param name="webnodes">Webnodes to clear</param>
        public async Task<bool> ClearCacheOnServers(string bag, List<string> webnodes = null)
        {
            var baseUrl = "https://replaceme.com"; // TODO: Fill in base url for cache clearing

            if (webnodes == null || !webnodes.Any())
                webnodes = new List<string> { 
                    // TODO: Fill in potential server nodes
                };

            var returnValue = true;

            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            try
            {
                foreach(var webnode in webnodes)
                {
                    client.BaseAddress = new Uri($"{baseUrl}/{bag}/0/en/System/ClearCache?webnode=" + webnode);

                    HttpResponseMessage response = await client.GetAsync("");
                    HttpContent responseContent = response.Content;
                    using (var reader = new StreamReader(await responseContent.ReadAsStreamAsync()))
                    {
                        var responseJson = await reader.ReadToEndAsync();
                        if (!responseJson.ToLower().Contains("cache cleared"))
                            returnValue = false;
                    }
                }
            }
            catch (Exception)
            {
                returnValue = false;
            }

            return returnValue;
        }

        /// <summary>
        /// Sets item to be cached with a certain key, if key is a complex object it will be serialized into json
        /// Can bet identified by an application and bag for manual cache clearing
        /// Timeout is absolute so further grabs from cache wont affect the current timeout
        /// </summary>
        /// <param name="key">String or complex object to define the cache result key</param>
        /// <param name="value">Value to be cached</param>
        /// <param name="timeout">Timeout in seconds to hold the cache</param>
        /// <param name="bag">Current bag identifier for manual cache clearing. Default: empty string</param>
        /// <param name="application">Current application identifier for manual cache clearing. Default: application</param>
        /// <param name="sliding">Use sliding expiration instead of absolute expiration</param>
        public virtual void Set(object key, object value, long timeout = 600, string bag = null, string application = "system", bool sliding = false)
        {
            if (value == null)
                return;

            var bags = _names.GetOrAdd(application, new ConcurrentDictionary<string, bool>());

            if (bag != null)
            {
                bag.Split(',').ForEach(b => bags.GetOrAdd(b, true));
            }

            var keyString = GetKey(key);

            try
            {
                _cache.Set(keyString, value, GetPolicy(timeout, bag, application, sliding));
            }
            catch (Exception ex)
            {
                throw new Exception("Object Cacher error: ", ex);
            }
        }

        public string GetBagId(string bag)
        {
            if (string.IsNullOrEmpty(bag))
                return null;

            var cached = _cache.Get(BagKey + "|" + bag);

            if (cached == null)
                return SetBagId(bag);

            return cached.ToString();
        }

        private string SetBagId(string bag)
        {
            var guid = Guid.NewGuid();
            var bagPolicy = new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.UtcNow.AddDays(1) };
            bagPolicy.ChangeMonitors.Add(new SignaledChangeMonitor(bag));
            _cache.Set(BagKey + "|" + bag, guid, bagPolicy);
            return guid.ToString();
        }

        public Dictionary<string, List<string>> GetBags()
        {
            return _names.Keys.ToDictionary(k => k, v => _names[v].Keys.ToList());
        }

        private static bool IsNullableType(Type type)
        {
            if (!type.IsValueType) return true; // ref-type
            return Nullable.GetUnderlyingType(type) != null;
        }

        public T Get<T>(object key)
        {
            var result = _cache.Get(GetKey(key));

            // set cache mock for locking
            if (result == null)
            {
                Set(key, new object(), 1);
                return default(T);
            }

            if (result is T)
                return (T) result;

            // wait for up to 10 seconds for cache
            for (var i = 0; i < 1000; i++)
            {
                Task.Delay(10).Wait();
                result = _cache.Get(GetKey(key));
                if (result is T)
                    return (T) result;
            }

            return default(T);
        }

        protected virtual string GetBag(string bag = null)
        {
            return  bag ?? "";
        }

        public TResult GetOrSet<TParam, TResult>(TParam parameters, Func<TParam, TResult> func, long timeout = 600, string bag = null, string application = "system")
        {
            if (default(TResult) != null)
                throw new ArgumentException("Cannot call ObjectCacher.GetOrSet with a non-nullable type");

            var key = GetKey(new {parameters, func.Method});

            var result = Get<TResult>(key);
            if (result != null)
                return result;

            result = func.Invoke(parameters);

            Set(key, result, timeout, bag, application);

            return result;
        }

        public TResult GetOrSet<TResult>(Func<TResult> func, long timeout = 600, string bag = null, string application = "system")
        {
            if (default(TResult) != null)
                throw new ArgumentException("Cannot call ObjectCacher.GetOrSet with a non-nullable type");

            var key = GetKey(new { func.Method });

            var result = Get<TResult>(key);
            if (result != null)
                return result;

            result = func.Invoke();

            Set(key, result, timeout, bag, application);

            return result;
        }

        private CacheItemPolicy GetPolicy(long timeout, string bag, string application, bool sliding = true)
        {
            var policy = sliding
                ? new CacheItemPolicy { SlidingExpiration = new TimeSpan(0, (int)(timeout / 60), (int)(timeout % 60)) } 
                : new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.UtcNow.AddSeconds(timeout) };


            policy.ChangeMonitors.Add(new SignaledChangeMonitor(application));

            var bagsToUse = GetBag(bag);

            if (string.IsNullOrEmpty(bagsToUse))
                return policy;

            bagsToUse.Split(',')
                .ForEach(b => policy.ChangeMonitors.Add(new SignaledChangeMonitor(b)));

            // set ids for each bag for cache clear identification
            var unsetBags = bagsToUse.Split(',')
                .Where(b => _cache.Get(BagKey + "|" + b) == null)
                .ToList();

            foreach (var unsetBag in unsetBags)
            {
                SetBagId(unsetBag);
            }

            return policy;
        }

        protected virtual string GetKey(object key)
        {
            var keyString = JsonConvert.SerializeObject(key, new JsonSerializerSettings {TypeNameHandling = TypeNameHandling.Objects});

            return keyString;
        }

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
        public IList<T> CachedQuery<T>(string sql, dynamic param = null, string database = "master", IDbTransaction transaction = null, bool buffered = true,
            int? commandTimeout = null, CommandType? commandType = null, int cacheTimer = 600)
        {
            string key = sql + JsonConvert.SerializeObject(param ?? new { });

            var cacheData = Get<IList<T>>(key);

            if (cacheData != null)
                return cacheData;

            IList<T> dataList = null;

            using (var connection = _dataSource.CreateConnection(database))
            {
                IEnumerable<T> data = SqlMapper.Query<T>(connection, sql, param, transaction, buffered, commandTimeout, commandType);
                dataList = data.ToSafeList();
            }

            Set(key, dataList, cacheTimer);

            return dataList;
        }

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
        public async Task<IList<T>> CachedQueryAsync<T>(string sql, dynamic param = null, string database = "master", IDbTransaction transaction = null,
            int? commandTimeout = null, CommandType? commandType = null, int cacheTimer = 600)
        {
            string key = sql + JsonConvert.SerializeObject(param ?? new { });

            var cacheData = Get<IList<T>>(key);

            if (cacheData != null)
                return cacheData;

            IList<T> dataList = null;

            using (var connection = _dataSource.CreateConnection(database))
            {
                IEnumerable<T> data = await SqlMapper.QueryAsync<T>(connection, sql, param, transaction, commandTimeout, commandType);
                dataList = data.ToSafeList();
            }

            Set(key, dataList, cacheTimer);
            return dataList;
        }
    }
}
