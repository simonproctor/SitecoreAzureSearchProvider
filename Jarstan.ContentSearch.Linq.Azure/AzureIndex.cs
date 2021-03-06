﻿using Sitecore.ContentSearch.Linq.Indexing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sitecore.ContentSearch.Linq.Common;
using Sitecore.ContentSearch.Linq.Parsing;
using Jarstan.ContentSearch.Linq.Azure;
using Sitecore.ContentSearch;
using Jarstan.ContentSearch.Linq.Parsing;

namespace Jarstan.ContentSearch.Linq.Azure
{
    public class AzureIndex<TItem> : Index<TItem, AzureQuery>
    {
        public AzureIndex(AzureIndexParameters parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException("parameters");
            QueryMapper = new AzureQueryMapper(parameters);
            QueryOptimizer = new AzureQueryOptimizer();
            FieldNameTranslator = parameters.FieldNameTranslator;
            Parameters = parameters;
        }

        public AzureIndexParameters Parameters { get; }

        protected override FieldNameTranslator FieldNameTranslator { get; }

        protected override QueryMapper<AzureQuery> QueryMapper { get; }

        protected override IQueryOptimizer QueryOptimizer { get; }

        protected override IIndexValueFormatter ValueFormatter { get; }

        public override IQueryable<TItem> GetQueryable()
        {
            IQueryable<TItem> queryable = new AzureGenericQueryable<TItem, AzureQuery>(this, QueryMapper, QueryOptimizer, FieldNameTranslator);
            (queryable as IHasTraceWriter).TraceWriter = ((IHasTraceWriter)this).TraceWriter;
            var list = GetTypeInheritance(typeof(TItem)).SelectMany((Type t) => t.GetCustomAttributes(typeof(IPredefinedQueryAttribute), true).Cast<IPredefinedQueryAttribute>()).ToList();
            foreach (IPredefinedQueryAttribute current in list)
            {
                queryable = current.ApplyFilter(queryable, ValueFormatter);
            }
            return queryable;
        }

        private IEnumerable<Type> GetTypeInheritance(Type type)
        {
            yield return type;
            Type baseType = type.BaseType;
            while (baseType != null)
            {
                yield return baseType;
                baseType = baseType.BaseType;
            }
            yield break;
        }

        public override TResult Execute<TResult>(AzureQuery query)
        {
            return default(TResult);
        }

        public override IEnumerable<TElement> FindElements<TElement>(AzureQuery query)
        {
            return new List<TElement>();
        }
    }
}
