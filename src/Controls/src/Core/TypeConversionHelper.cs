#nullable enable

using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Hosting;

namespace Microsoft.Maui.Controls
{
	internal static class TypeConversionHelper
	{
		internal static bool TryConvert(object value, Type targetType, out object? convertedValue)
		{
			Type valueType = value.GetType();

			if (value is IWrappedValue { Value: var wrappedValue, ValueType: var wrappedType })
			{
				if (wrappedValue is null)
				{
					convertedValue = null;
					return !targetType.IsValueType || targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(Nullable<>);
				}

				// It might be enough just to unwrap the value
				if (targetType.IsAssignableFrom(wrappedType))
				{
					convertedValue = wrappedValue;
					return true;
				}

				value = wrappedValue;
				valueType = wrappedType;
			}

			if (RuntimeFeature.IsImplicitCastOperatorsUsageViaReflectionSupported)
			{
#if NET8_0
#pragma warning disable IL2026 // FeatureGuardAttribute is not supported on .NET 8
#endif
				if (TryConvertUsingImplicitCastOperator(value, targetType, out convertedValue))
#if NET8_0
#pragma warning restore IL2026 // FeatureGuardAttribute is not supported on .NET 8
#endif
				{
					return true;
				}
			}
			else
			{
				if (TryGetTypeConverter(valueType, out var converter) && converter is not null && converter.CanConvertTo(targetType))
				{
					convertedValue = converter.ConvertTo(value, targetType) ?? throw new InvalidOperationException($"The {converter.GetType()} returned null when converting {valueType} to {targetType}");
					return true;
				}

				if (TryGetTypeConverter(targetType, out converter) && converter is not null && converter.CanConvertFrom(valueType))
				{
					convertedValue = converter.ConvertFrom(value) ?? throw new InvalidOperationException($"The {converter.GetType()} returned null when converting from {valueType}");
					return true;
				}

				WarnIfImplicitOperatorIsAvailable(value, targetType);
			}

			convertedValue = null;
			return false;
		}

		private static bool TryGetTypeConverter(Type type, [NotNullWhen(true)] out TypeConverter? converter)
			=> type.TryGetTypeConverter(out converter);

		private static bool ShouldCheckForImplicitConversionOperator(Type type) =>
			type != typeof(string) && !BindableProperty.SimpleConvertTypes.ContainsKey(type);

		[RequiresUnreferencedCode("The method uses reflection to find implicit conversion operators. " +
			"It is not possible to guarantee that trimming does not remove some of the implicit operators. " +
			"Consider adding a custom TypeConverter that can perform the conversion instead.")]
		private static bool TryConvertUsingImplicitCastOperator(object value, Type targetType, [NotNullWhen(true)] out object? result)
		{
			Type valueType = value.GetType();
			MethodInfo? opImplicit = GetImplicitConversionOperator(valueType, fromType: valueType, toType: targetType)
										?? GetImplicitConversionOperator(targetType, fromType: valueType, toType: targetType);

			object? convertedValue = opImplicit?.Invoke(null, new[] { value });
			if (convertedValue is not null)
			{
				result = convertedValue;
				return true;
			}

			result = null;
			return false;

			[RequiresUnreferencedCode("The method looks for op_Implicit methods using reflection. " +
				"We cannot guarantee that the method is preserved when the code is trimmed.")]
			static MethodInfo? GetImplicitConversionOperator(Type onType, Type fromType, Type toType)
			{
				if (!ShouldCheckForImplicitConversionOperator(onType))
					return null;

				const BindingFlags bindingAttr = BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy;

				try
				{
					var method = onType.GetMethod("op_Implicit", bindingAttr, null, new[] { fromType }, null);
					if (method is not null
						&& IsImplicitOperator(method)
						&& HasSuitableReturnType(method, toType))
					{
						return method;
					}
				}
				catch (AmbiguousMatchException)
				{
					// Ignore the exception and fall back to custom filtering of all methods.
					// This happens when there are multiple implicit operators with the same parameter type,
					// but with different return types.
				}

				foreach (var method in onType.GetMethods(bindingAttr))
				{
					if (IsImplicitOperator(method)
						&& HasSuitableParameterType(method, fromType)
						&& HasSuitableReturnType(method, toType))
					{
						return method;
					}
				}

				return null;
			}

			static bool IsImplicitOperator(MethodInfo method)
				=> method.IsSpecialName
					&& method.IsPublic
					&& method.IsStatic
					&& method.Name == "op_Implicit";

			// Note: our custom type compatiblity checks are much less permissie than what .NET does internally.
			// The `IsAssignableFrom` method will for example return `false` when asking if `int` can be assigned
			// to `long`. This will filter out some valid matches and can lead to unexpected behavior.
			// The converison method has behaved this way for many years so we should probably keep it this way,
			// especially since we're trying to replace these implicit operators with type converters.

			static bool HasSuitableParameterType(MethodInfo method, Type fromType)
				=> method.GetParameters() is ParameterInfo[] parameters
					&& parameters.Length == 1
					&& parameters[0].ParameterType.IsAssignableFrom(fromType);

			static bool HasSuitableReturnType(MethodInfo method, Type toType)
				=> toType.IsAssignableFrom(method.ReturnType);
		}

		private static void WarnIfImplicitOperatorIsAvailable(object value, Type targetType)
		{
			[UnconditionalSuppressMessage("ReflectionAnalysis", "IL2026:RequiresUnreferencedCode",
				Justification = "The method tries finding implicit cast operators reflection to help developers " +
					"catch the cases where they are missing type converters during development mostly in debug builds. " +
					"The method is expected not to find anything when the app code is trimmed.")]
			bool HasImplicitOperatorConversion()
			{
				return TryConvertUsingImplicitCastOperator(value, targetType, out _);
			}

			if (HasImplicitOperatorConversion())
			{
				// If we reach this point, it means that the implicit cast operator exists, but we are not allowed to use it. This can happen for example in debug builds
				// when the app is not trimmed. This is the best effort to help developers catch the cases where they are missing type converters during development.
				// Unforutnately, we cannot just add a build warning at this moment.
				Application.Current?.FindMauiContext()?.CreateLogger(nameof(TypeConversionHelper))?.LogWarning(
					$"It is not possible to convert value of type {value.GetType()} to {targetType} via an implicit cast " +
					"because this feature is disabled. You should add a type converter that will implement this conversion and attach it to either of " +
					"these types using the [TypeConverter] attribute. Alternatively, you " +
					"can enable this feature by setting the MauiImplicitCastOperatorsUsageViaReflectionSupport MSBuild property to true in your project file. " +
					"Note: this feature is not compatible with trimming and with NativeAOT.");
			}
		}
	}
}
