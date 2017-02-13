using System;
using System.Linq;
using Apache.Ignite.Core;
using Apache.Ignite.Core.Cache.Configuration;

namespace IgniteEFCacheStore
{
    using System.Diagnostics;
    using System.Linq.Expressions;

    using Apache.Ignite.Core.Binary;
    using Apache.Ignite.Core.Cache;
    using Apache.Ignite.Core.Discovery.Tcp;
    using Apache.Ignite.Core.Discovery.Tcp.Static;
    using Apache.Ignite.Linq;
    using System.Reflection;
    using System.Collections.Generic;
    using Apache.Ignite.Core.Transactions;

    public static class Program
    {

        /// <summary>
        /// Create expression tree for Filteration
        /// </summary>
        /// <returns></returns>
        public static Expression<Func<ICacheEntry<int, Post>, bool>> FilterExpression()
        {
            

            ParameterExpression pe = Expression.Parameter(typeof(ICacheEntry<int, Post>), "s");

            MemberExpression me = Expression.Property(Expression.Property(pe, "Value"), "Title");

            //ToLower expression
            MethodInfo toLowerMethodInfo = typeof(string).GetMethod("ToLower", new Type[] { });
            Expression toLowerCall = Expression.Call(me, toLowerMethodInfo);

            MethodInfo method = typeof(string).GetMethod("Contains", new[] { typeof(string) });

            // Applying filter for check contains of two records (jai, vipin)
            var firstValue = Expression.Constant("Jai".ToLower(), typeof(string));

            var secondValue = Expression.Constant("Vipin".ToLower(), typeof(string));

            // Pass ToLowerCall to  
            var containsMethodExp = Expression.Call(toLowerCall, method, firstValue);

            var containsMethodExp1 = Expression.Call(toLowerCall, method, secondValue);

            List<Expression> expList = new List<Expression> { containsMethodExp, containsMethodExp1 };

            var finalAggExpression = expList.Skip(1).Aggregate(expList.First(), Expression.Or);

            return Expression.Lambda<Func<ICacheEntry<int, Post>, bool>>(finalAggExpression, pe);
        }

        /// <summary>
        /// Create expression for Sorting (but not recommended)
        /// </summary>
        /// <returns></returns>
        public static Expression<Func<ICacheEntry<int, Post>, int>> SortExpression()
        {
            //var type = typeof(Post);
            //var property = type.GetProperty("PostId");

            //ParameterExpression pe = Expression.Parameter(typeof(ICacheEntry<int, Post>), "s");

            //var propertyAccess = Expression.MakeMemberAccess(pe, property);

            //var orderByExp = Expression.Lambda(propertyAccess, pe);

            //var typeArguments = new Type[] { type, property.PropertyType };
            //var methodName = "OrderBy";
            //var resultExp = Expression.Call(typeof(Queryable), methodName, typeArguments, source.Expression, Expression.Quote(orderByExp));

            return null;
            //return Expression.Lambda<Func<ICacheEntry<int, Post>, bool>>(resultExp, pe);

        }
        /// <summary>
        /// Main function to operate on Ignite
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {

            // Ignite can be start using app.config file.
            var ignite = Ignition.Start(
                            new IgniteConfiguration
                            {
                                GridName = "HaulplanClient",
                                ClientMode = true,
                                DiscoverySpi = new TcpDiscoverySpi
                                {
                                    IpFinder = new TcpDiscoveryStaticIpFinder
                                    {
                                        Endpoints = new[]
                                                                    {
                                                                        "127.0.0.1:47500"
                                                                    },
                                        
                                    },
                                    SocketTimeout =
                                                    TimeSpan.FromSeconds(0.3)
                                },
                                BinaryConfiguration =
                                        new BinaryConfiguration(typeof(Post))
                            });


            var posts = ignite.GetCache<int, Post>("posts");

            var stopwatch = new Stopwatch();

            if (!posts.AsCacheQueryable().Any())
            {
                stopwatch.Start();
                Console.WriteLine("Calling LoadCache from Client side");
                posts.LoadCache(null);
                stopwatch.Stop();
                Console.WriteLine($"Total size of cache {0} loaded in {stopwatch.ElapsedMilliseconds} ms");
            }

            var testrecord = new Post
            {
                PostId = 1,
                Title = "New Year 2017",

            };

            try
            {
                posts.Put(1, testrecord);
            }
            catch
            {
                Console.WriteLine("Update Exception occure");
            }

            // set the same reference for Post in PostCacheStore.cs file
            Console.WriteLine("\n>>> Example started\n\n");

            // Create Expression for Filter
            var expressionFunc = FilterExpression();

            // Not recommended for multi Column sort. 
            // Multicolun sort need to work on Iorder set
            var sortExpression = SortExpression();

            // time to perform Filteration and getting 10 record from Filter.
            stopwatch.Restart();

            var resultSet1 = posts.AsCacheQueryable().Where(expressionFunc).Take(10).ToList();
            
            Console.WriteLine($"Total number of record after filter: {resultSet1.Count()}");
            stopwatch.Stop();
            Console.ReadLine();
            }
        }
    }
