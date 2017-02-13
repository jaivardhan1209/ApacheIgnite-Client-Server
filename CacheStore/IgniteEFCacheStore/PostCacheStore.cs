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
    /// <summary>
    /// Ignite Cache Store for <see cref="Post"/> entities.
    /// This class never call from Client side. This is require to serialize Cache Factory for Server End.
    /// </summary>
    public class PostCacheStore<T> : ICacheStore
    {
        public void LoadCache(Action<object, object> act, params object[] args)
        {
            return;
        }

        public object Load(object key)
        {
            Console.WriteLine("{0}.Load({1}) called.", GetType().Name, key);

            return null;
        }

        public IDictionary LoadAll(ICollection keys)
        {

            return null;
        }

        public void Write(object key, object val)
        {
            Console.WriteLine("{0}.Write({1}, {2}) called.", GetType().Name, key, val);
        }

        public void WriteAll(IDictionary entries)
        {
            
        }

        public void Delete(object key)
        {
            Console.WriteLine("{0}.Delete({1}) called.", GetType().Name, key);
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
    /// <summary>
    /// Cache Store Factory
    /// </summary>
    [Serializable]
    public class PostCacheStoreFactory : IFactory<ICacheStore>
    {
        public ICacheStore CreateInstance()
        {
            return new PostCacheStore<Post>();
        }
    }
}
