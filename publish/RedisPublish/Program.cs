using StackExchange.Redis;
using System;
using System.Threading;

namespace RedisPublish
{   
    class Program
    {
        private ConnectionMultiplexer redisConnection;

        static void Main(string[] args)
        {    
            using (var con = ConnectionMultiplexer.Connect("localhost:6383"))
            {
                var sub = con.GetSubscriber();
               
                for (int i=0; i < 1000000000; i++)
                {
                    Console.WriteLine(i);
                    sub.Publish("topic", "SendMessage : " + i);  
                }   
            }

        }
    }
}
