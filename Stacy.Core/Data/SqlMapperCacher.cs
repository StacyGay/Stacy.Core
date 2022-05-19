/*
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Caching;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Newtonsoft.Json;

namespace Stacy.Core.Data
{
    public static class SqlMapperCacher
    {
        /// <summary>
        /// Caches a query for value given in cacheTimer in seconds
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cnn"></param>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <param name="transaction"></param>
        /// <param name="buffered"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="commandType"></param>
        /// <param name="cacheTimer">Timespan to cache query in seconds</param>
        /// <returns></returns>
        public static IEnumerable<T> CachedQuery<T>(
            this IDbConnection cnn, string sql, dynamic param = null, IDbTransaction transaction = null, bool buffered = true,
            int? commandTimeout = null, CommandType? commandType = null, int cacheTimer = 0
            )
        {
            if (cacheTimer == 0)
                return SqlMapper.Query<T>(cnn, sql, param, transaction, buffered, commandTimeout, commandType);

            string key = sql + JsonConvert.SerializeObject(param ?? new { });

            var memCache = MemoryCache.Default;
            var cacheData = memCache.Get(key) as IEnumerable<T>;

            if (cacheData != null)
                return cacheData;

            var data = SqlMapper.Query<T>(cnn, sql, param, transaction, buffered, commandTimeout, commandType);

            var cachePolicy = new CacheItemPolicy();
            cachePolicy.AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(cacheTimer);

            memCache.Add(key, data, cachePolicy);
            return data;
        }

        public static async Task<IEnumerable<T>> CachedQueryAsync<T>(
            this IDbConnection cnn, string sql, dynamic param = null, IDbTransaction transaction = null,
            int? commandTimeout = null, CommandType? commandType = null, int cacheTimer = 0
            )
        {

            if (cacheTimer == 0)
                return SqlMapper.QueryAsync<T>(cnn, sql, param, transaction, commandTimeout, commandType);

            string key = sql + JsonConvert.SerializeObject(param ?? new { });

            var memCache = MemoryCache.Default;
            var cacheData = memCache.Get(key) as IEnumerable<T>;

            if (cacheData != null)
                return cacheData;

            var data = await SqlMapper.QueryAsync<T>(cnn, sql, param, transaction, commandTimeout, commandType);

            var cachePolicy = new CacheItemPolicy {AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(cacheTimer)};

            memCache.Add(key, data, cachePolicy);
            return data;
        }
    }
}
*/
