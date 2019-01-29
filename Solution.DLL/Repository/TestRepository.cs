using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace Solution.DLL.Repository
{
    public interface ITestRepository
    {
        Task<string> getData(string key);

        void setData(string key);
        Task<Person> insertObject(Person person);

        Task<bool> addDataElastic();
    }
    public class TestRepository : ITestRepository
    {
        private readonly IDistributedCache _cache;
        private readonly IElasticHelper _elasticHelper;

        public TestRepository(IDistributedCache cache, IElasticHelper elasticHelper)
        {
            _cache = cache;
            _elasticHelper = elasticHelper;
        }

        public async Task<string> getData(string key)
        {
            await _cache.SetStringAsync(key, "tapos");
            return await _cache.GetStringAsync(key);
        }

        public void setData(string key)
        {
            _cache.SetString(key, "Tapos",
          new DistributedCacheEntryOptions { SlidingExpiration = TimeSpan.MaxValue });
        }

        public async Task<Person> insertObject(Person person)
        {
            await _cache.SetStringAsync("person", Newtonsoft.Json.JsonConvert.SerializeObject(person));

            return JsonConvert.DeserializeObject<Person>(await _cache.GetStringAsync("person"));


        }


        public async Task<bool> addDataElastic()
        {
            var person = new Person
            {

                Name = "Martijn",
                Email = "Laarman"
            };

            var indexResponse = _elasticHelper.ElasticClient.IndexDocument(person);

            var asyncIndexResponse = await _elasticHelper.ElasticClient.IndexDocumentAsync(person);
            return true;
        }
    }
}
