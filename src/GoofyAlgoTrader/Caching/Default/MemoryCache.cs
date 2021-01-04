using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoofyAlgoTrader.Caching.Default
{
    public class MemoryCache : ICaching
    {
        private static readonly ConcurrentDictionary<string, Tuple<string, object, DateTime>> _cache = new ConcurrentDictionary<string, Tuple<string, object, DateTime>>();
        private const int _taskInterval = 5;

        static MemoryCache()
        {
            new Task(() =>
            {
                while (true)
                {
                    try
                    {
                        var cacheValues = _cache.Values;
                        cacheValues = cacheValues.OrderBy(p => p.Item3).ToList();
                        foreach (var cacheValue in cacheValues)
                        {
                            if (cacheValue.Item3 < DateTime.Now)
                            {
                                _cache.TryRemove(cacheValue.Item1, out Tuple<string, object, DateTime> item);
                            }
                        }
                    }
                    catch { }
                    finally
                    {
                        Task.Delay(TimeSpan.FromMinutes(_taskInterval));
                    }

                }
            }, TaskCreationOptions.LongRunning).Start();
        }

        public bool Exists(string key)
        {
            bool isSuccess = false;
            if (_cache.TryGetValue(key, out Tuple<string, object, DateTime> item))
            {
                isSuccess = item.Item3 > DateTime.Now;
            }
            return isSuccess;
        }

        public string Get(string key)
        {
            return Get<string>(key);
        }

        public T Get<T>(string key)
        {
            if (_cache.TryGetValue(key, out Tuple<string, object, DateTime> item))
            {
                if (item.Item3 > DateTime.Now && item.Item2 is T)
                {
                    return (T)item.Item2;
                }
            }
            return default(T);
        }

        public bool Set(string key, object data, TimeSpan? expiry = null)
        {
            var cacheTime = expiry.HasValue ? DateTime.Now.Add(expiry.Value) : DateTime.Now.AddYears(99);
            var cacheValue = new Tuple<string, object, DateTime>(key, data, cacheTime);
            _cache.AddOrUpdate(key, cacheValue, (k, oldValue) => cacheValue);
            return true;
        }

        public bool Remove(string key)
        {
            return _cache.TryRemove(key, out Tuple<string, object, DateTime> value);
        }
    }
}
