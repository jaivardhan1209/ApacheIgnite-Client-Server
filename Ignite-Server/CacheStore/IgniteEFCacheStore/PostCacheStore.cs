using System;
using System.Collections;
using System.Data.Entity.Migrations;
using System.Linq;
using Apache.Ignite.Core.Cache.Store;
using Apache.Ignite.Core.Common;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.SqlClient;

using Dapper;
using System.Reflection;

namespace IgniteEFCacheStore
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading.Tasks;

    /// <summary>
    /// Ignite Cache Store for <see cref="Post"/> entities.
    /// </summary>
    public class PostCacheStore<T> : ICacheStore
    {
        public void LoadCache(Action<object, object> act, params object[] args)
        {
             Console.WriteLine("{0}.LoadCache() called.", GetType().Name);
            var sw = Stopwatch.StartNew();
            using (IDbConnection db = new SqlConnection("Data Source=.;Initial Catalog=Test;User Id=sa;Password=$sqladmin12"))
            {
               
                var resultset = db.Query<T>("Select top 1000 * From Post").ToList();
                Console.WriteLine("Retrieved posts from DB");

                // Use custom attribute to set key using identifier Property.
                var propertyInfos = typeof(T).GetProperties();

                PropertyInfo prop = null;
                foreach( var property in propertyInfos)
                {
                    if(property.IsDefined(typeof(IgniteCacheParamAttribute)))
                    {
                        prop = property;
                    }
                }

                foreach(var result in resultset)
                {
                    act(prop.GetValue(resultset), result);
                }
            }
            sw.Stop();
            Console.WriteLine($"Loaded cache in {sw.ElapsedMilliseconds} ms");


            //for (int i = 0; i < 100000; i++)
            //{
            //    act(i, new Post
            //    {
            //        PostId = i,
            //        BlogId = i % 5,
            //        Content = "Foo" + i,
            //        Title = "Bar" + i
            //    });
            //}

            Console.WriteLine("Load cache complete");
            return;
        }

        public object Load(object key)
        {
            Console.WriteLine("{0}.Load({1}) called.", GetType().Name, key);

            //using (var ctx = GetDbContext())
            //{
            //    return ctx.Posts.Find(key);
            //}
            return null;
        }

        public IDictionary LoadAll(ICollection keys)
        {
            //using (var ctx = GetDbContext())
            //{
            //    return keys.Cast<int>().ToDictionary(key => key, key => ctx.Posts.Find(key));
            //}
            return null;
        }

        public void Write(object key, object val)
        {
            Console.WriteLine("{0}.Write({1}, {2}) called.", GetType().Name, key, val);

            DataTable dt = new DataTable();
            dt.Columns.Add("PostId", typeof(int));
            dt.Columns.Add("Title");
            dt.Columns.Add("Content1");
            dt.Columns.Add("BlogId", typeof(int));
            dt.Columns.Add("IsRequired", typeof(bool));
            var drow = dt.NewRow();

            var source = val as Post;
            drow["PostId"] = source.PostId;
            drow["Title"] = source.Title;
            drow["Content1"] = source.Content;
            drow["BlogId"] = source.PostId;

            dt.Rows.Add(drow);

            using (
                IDbConnection db =
                    new SqlConnection("Data Source=IS-GNV-HPDB02;Initial Catalog=Test;User Id=tnpsapp;Password=tnpsapp"))
            {

                var param = new DynamicParameters(new { PostSource = dt });
                db.Execute("dbo.UpsertPost", param, commandType: CommandType.StoredProcedure);

            }
        }

        public void WriteAll(IDictionary entries)
        {

            Console.WriteLine("Write behind api is called");
            var dt = new DataTable();
            dt.Columns.Add("PostId", typeof(int));
            dt.Columns.Add("Title");
            dt.Columns.Add("Content1");
            dt.Columns.Add("BlogId", typeof(int));

            foreach (var keys in entries.Keys)
            {
                var drow = dt.NewRow();

                var source = entries[keys] as Post;
                drow["PostId"] = source?.PostId;
                drow["Title"] = source?.Title;
                drow["Content1"] = source?.Content;
                drow["BlogId"] = source?.PostId;

                dt.Rows.Add(drow);
            }
            using (
                IDbConnection db =
                    new SqlConnection("Data Source=IS-GNV-HPDB02;Initial Catalog=Test;User Id=tnpsapp;Password=tnpsapp"))
            {

                var param = new DynamicParameters(new { PostSource = dt });
                var result = db.Query("dbo.UpsertPost", param, commandType: CommandType.StoredProcedure);

            }
        }

        public void Delete(object key)
        {
            Console.WriteLine("{0}.Delete({1}) called.", GetType().Name, key);

            using (IDbConnection db = new SqlConnection("Data Source=IS-GNV-HPDB02;Initial Catalog=Test;User Id=tnpsapp;Password=tnpsapp"))
            {

                var query = $"Delete From Post where [PostId] = {key}";

                db.Execute(query);

            }
        }

        public void DeleteAll(ICollection keys)
        {
            foreach (var key in keys)
            {
                Delete(key);
            }
        }

        public void SessionEnd(bool commit)
        {
            // No-op.
        }
    }

    [Serializable]
    public class PostCacheStoreFactory<T> : IFactory<ICacheStore>
    {
        public ICacheStore CreateInstance()
        {
            return new PostCacheStore<T>();
        }
    }
}
