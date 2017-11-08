using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Platform.GTK
{
    internal class GtkExpressionSearch : ExpressionVisitor, IExpressionSearch
    {
        private List<object> _results;
        private Type _targetType;

        public List<T> FindObjects<T>(Expression expression) where T : class
        {
            _results = new List<object>();
            _targetType = typeof(T);
            Visit(expression);

            return _results.Select(o => o as T).ToList();
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.Expression is ConstantExpression && node.Member is FieldInfo)
            {
                object container = ((ConstantExpression)node.Expression).Value;
                object value = ((FieldInfo)node.Member).GetValue(container);

                if (_targetType.IsInstanceOfType(value))
                    _results.Add(value);
            }

            return base.VisitMember(node);
        }
    }
}
