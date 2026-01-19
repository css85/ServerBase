using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using WebTool.Base.DataTables;
using Shared.Repository.Extensions;

namespace WebTool.Extensions
{
    public static class DataTablesExtensions
    {
        public static bool TryGetFormValue<T>(this IFormCollection form, string name, out T value)
        {
            if (form.TryGetValue(name, out var stringValues))
            {
                var stringValue = stringValues.FirstOrDefault();
                if (stringValue != null)
                {
                    value = (T) Convert.ChangeType(stringValue, typeof(T));
                    return true;
                }
            }

            value = default;
            return false;
        }

        public static T GetFormValue<T>(this IFormCollection form, string name)
        {
            if (form.TryGetValue(name, out var stringValues))
            {
                var stringValue = stringValues.FirstOrDefault();
                if (stringValue != null)
                {
                    return (T)Convert.ChangeType(stringValue, typeof(T));
                }
            }

            throw new Exception($"{name} not found.");
        }

        public static ICollection<T> ApplyOrder<T>(this DataTablesInput input, IEnumerable<T> source, int defaultOrderIndex = -1)
        {
            IOrderedEnumerable<T> orderedCollection = null;

            var isOrderId = false;
            foreach (var order in input.orders)
            {
                if (order != null &&
                    order.dir != DataTablesOrderType.None)
                {
                    var propertyName = input.columnInfos[order.index].PropertyName;
                    if (string.IsNullOrEmpty(propertyName) == false)
                    {
                        orderedCollection = order.dir == DataTablesOrderType.Asc
                            ? (orderedCollection ?? source).OrderBy(propertyName)
                            : (orderedCollection ?? source).OrderByDescending(propertyName);

                        if (order.index == defaultOrderIndex)
                            isOrderId = true;
                    }
                }
            }

            if (defaultOrderIndex != -1 && isOrderId == false)
            {
                var propertyName = input.columnInfos[defaultOrderIndex].PropertyName;
                orderedCollection = (orderedCollection ?? source).OrderBy(propertyName);
            }

            return (orderedCollection ?? source).ToArray();
        }

        public static ICollection<T> ApplyOrder<T>(this DataTablesInput input, ICollection<T> source, int defaultOrderIndex = -1)
        {
            IOrderedEnumerable<T> orderedCollection = null;

            var isOrderId = false;
            foreach (var order in input.orders)
            {
                if (order != null &&
                    order.dir != DataTablesOrderType.None)
                {
                    var propertyName = input.columnInfos[order.index].PropertyName;
                    if (string.IsNullOrEmpty(propertyName) == false)
                    {
                        orderedCollection = order.dir == DataTablesOrderType.Asc
                            ? (orderedCollection ?? (IEnumerable<T>)source).OrderBy(propertyName)
                            : (orderedCollection ?? (IEnumerable<T>)source).OrderByDescending(propertyName);

                        if (order.index == defaultOrderIndex)
                            isOrderId = true;
                    }
                }
            }

            if (defaultOrderIndex != -1 && isOrderId == false)
            {
                var propertyName = input.columnInfos[defaultOrderIndex].PropertyName;
                orderedCollection = (orderedCollection ?? (IEnumerable<T>) source).OrderBy(propertyName);
            }

            return orderedCollection?.ToArray() ?? source;
        }

        public static IQueryable<T> ApplyOrder<T>(this DataTablesInput input, IQueryable<T> source)
        {
            foreach (var order in input.orders)
            {
                if (order != null &&
                    order.dir != DataTablesOrderType.None)
                {
                    var propertyName = input.columnInfos[order.index].PropertyName;
                    if (string.IsNullOrEmpty(propertyName) == false)
                    {
                        return order.dir == DataTablesOrderType.Asc
                            ? source.OrderBy(propertyName)
                            : source.OrderByDescending(propertyName);
                    }
                }
            }

            return source;
        }

        public static IEnumerable<T> ApplyLimit<T>(this DataTablesInput input, IEnumerable<T> source)
        {
            return source.Skip(input.start)
                .Take(input.length);
        }

        public static ICollection<T> ApplyLimit<T>(this DataTablesInput input, ICollection<T> source)
        {
            return source.Skip(input.start)
                .Take(input.length).ToArray();
        }

        public static IQueryable<T> ApplyLimit<T>(this DataTablesInput input, IQueryable<T> source)
        {
            return source.Skip(input.start)
                .Take(input.length);
        }

        public static void ApplyData<T>(this DataTablesInput input, DataTablesOutput output, IEnumerable<T> source,
            Func<T, string[]> selector)
        {
            output.data = source.Select(selector).ToArray();
        }

        public static async Task ApplyDataAsync<TSource>(this DataTablesInput input, DataTablesOutput output,
            IQueryable<TSource> source, Expression<Func<TSource, string[]>> selector)
        {
            output.data = await source.Select(selector).ToArrayAsync();
        }

        public static void ApplyCount(this DataTablesInput input, DataTablesOutput output, int count)
        {
            output.recordsFiltered = output.recordsTotal = count;
        }

        public static void ApplyCount<T>(this DataTablesInput input, DataTablesOutput output, ICollection<T> source)
        {
            output.recordsTotal = source.Count;
            output.recordsFiltered = output.recordsTotal;
        }

        public static async Task ApplyCountAsync<T>(this DataTablesInput input, DataTablesOutput output, IQueryable<T> source, string name = "")
        {
            if (string.IsNullOrEmpty(name))
            {
                output.recordsTotal = await source.CountAsync();
            }
            else
            {
                output.recordsTotal = await source.CachedCountAsync(name);
            }
            output.recordsFiltered = output.recordsTotal;
        }

        public static IEnumerable<T> ApplyFilter<T>(this DataTablesInput input, DataTablesOutput output, IEnumerable<T> source,
            Func<T, string, bool> predicate)
        {
            foreach (var searchValue in input.searchValues)
            {
                source = source.Where(p => predicate(p, searchValue));
            }

            return source;
        }

        public static void ApplyFilter<T>(this DataTablesInput input, DataTablesOutput output, ICollection<T> source,
            Action<string> action)
        {
            if (input.searchValues.Length > 0)
            {
                foreach (var searchValue in input.searchValues)
                {
                    action.Invoke(searchValue);
                }

                output.recordsFiltered = source.Count;
            }
        }

        public static async Task ApplyFilterAsync<T>(this DataTablesInput input, DataTablesOutput output, IQueryable<T> source,
            Action<string> action, string name = "")
        {
            if (input.searchValues.Length > 0)
            {
                foreach (var searchValue in input.searchValues)
                {
                    action.Invoke(searchValue);
                }

                if (string.IsNullOrEmpty(name))
                {
                    output.recordsFiltered = await source.CountAsync();
                }
                else
                {
                    output.recordsFiltered = await source.CachedCountAsync(name);
                }
            }
        }
    }
}
