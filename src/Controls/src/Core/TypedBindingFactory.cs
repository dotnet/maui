#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	internal static class TypedBindingFactory
	{
		internal static BindingBase Create<TSource, TProperty>(
			Expression<Func<TSource, TProperty>> getter,
			Action<TSource, TProperty>? setter = null,
			BindingMode mode = BindingMode.Default,
			IValueConverter? converter = null,
			object? converterParameter = null,
			string? stringFormat = null,
			object? source = null,
			object? targetNullValue = null,
			object? fallbackValue = null,
			string? updateSourceEventName = null)
		{
			if (getter is null)
				throw new ArgumentNullException(nameof(getter));

			Stack<Expression> expressionStack = Preprocess((getter as LambdaExpression)?.Body)
				?? throw new ArgumentException("Unsupported expression", nameof(getter));

			Func<TSource, (TProperty, bool)> _getter = CreateGetter<TSource, TProperty>(expressionStack);

			if (mode != BindingMode.OneWay && mode != BindingMode.OneTime && setter is null)
			{
				setter = CreateSetter<TSource, TProperty>(expressionStack)
					?? throw new ArgumentException($"Cannot generate setter from getter expression '{getter}' for {mode} binding.", nameof(setter));
			}

			Tuple<Func<TSource, object?>, string>[]? handlers = null;
			if (mode != BindingMode.OneTime)
			{
				handlers = CreateHandlers<TSource, TProperty>(expressionStack);
			}

			return new TypedBinding<TSource, TProperty>(_getter, setter, handlers)
			{
				Mode = mode,
				Converter = converter,
				ConverterParameter = converterParameter,
				StringFormat = stringFormat,
				Source = source,
				UpdateSourceEventName = updateSourceEventName,
				TargetNullValue = targetNullValue,
				FallbackValue = fallbackValue,
			};
		}

		private static Stack<Expression>? Preprocess(Expression? expression)
		{
			Stack<Expression>? stack = null;

			while (expression is not null)
			{
				stack ??= new();
				stack.Push(expression);

				expression = expression switch
				{
					ParameterExpression => null,
					UnaryExpression { NodeType: ExpressionType.Convert or ExpressionType.TypeAs, Operand: var next } => next,
					MemberExpression { Member: not null, Expression: var next } => next,
					MethodCallExpression {
						Method: MethodInfo { IsSpecialName: true, Name: "get_Indexer" or "get_Item" },
						Arguments: { Count: 1 },
						Object: var next } => next,
					BinaryExpression { Left: var next, Right: ConstantExpression { Value: int _ } } => next,
					_ => throw new ArgumentException($"Unsupported expression ({expression?.GetType()})", nameof(expression)),
				};
			}

			return stack;
		}

		private static Func<TSource, (TProperty, bool)> CreateGetter<TSource, TProperty>(Stack<Expression> expressionStack)
		{
			Func<TSource, (object?, bool)> getter = CreateNestedGetter<TSource, TProperty>(expressionStack.SkipLast(1));
			Func<object, (object?, bool)> finalGetter = CreateGetter(expressionStack.Last());

			return (TSource source) =>
			{
				var (maybeValue, success) = getter(source);
				if (!success)
				{
					return (default!, false);
				}

				(maybeValue, _) = finalGetter(maybeValue!);
				return maybeValue switch
				{
					TProperty value => (value, true),
					null => (default!, true),
					_ => (default!, false),
				};
			};
		}

		private static Action<TSource, TProperty>? CreateSetter<TSource, TProperty>(Stack<Expression> expressionStack)
		{
			Func<TSource, (object?, bool)> getter = CreateNestedGetter<TSource, TProperty>(expressionStack.SkipLast(1));
			Action<object, TProperty>? setter = CreateSetter<TProperty>(expressionStack.Last());
			if (setter is null)
			{
				return null;
			}

			return (TSource source, TProperty value) =>
			{
				var (nestedSource, success) = getter(source);
				if (success && nestedSource is not null)
				{
					setter(nestedSource, value);
				}
			};
		}


		private static Func<TSource, (object?, bool)> CreateNestedGetter<TSource, TProperty>(IEnumerable<Expression> expressionStack)
		{
			Func<TSource, (object?, bool)> getter = static (source) => (source, true);
			foreach (var expresisonPart in expressionStack)
			{
				var previousGetter = getter;
				var partGetter = CreateGetter(expresisonPart);
				getter = (source) => previousGetter(source) switch
				{
					(object value, true) => partGetter(value),
					_ => (null, false),
				};
			}

			return getter;
		}

		private static Func<object, (object?, bool)> CreateGetter(Expression expression)
		{
			return expression switch
			{
				ParameterExpression =>
					static (target) => WrapResult(target),
				MemberExpression { Member: PropertyInfo property } =>
					(target) => WrapResult(property.GetValue(target)),
				MemberExpression { Member: FieldInfo field } =>
					(target) => WrapResult(field.GetValue(target)),
				MethodCallExpression { Method: MethodInfo { IsSpecialName: true, Name: "get_Indexer" or "get_Item" } method, Arguments: { Count: 1 } arguments }
					when arguments[0] is ConstantExpression { Value: object index } =>
						CreateIndexerGetter(method, index),
				UnaryExpression { NodeType: ExpressionType.Convert or ExpressionType.TypeAs } =>
					static (target) => WrapResult(target),
				BinaryExpression { Left: MemberExpression, Right: ConstantExpression { Value: int index } } =>
					(target) => WrapResult((target as Array)?.GetValue(index)),
				_ => throw new InvalidOperationException($"Cannot create getter for {expression} ({expression.GetType()})"),
			};

			static (object?, bool) WrapResult(object? maybeValue)
				=> maybeValue is object value ? (value, true) : (null, false);

			static Func<object, (object?, bool)> CreateIndexerGetter(MethodInfo indexer, object index)
				=> (target) =>
				{
					try
					{
						return WrapResult(indexer.Invoke(target, [index]));
					}
					catch (TargetInvocationException ex) when (ex.InnerException is KeyNotFoundException)
					{
						return (null, false);
					}
				};
		}

		static Action<object, TProperty>? CreateSetter<TProperty>(Expression expression)
			=> expression switch
			{
				ParameterExpression => static (source, value) => source = value!,
				UnaryExpression { NodeType: ExpressionType.Convert or ExpressionType.TypeAs } => static (source, value) => source = value!,
				MemberExpression { Member: PropertyInfo property } => TryCreateSetPropertyValue<TProperty>(property),
				MemberExpression { Member: FieldInfo field } => TryCreateSetFieldValue<TProperty>(field),
				BinaryExpression { Left: MemberExpression, Right: ConstantExpression { Value: int index } } => CreateSetArrayValue<TProperty>(index),
				_ => null,
			};

		static Action<object, TProperty>? TryCreateSetPropertyValue<TProperty>(PropertyInfo property)
			=> property.CanWrite
				? (source, value) => property.SetValue(source, value)
				: null;

		static Action<object, TProperty>? TryCreateSetFieldValue<TProperty>(FieldInfo field)
			=> !field.IsInitOnly
				? (source, value) => field.SetValue(source, value)
				: null;

		static Action<object, TProperty>? CreateSetArrayValue<TProperty>(int index)
			=> (source, value) =>
			{
				if (source is not Array array)
				{
					throw new InvalidOperationException($"Cannot set value because the target property is not an array.");
				}

				array.SetValue(value, index);
			};

		private static Tuple<Func<TSource, object?>, string>[] CreateHandlers<TSource, TProperty>(Stack<Expression> expressionStack)
		{
			var handlers = new List<Tuple<Func<TSource, object?>, string>>(expressionStack.Count);

			var firstStep = expressionStack.FirstOrDefault();
			if (firstStep is not ParameterExpression)
			{
				throw new InvalidOperationException($"The getter expression is invalid.");
			}

			Func<TSource, object?> getHandler = static (source) => source;

			foreach (var getterStep in expressionStack.Skip(1))
			{
				var (getNextPart, propertyName) = CreateHandler(getterStep, getHandler);

				handlers.Add(new(getHandler, propertyName));
				getHandler = getNextPart;
			}

			return handlers.ToArray();

			static (Func<TSource, object?>, string) CreateHandler(Expression getterStep, Func<TSource, object?> getPreviousHandler)
			{
				return getterStep switch
				{
					MemberExpression { Member: PropertyInfo property } =>
						((source) => property.GetValue(getPreviousHandler(source)), property.Name),
					MemberExpression { Member: FieldInfo field } =>
						((source) => field.GetValue(getPreviousHandler(source)), field.Name),
					UnaryExpression { NodeType: ExpressionType.Convert or ExpressionType.TypeAs } =>
						((source) => getPreviousHandler(source), string.Empty),
					BinaryExpression { Left: MemberExpression { Member: { DeclaringType: Type declaringType } }, Right: ConstantExpression { Value: int index } } =>
						((source) => (getPreviousHandler(source) as Array)?.GetValue(index), GetIndexerPropertyName(declaringType, index)),
					MethodCallExpression { Method: MethodInfo { IsSpecialName: true, Name: "get_Indexer" or "get_Item" } method, Arguments: { Count: 1 } arguments }
						when arguments[0] is ConstantExpression { Value: object argument } =>
							(CreateIndexerHandler(method, argument, getPreviousHandler), GetIndexerPropertyName(method.DeclaringType, argument)),
					_ => throw new InvalidOperationException($"Unsupported expression: {getterStep} ({getterStep.GetType()})"),
				};
			}

			static Func<TSource, object?> CreateIndexerHandler(MethodInfo indexer, object index, Func<TSource, object?> getPreviousHandler)
			{
				return (source) =>
				{
					try
					{
						return indexer.Invoke(getPreviousHandler(source), [index]);
					}
					catch (TargetInvocationException ex) when (ex.InnerException is KeyNotFoundException)
					{
						return null;
					}
				};
			}

			static string GetIndexerPropertyName(Type? declaringType, object index)
			{
				var defaultMemberName = declaringType?.GetCustomAttribute<DefaultMemberAttribute>(inherit: true)?.MemberName ?? "Item";
				return $"{defaultMemberName}[{index}]";
			}
		}
	}

#if NETSTANDARD2_0
	internal static class StackExtensions
	{
		internal static IEnumerable<T> SkipLast<T>(this Stack<T> expressionStack, int count)
			=> expressionStack.Take(expressionStack.Count - count);
	}
#endif
}
