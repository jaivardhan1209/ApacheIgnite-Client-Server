using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Apache.Ignite.Linq;


namespace IgniteEFCacheStore
{
    public static class ExpressionTreeClass
    {
        public static Expression<Func<ICacheEntry<int, T>, bool>> FilterExpression()
        {
            ParameterExpression pe = Expression.Parameter(typeof(ICacheEntry<int, Post>), "s");

            MemberExpression me = Expression.Property(pe, "PostId");

            ConstantExpression constant = Expression.Constant(2, typeof(int));

            BinaryExpression body = Expression.GreaterThanOrEqual(me, constant);

            var ExpressionTree = Expression.Lambda<Func<ICacheEntry<int, T>, bool>>(body, new[] { pe });

            return ExpressionTree;
        }
    }
}
