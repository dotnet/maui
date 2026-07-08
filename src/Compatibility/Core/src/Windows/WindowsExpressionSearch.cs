using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
{
	internal sealed class WindowsExpressionSearch : IExpressionSearch
	{
		List<object> _results;
		Type _targeType;

		public List<T> FindObjects<T>(Expression expression) where T : class
		{
			_results = new List<object>();
			_targeType = typeof(T);

			Visit(expression);

			List<T> final = _results.Cast<T>().ToList();
			_results = null;
			return final;
		}

		void Visit(Expression expression)
		{
			if (expression == null)
				return;

			switch (expression.NodeType)
			{
				case ExpressionType.Negate:
				case ExpressionType.NegateChecked:
				case ExpressionType.Not:
				case ExpressionType.Convert:
				case ExpressionType.ConvertChecked:
				case ExpressionType.ArrayLength:
				case ExpressionType.Quote:
				case ExpressionType.TypeAs:
				case ExpressionType.UnaryPlus:
					Visit(((UnaryExpression)expression).Operand);
					break;
				case ExpressionType.Add:
				case ExpressionType.AddChecked:
				case ExpressionType.Subtract:
				case ExpressionType.SubtractChecked:
				case ExpressionType.Multiply:
				case ExpressionType.MultiplyChecked:
				case ExpressionType.Divide:
				case ExpressionType.Modulo:
				case ExpressionType.Power:
				case ExpressionType.And:
				case ExpressionType.AndAlso:
				case ExpressionType.Or:
				case ExpressionType.OrElse:
				case ExpressionType.LessThan:
				case ExpressionType.LessThanOrEqual:
				case ExpressionType.GreaterThan:
				case ExpressionType.GreaterThanOrEqual:
				case ExpressionType.Equal:
				case ExpressionType.NotEqual:
				case ExpressionType.Coalesce:
				case ExpressionType.ArrayIndex:
				case ExpressionType.RightShift:
				case ExpressionType.LeftShift:
				case ExpressionType.ExclusiveOr:
					var binary = (BinaryExpression)expression;
					Visit(binary.Left);
					Visit(binary.Right);
					Visit(binary.Conversion);
					break;
				case ExpressionType.TypeIs:
					Visit(((TypeBinaryExpression)expression).Expression);
					break;
				case ExpressionType.Conditional:
					var conditional = (ConditionalExpression)expression;
					Visit(conditional.Test);
					Visit(conditional.IfTrue);
					Visit(conditional.IfFalse);
					break;
				case ExpressionType.MemberAccess:
					VisitMemberAccess((MemberExpression)expression);
					break;
				case ExpressionType.Call:
					var methodCall = (MethodCallExpression)expression;
					Visit(methodCall.Object);
					VisitList(methodCall.Arguments, Visit);
					break;
				case ExpressionType.Lambda:
					Visit(((LambdaExpression)expression).Body);
					break;
				case ExpressionType.New:
					VisitList(((NewExpression)expression).Arguments, Visit);
					break;
				case ExpressionType.NewArrayInit:
				case ExpressionType.NewArrayBounds:
					VisitList(((NewArrayExpression)expression).Expressions, Visit);
					break;
				case ExpressionType.Invoke:
					var invocation = (InvocationExpression)expression;
					VisitList(invocation.Arguments, Visit);
					Visit(invocation.Expression);
					break;
				case ExpressionType.MemberInit:
					var init = (MemberInitExpression)expression;
					VisitList(init.NewExpression.Arguments, Visit);
					VisitList(init.Bindings, VisitBinding);
					break;
				case ExpressionType.ListInit:
					var init1 = (ListInitExpression)expression;
					VisitList(init1.NewExpression.Arguments, Visit);
					VisitList(init1.Initializers, initializer => VisitList(initializer.Arguments, Visit));
					break;
				case ExpressionType.Constant:
					break;
				default:
					throw new ArgumentException(string.Format("Unhandled expression type: '{0}'", expression.NodeType));
			}
		}

		void VisitBinding(MemberBinding binding)
		{
			switch (binding.BindingType)
			{
				case MemberBindingType.Assignment:
					Visit(((MemberAssignment)binding).Expression);
					break;
				case MemberBindingType.MemberBinding:
					VisitList(((MemberMemberBinding)binding).Bindings, VisitBinding);
					break;
				case MemberBindingType.ListBinding:
					VisitList(((MemberListBinding)binding).Initializers, initializer => VisitList(initializer.Arguments, Visit));
					break;
				default:
					throw new ArgumentException(string.Format("Unhandled binding type '{0}'", binding.BindingType));
			}
		}

		static void VisitList<TList>(IEnumerable<TList> list, Action<TList> visitor)
		{
			foreach (TList element in list)
			{
				visitor(element);
			}
		}

		// All important magic happens here
		void VisitMemberAccess(MemberExpression member)
		{
			if (member.Expression is ConstantExpression && member.Member is FieldInfo)
			{
				object container = ((ConstantExpression)member.Expression).Value;
				object value = ((FieldInfo)member.Member).GetValue(container);

				if (ReflectionExtensions.IsInstanceOfType(_targeType, value))
				{
					_results.Add(value);
				}
			}
			Visit(member.Expression);
		}
	}
}