﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace PinoyCode.Data.Extensions
{
    public static class QueryExtensions
    {
        public static PagedList<T> PagedList<T>(this IQueryable<T> query, 
            int pageIndex, 
            int pageSize, 
            string searchByPropertyName, 
            string searchByText, 
            string orderByPropertyName, 
            bool orderByDescending) where T : class
        {
            return new PagedList<T>(query)
            {
                PageSize = pageSize,
                PageIndex = pageIndex,
                SearchByPropertyName = searchByPropertyName,
                SearchByText = searchByText,
                OrderByPropertyName = orderByPropertyName,
                OrderByDescending = orderByDescending
            };
        }

        //makes expression for specific prop
        public static Expression<Func<TSource, object>> GetExpression<TSource>(string propertyName)
        {
            var param = Expression.Parameter(typeof(TSource), "x");
            Expression conversion = Expression.Convert(Expression.Property
            (param, propertyName), typeof(object));   //important to use the Expression.Convert
            return Expression.Lambda<Func<TSource, object>>(conversion, param);
        }

        //makes deleget for specific prop
        public static Func<TSource, object> GetFunc<TSource>(string propertyName)
        {
            return GetExpression<TSource>(propertyName).Compile();  //only need compiled expression
        }

        //OrderBy overload
        public static IOrderedEnumerable<TSource>
        OrderBy<TSource>(this IEnumerable<TSource> source, string propertyName)
        {
            return source.OrderBy(GetFunc<TSource>(propertyName));
        }

        //OrderBy overload
        public static IOrderedQueryable<TSource>
        OrderBy<TSource>(this IQueryable<TSource> source, string propertyName)
        {
            return source.OrderBy(GetExpression<TSource>(propertyName));
        }


        //OrderByDescending overload
        public static IOrderedEnumerable<TSource>
        OrderByDescending<TSource>(this IEnumerable<TSource> source, string propertyName)
        {
            return source.OrderByDescending(GetFunc<TSource>(propertyName));
        }

        //OrderByDescending overload
        public static IOrderedQueryable<TSource>
        OrderByDescending<TSource>(this IQueryable<TSource> source, string propertyName)
        {
            return source.OrderByDescending(GetExpression<TSource>(propertyName));
        }
    }
}
