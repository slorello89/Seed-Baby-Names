using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StackExchange.Redis;

namespace SeedBabyNamesDb
{
    class Program
    {
        async static Task Main(string[] args)
        {
            var json = System.IO.File.ReadAllText("babynames.json");
            var obj = JsonConvert.DeserializeObject<JObject>(json);

            const string HOST_NAME = "HOST_NAME";
            const string PASSWORD = "PASSWORD";

            var redis = await ConnectionMultiplexer.ConnectAsync($"{HOST_NAME},password={PASSWORD}");
            var db = redis.GetDatabase();
            var batch = db.CreateBatch();
            var tasks = new List<Task<RedisResult>>();

            tasks.Add(batch.ExecuteAsync("CMS.INITBYDIM", "baby-names", 1000, 10));
            var rand = new Random();

            foreach(var gender in new[]{"girls","boys"})
            {
                for(var i = 0; i <50000; i++)
                {
                    var nameObj = obj[gender][rand.Next(500)];

                    tasks.Add(batch.ExecuteAsync("CMS.INCRBY", $"baby-names", nameObj.ToString(), 1 ));                    
                }
            }

            batch.Execute();
            batch.WaitAll(tasks.ToArray());
        }
    }
}
