using System;
using System.Collections.Concurrent;
using System.Globalization;
using Microsoft.Extensions.Localization;
using MoreLinq;
using Newtonsoft.Json;

namespace Stacy.Core.Data
{
    public class AsyncBatchResult<TResult>
    {
        public string Error { get; set; }
        public string InternalError { get; set; }
        public TResult Result { get; set; }

        [JsonIgnore]
        public Exception Exception { get; set; }

        [JsonIgnore]
        public Task<TResult> Task { get; set; }
    }

    public class AsyncBatchStatus<TResult>
    {
        public bool Complete { get; set; }
        public string Guid { get; set; }

        public string OverallError { get; set; }

        public Dictionary<string, AsyncBatchResult<TResult>> Results { get; set; }
    }

    public class AsyncBatchService<TResult>
    {
        private readonly IStringLocalizer _localizer;
        private readonly IObjectCacher _cacher;

        public AsyncBatchService(IObjectCacher cacher, IStringLocalizer localizer)
        {
            _cacher = cacher;
            _localizer = localizer;
        }

        public AsyncBatchStatus<TResult> Start(Dictionary<string, Task<TResult>> tasks)
        {
            var guid = Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture);

            var bag = new ConcurrentDictionary<string, Task<TResult>>(tasks);

            _cacher.Set(guid, bag);

            return new AsyncBatchStatus<TResult>
            {
                Complete = false,
                Guid = guid,
                Results = new Dictionary<string, AsyncBatchResult<TResult>>()
            };
        }
        
        public AsyncBatchStatus<TResult> Peek(string guid)
        {
            var bag = _cacher.Get<ConcurrentDictionary<string, Task<TResult>>>(guid);

            if (bag == null)
                throw new ArgumentException(_localizer["Invalid GUID"]);

            var status = new AsyncBatchStatus<TResult>
            {
                Complete = false,
                Results = new Dictionary<string, AsyncBatchResult<TResult>>(),
                Guid = guid
            };

            if (bag.Count == 0)
            {
                status.Complete = true;
                return status;
            }

            var completedTasks = bag.Where(x => x.Value.IsCompleted)
                .ToDictionary(t => t.Key, t => PrepareResult(t.Value));

            if (!completedTasks.Any())
                return status;

            status.Results = completedTasks;

            Task<TResult> placeholder;
            completedTasks.Keys.ForEach(k => bag.TryRemove(k, out placeholder));

            if (bag.Count == 0)
                status.Complete = true;

            return status;
        }

        private static AsyncBatchResult<TResult> PrepareResult(Task<TResult> task)
        {
            var result = new AsyncBatchResult<TResult>
            {
                Task = task,
                Result = !task.IsFaulted ? task.Result : default(TResult),
                Error = task.Exception?.Message ?? "",
                Exception = task.Exception
            };

            while (result.Exception is AggregateException)
            {
                result.Exception = ((AggregateException)result.Exception).InnerExceptions.FirstOrDefault();
                result.Error = result.Exception?.Message ?? result.Error;
                result.InternalError = result.Exception?.GetBaseException()?.Message ?? result.Error;
            }

            return result;
        }
    }
}

