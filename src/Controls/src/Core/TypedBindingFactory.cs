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

			bool compileSetter = setter is null && mode != BindingMode.OneWay && mode != BindingMode.OneTime;
			bool compileHandlers = mode != BindingMode.OneWayToSource && mode != BindingMode.OneTime;

			Func<object?, (TProperty, bool)>? getterAccumulator = null;
			Action<object?, TProperty>? setterAccumulator = null;
			List<Tuple<Func<object?, object?>, string?>>? handlersAccumulator = null;

			Expression? expression = getter.Body;

			while (expression is not null)
			{
				(Expression? nextExpression, Func<object?, object?> getNextPart) = Process<TProperty>(expression!);

				getterAccumulator = getterAccumulator is null
					? CreateInnermostGetter<TProperty>(getNextPart)
					: Compose<TProperty>(getterAccumulator, getNextPart);

				if (compileSetter)
				{
					setterAccumulator = setterAccumulator is null
						? CreateInnermostSetter<TProperty>(expression)
						: Compose<TProperty>(setterAccumulator, getNextPart);
				}

				if (compileHandlers)
				{
					handlersAccumulator ??= new();
					handlersAccumulator.Add(new(getNextPart, GetPartName(expression)));
				}

				expression = nextExpression;
			}

			if (getterAccumulator is null)
			{
				throw new InvalidOperationException("Cannot compile expression.");
			}

			Func<TSource, (TProperty, bool)> compiledGetter = (TSource source) => getterAccumulator(source);
			Action<TSource, TProperty>? compiledSetter = compileSetter ? (source, value) => setterAccumulator?.Invoke(source, value) : null;
			Tuple<Func<TSource, object?>, string>[]? compiledHandlers = compileHandlers ? CompileHandlers<TSource>(handlersAccumulator) : null;

			return new TypedBinding<TSource, TProperty>(compiledGetter, setter ?? compiledSetter, compiledHandlers)
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

		private static ValueTuple<Expression?, Func<object?, object?>> Process<TProperty>(Expression expression)
		{
			return expression switch
			{
				ParameterExpression => (null, Identity),
				UnaryExpression {
					NodeType: ExpressionType.Convert or ExpressionType.TypeAs,
					Operand: var parent } =>
						(parent, Identity),
				MemberExpression {
					Member: PropertyInfo { CanRead: true } property,
					Expression: var parent } =>
						(parent, property.GetValue),
				MemberExpression {
					Member: FieldInfo field,
					Expression: var parent } =>
						(parent, field.GetValue),
				MethodCallExpression {
					Object: var parent,
					Method: MethodInfo { IsSpecialName: true, Name: "get_Indexer" or "get_Item" } method,
					Arguments: { Count: 1 } arguments }
					when arguments[0] is ConstantExpression { Value: object index } =>
						(parent, CreateIndexerGetter(method, index)),
				BinaryExpression {
					Left: var parent,
					Right: ConstantExpression { Value: int index } } =>
						(parent, (target) => (target as Array)?.GetValue(index)),
				_ => throw new InvalidOperationException($"Unsupported expression: {expression}"),
			};

			static object? Identity(object? value) => value;
		}

		private static Func<object?, object?> CreateIndexerGetter(MethodInfo indexer, object index)
			=> (target) =>
			{
				try
				{
					return indexer.Invoke(target, [index]);
				}
				catch (TargetInvocationException ex) when (ex.InnerException is KeyNotFoundException)
				{
					return null;
				}
			};

		private static Func<object?, (TProperty, bool)> CreateInnermostGetter<TProperty>(Func<object?, object?> getter)
			=> (object? source) => getter(source) switch
			{
				TProperty value => (value, true),
				null => (default!, IsNullValidValue(typeof(TProperty))),
				_ => (default!, false),
			};

		private static bool IsNullValidValue(Type type)
			=> !type.IsValueType || type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);

		private static Func<object?, (TProperty, bool)> Compose<TProperty>(Func<object?, (TProperty, bool)> previous, Func<object?, object?> next)
			=> (object? source) => next(source) switch
			{
				object value => previous(value),
				_ => (default!, false),
			};

		private static Action<object?, TProperty> Compose<TProperty>(Action<object?, TProperty>? previous, Func<object?, object?> next)
			=> (object? source, TProperty value) => previous?.Invoke(next(source), value);

		private static Action<object?, TProperty> CreateInnermostSetter<TProperty>(Expression expression)
		{
			Action<object, object?> setter = expression switch
			{
				ParameterExpression => CreateDirectSetter(),
				UnaryExpression { NodeType: ExpressionType.Convert or ExpressionType.TypeAs } => CreateDirectSetter(),
				MemberExpression { Member: PropertyInfo { CanWrite: false } property } =>
					throw new InvalidOperationException($"Cannot create setter for read-only property {property}"),
				MemberExpression { Member: PropertyInfo property } => property.SetValue,
				MemberExpression { Member: FieldInfo { IsInitOnly: true } field } =>
					throw new InvalidOperationException($"Cannot create setter for a read-only field {field}"),
				MemberExpression { Member: FieldInfo field } => field.SetValue,
				BinaryExpression { Right: ConstantExpression { Value: int index } } => CreateArraySetter(index),
				_ => throw new InvalidOperationException($"Cannot create setter for {expression} ({expression.GetType()})"),
			};

			return (source, value) =>
			{
				if (source is not null)
				{
					setter(source, value);
				}
			};

			static Action<object, object?> CreateDirectSetter()
				=> (source, value) => source = value!;

			static Action<object, object?> CreateArraySetter(int index)
				=> (source, value) =>
				{
					if (source is not Array array)
					{
						throw new InvalidOperationException($"Cannot set value because the target property is not an array.");
					}

					array.SetValue(value, index);
				};
		}

		private static Tuple<Func<TSource, object?>, string>[] CompileHandlers<TSource>(List<Tuple<Func<object?, object?>, string?>>? handlers)
		{
			if (handlers is null || handlers.Count == 0)
			{
				return Array.Empty<Tuple<Func<TSource, object?>, string>>();
			}

			int handlersCount = 0;
			for (int i = handlers.Count - 2; i >= 0; i--)
			{
				if (handlers[i].Item2 is not null)
				{
					handlersCount++;
				}
			}

			var compiledHandlers = new Tuple<Func<TSource, object?>, string>[handlersCount];
			var getter = (TSource source) => handlers[handlers.Count - 1].Item1(source);
			int nextHandlerIndex = 0;

			for (int i = handlers.Count - 2; i >= 0; i--)
			{
				if (handlers[i].Item2 is string name)
				{
					compiledHandlers[nextHandlerIndex++] = new(getter, name);
				}

				var previous = getter;
				var next = handlers[i].Item1;
				getter = (TSource source) => next(previous(source));
			}

			return compiledHandlers;
		}

		private static string? GetPartName(Expression expression)
		{
			return expression switch
			{
				MemberExpression { Member: PropertyInfo property } => property.Name,
				MemberExpression { Member: FieldInfo field } => field.Name,
				MethodCallExpression {
					Method: MethodInfo { IsSpecialName: true, Name: "get_Indexer" or "get_Item" } method,
					Arguments: { Count: 1 } arguments }
					when arguments[0] is ConstantExpression { Value: object index } => 
						GetIndexerName(method.DeclaringType, index),
				BinaryExpression {
					Left: MemberExpression { Member: { DeclaringType: Type declaringType } },
					Right: ConstantExpression { Value: int index } } =>
						GetIndexerName(declaringType, index),
				_ => null,
			};

			static string GetIndexerName(Type? declaringType, object index)
			{
				var defaultMemberName = declaringType?.GetCustomAttribute<DefaultMemberAttribute>(inherit: true)?.MemberName ?? "Item";
				return $"{defaultMemberName}[{index}]";
			}
		}
	}
}
