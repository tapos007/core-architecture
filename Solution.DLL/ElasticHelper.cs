using System;
using System.Collections.Generic;
using System.Text;
using Nest;

namespace Solution.DLL
{

    public interface IElasticHelper
    {
        ElasticClient ElasticClient { get; set; }
    }
   public  class ElasticHelper :IElasticHelper
    {
        public ElasticClient ElasticClient { get; set; }
        public ElasticClient DBClient { get; set; }
        public ElasticHelper()
        {
            var settings = new ConnectionSettings(new Uri("http://127.0.0.1:9200"))
                .DefaultIndex("people");

            var settings1 = new ConnectionSettings(new Uri("http://127.0.0.1:9200"))
                .DefaultIndex("avcd");

            ElasticClient = new ElasticClient(settings);
            DBClient = new ElasticClient(settings1);
        }
    }
}
