using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Shared.Clock;

namespace Shared.Repository.Extensions
{
    public static class QueryableExtensions
    {
        static LambdaExpression CreateExpression(Type type, string propertyName)
        {
            var param = Expression.Parameter(type, "x");
            Expression body = param;
            foreach (var member in propertyName.Split('.'))
            {
                body = Expression.PropertyOrField(body, member);
            }
            return Expression.Lambda(body, param);
        }

        public static IQueryable<T> OrderBy<T>(this IQueryable<T> source, string propertyName)
        {
            return (IQueryable<T>)OrderBy((IQueryable)source, propertyName);
        }

        public static IQueryable OrderBy(this IQueryable source, string propertyName)
        {
            var selector = CreateExpression(source.ElementType, propertyName);

            return source.Provider.CreateQuery(
                Expression.Call(typeof(Queryable), "OrderBy", new Type[] {
                        source.ElementType, selector.Body.Type },
                    source.Expression, selector
                ));
        }

        public static IQueryable<T> OrderByDescending<T>(this IQueryable<T> source, string propertyName)
        {
            return (IQueryable<T>)OrderByDescending((IQueryable)source, propertyName);
        }

        public static IQueryable OrderByDescending(this IQueryable source, string propertyName)
        {
            var selector = CreateExpression(source.ElementType, propertyName);

            return source.Provider.CreateQuery(
                Expression.Call(typeof(Queryable), "OrderByDescending", new Type[] {
                        source.ElementType, selector.Body.Type },
                    source.Expression, selector
                ));
        }

        private static readonly ConcurrentDictionary<string, Tuple<DateTime, int>> _cachedCountMap =
            new ConcurrentDictionary<string, Tuple<DateTime, int>>();
        public static async Task<int> CachedCountAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            string name = "",
            CancellationToken cancellationToken = default)
        {
            if (_cachedCountMap.TryGetValue(name, out var value))
            {
                if (value.Item1 > AppClock.UtcNow)
                {
                    return value.Item2;
                }
            }
            var count = await source.CountAsync(cancellationToken);
            _cachedCountMap[name] = new Tuple<DateTime, int>(AppClock.UtcNow + TimeSpan.FromMinutes(1), count);

            return count;
        }
    }
}
