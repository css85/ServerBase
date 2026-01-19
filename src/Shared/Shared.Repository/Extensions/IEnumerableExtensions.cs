using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Shared.Repository.Extensions
{
    public static class IEnumerableExtensions
    {
        static PropertyInfo GetProperty(Type type, string propertyName)
        {
            var propertyNameSplits = propertyName.Split('.');

            if (propertyNameSplits.Length == 0)
            {
                return type.GetProperty(propertyName);
            }
            else
            {
                PropertyInfo property = null;
                foreach (var propertyNameSplit in propertyNameSplits)
                {
                    property = type.GetProperty(propertyNameSplit);
                    type = property.GetType();
                }

                return property;
            }
        }

        public static IOrderedEnumerable<T> OrderBy<T>(this ICollection<T> source, string propertyName)
        {
            var property = GetProperty(typeof(T), propertyName);
            return source.OrderBy(p => property.GetValue(p, null));
        }

        public static IOrderedEnumerable<T> OrderBy<T>(this IEnumerable<T> source, string propertyName)
        {
            var property = GetProperty(typeof(T), propertyName);
            return source.OrderBy(p => property.GetValue(p, null));
        }

        public static IOrderedEnumerable<T> OrderByDescending<T>(this ICollection<T> source, string propertyName)
        {
            var property = GetProperty(typeof(T), propertyName);
            return source.OrderByDescending(p => property.GetValue(p, null));
        }

        public static IOrderedEnumerable<T> OrderByDescending<T>(this IEnumerable<T> source, string propertyName)
        {
            var property = GetProperty(typeof(T), propertyName);
            return source.OrderByDescending(p => property.GetValue(p, null));
        }
    }
}
