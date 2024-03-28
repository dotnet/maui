#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Maui.Hosting
{
	public static class TypeConversionAppBuilderExtensions
	{
		private static Dictionary<Type, TypeConverter>? s_typeConverters;

		public static MauiAppBuilder ConfigureTypeConversions(this MauiAppBuilder builder, Action<ITypeConversionBuilder> configureDelegate)
		{
			var typeConversionBuilder = new TypeConversionBuilder();

			configureDelegate(typeConversionBuilder);

			foreach (var kvp in typeConversionBuilder.Converters)
			{
				s_typeConverters ??= new();
				s_typeConverters[kvp.Key] = kvp.Value;
			}

			return builder;
		}

		internal static bool TryGetTypeConverter(Type type, [NotNullWhen(true)] out TypeConverter? typeConverter)
		{
			typeConverter = null;
			return s_typeConverters?.TryGetValue(type, out typeConverter) ?? false;
		}

		private class TypeConversionBuilder : ITypeConversionBuilder
		{
			internal Dictionary<Type, TypeConverter> Converters { get; } = new();

			public ITypeConversionBuilder AddTypeConverter<T, TConverter>()
				where TConverter : TypeConverter, new()
			{
				Converters[typeof(T)] = new TConverter();
				return this;
			}

			public ITypeConversionBuilder AddTypeConverter<T>(Func<TypeConverter> createConverter)
			{
				Converters[typeof(T)] = createConverter();
				return this;
			}
		}
	}

	public interface ITypeConversionBuilder
	{
		ITypeConversionBuilder AddTypeConverter<T, TConverter>() where TConverter : TypeConverter, new();
		ITypeConversionBuilder AddTypeConverter<T>(Func<TypeConverter> createConverter);
	}
}
