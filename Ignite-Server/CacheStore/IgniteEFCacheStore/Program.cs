using System;
using Apache.Ignite.Core;
using Apache.Ignite.Core.Cache.Configuration;
using Apache.Ignite.Core.Discovery.Tcp;
using NLog.Config;
using NLog.Targets;
using NLog;
using Apache.Ignite.NLog;
using Apache.Ignite.Core.Binary;

namespace IgniteEFCacheStore
{


    public static class Program
    {
        public static void Main(string[] args)
        {
            var nlogConfig = new LoggingConfiguration();
            var fileTarget = new FileTarget
            {
                FileName = "ignite_nlog.log"
            };
            nlogConfig.AddTarget("logfile", fileTarget);
            nlogConfig.LoggingRules.Add(new LoggingRule("*", LogLevel.Trace, fileTarget));
            LogManager.Configuration = nlogConfig;

            var igniteConfig = new IgniteConfiguration
            {
                Logger = new IgniteNLogLogger(),
                GridName = "HaulplanServer",
                DiscoverySpi = new TcpDiscoverySpi
                {

                    LocalPort = 47500
                },
                // IncludedEventTypes = EventType.CacheAll,
                BinaryConfiguration = new BinaryConfiguration(typeof(Post), typeof(Blog)),
                //JvmInitialMemoryMb = 2048,
                //JvmMaxMemoryMb = 4096,

            };
            
            using (var ignite = Ignition.Start(igniteConfig))
            {
                // The Cache Configuration can be moved to app.config file
                var posts = ignite.CreateCache<int, Post>(new CacheConfiguration
                {
                    QueryEntities = new[] { new QueryEntity(typeof(int), typeof(Post)) },
                    Name = "posts",
                    CacheStoreFactory = new PostCacheStoreFactory<Post>(),
                    ReadThrough = true,
                    WriteThrough = true,
                    KeepBinaryInStore = true,   // Store works with concrete classes.
                    WriteBehindEnabled = false,
                    // WriteBehindBatchSize = 2048,
                    WriteBehindFlushFrequency = TimeSpan.FromSeconds(5),
                });

                Console.ReadLine();
            }
        }
    }

    /// <summary>
    /// The cache param attribute. 
    /// This custom attribute can help you to create your own Key for CacheStore.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class IgniteCacheParamAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CacheParamAttribute"/> class.
        /// </summary>
        public IgniteCacheParamAttribute()
        {
        }
    }
}
