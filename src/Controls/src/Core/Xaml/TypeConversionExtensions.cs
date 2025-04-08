#nullable disable
//
// TypeConversionExtensions.cs
//
// Author:
//       Stephane Delcroix <stephane@mi8.be>
//
// Copyright (c) 2013 Mobile Inception
// Copyright (c) 2014 Xamarin, Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using Microsoft.Maui.Controls.Xaml.Internals;

namespace Microsoft.Maui.Controls.Xaml
{
	static class TypeConversionExtensions
	{
		// caches both Type and MemberInfo keys to their corresponding TypeConverter
		static readonly ConcurrentDictionary<MemberInfo, TypeConverter> s_converterCache = new();

		internal static object ConvertTo(this object value, Type toType, Func<ParameterInfo> pinfoRetriever,
			IServiceProvider serviceProvider, out Exception exception)
		{
			Func<TypeConverter> getConverter = () =>
			{
				if (pinfoRetriever == null || pinfoRetriever() is not ParameterInfo pInfo)
					return null;

				var convertertype = pInfo.GetCustomAttribute<TypeConverterAttribute>()?.GetConverterType();
				if (convertertype == null)
					return null;
				return (TypeConverter)Activator.CreateInstance(convertertype);
			};

			return ConvertTo(value, toType, getConverter, serviceProvider, out exception);
		}

		internal static object ConvertTo(this object value, Type toType, Func<MemberInfo> minfoRetriever,
			IServiceProvider serviceProvider, out Exception exception)
		{
			Func<TypeConverter> getConverter = () =>
			{
				TypeConverter converter = null;
				if (minfoRetriever != null && minfoRetriever() is MemberInfo memberInfo)
				{
					if (TryGetTypeConverter(memberInfo, out converter))
					{
						return converter;
					}
				}

				if (TryGetTypeConverter(toType, out converter))
				{
					return converter;
				}

				return null;
			};

			return ConvertTo(value, toType, getConverter, serviceProvider, out exception);
		}

		internal static bool TryGetTypeConverter(this MemberInfo memberInfo, [NotNullWhen(true)] out TypeConverter converter)
		{
			if (!s_converterCache.TryGetValue(memberInfo, out converter))
			{
				if (memberInfo.GetCustomAttribute<TypeConverterAttribute>()?.GetConverterType() is Type converterType)
				{
					converter = (TypeConverter)Activator.CreateInstance(converterType);
				}

				// cache the result, even if it is null
				s_converterCache[memberInfo] = converter;
			}

			return converter is not null;
		}

		[return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
		static Type GetConverterType(this TypeConverterAttribute attribute)
			=> Type.GetType(attribute.ConverterTypeName);

		//Don't change the name or the signature of this, it's used by XamlC
		public static object ConvertTo(
			this object value,
			Type toType,
			[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] Type convertertype,
			IServiceProvider serviceProvider)
		{
			Exception exception = null;
			object ret = null;
			if (convertertype == null)
			{
				ret = value.ConvertTo(toType, (Func<TypeConverter>)null, serviceProvider, out exception);
				if (exception != null)
					throw exception;
				return ret;
			}
			Func<TypeConverter> getConverter = () => (TypeConverter)Activator.CreateInstance(convertertype);
			ret = value.ConvertTo(toType, getConverter, serviceProvider, out exception);
			if (exception != null)
				throw exception;
			return ret;
		}

		internal static object ConvertTo(this object value, Type toType, Func<TypeConverter> getConverter,
			IServiceProvider serviceProvider, out Exception exception)
		{
			exception = null;
			if (value == null)
				return null;

			if (value is string str)
			{
				//If there's a [TypeConverter], use it
				TypeConverter converter;
				try
				{ //minforetriver can fail
					converter = getConverter?.Invoke();
				}
				catch (Exception e)
				{
					exception = e;
					return null;
				}
				try
				{
					if (converter is IExtendedTypeConverter xfExtendedTypeConverter)
						return xfExtendedTypeConverter.ConvertFromInvariantString(str, serviceProvider);
					if (converter is TypeConverter xfTypeConverter)
						return xfTypeConverter.ConvertFromInvariantString(str);
				}
				catch (Exception e)
				{
					exception = e as XamlParseException ?? new XamlParseException($"Type converter failed: {e.Message}", serviceProvider, e);
					return null;
				}

				var ignoreCase = (serviceProvider?.GetService(typeof(IConverterOptions)) as IConverterOptions)?.IgnoreCase ?? false;

				//If the type is nullable, as the value is not null, it's safe to assume we want the built-in conversion
				if (toType.IsGenericType && toType.GetGenericTypeDefinition() == typeof(Nullable<>))
					toType = Nullable.GetUnderlyingType(toType);

				//Obvious Built-in conversions
				try
				{
					if (toType.IsEnum)
						return Enum.Parse(toType, str, ignoreCase);
					if (toType == typeof(SByte))
						return SByte.Parse(str, CultureInfo.InvariantCulture);
					if (toType == typeof(Int16))
						return Int16.Parse(str, CultureInfo.InvariantCulture);
					if (toType == typeof(Int32))
						return Int32.Parse(str, CultureInfo.InvariantCulture);
					if (toType == typeof(Int64))
						return Int64.Parse(str, CultureInfo.InvariantCulture);
					if (toType == typeof(Byte))
						return Byte.Parse(str, CultureInfo.InvariantCulture);
					if (toType == typeof(UInt16))
						return UInt16.Parse(str, CultureInfo.InvariantCulture);
					if (toType == typeof(UInt32))
						return UInt32.Parse(str, CultureInfo.InvariantCulture);
					if (toType == typeof(UInt64))
						return UInt64.Parse(str, CultureInfo.InvariantCulture);
					if (toType == typeof(Single))
						return Single.Parse(str, CultureInfo.InvariantCulture);
					if (toType == typeof(Double))
						return Double.Parse(str, CultureInfo.InvariantCulture);
					if (toType == typeof(Boolean))
						return Boolean.Parse(str);
					if (toType == typeof(TimeSpan))
						return TimeSpan.Parse(str, CultureInfo.InvariantCulture);
					if (toType == typeof(DateTime))
						return DateTime.Parse(str, CultureInfo.InvariantCulture);
					if (toType == typeof(Char))
					{
						Char.TryParse(str, out var c);
						return c;
					}
					if (toType == typeof(String) && str.StartsWith("{}", StringComparison.Ordinal))
						return str.Substring(2);
					if (toType == typeof(String))
						return value;
					if (toType == typeof(Decimal))
						return Decimal.Parse(str, CultureInfo.InvariantCulture);
				}
				catch (FormatException fe)
				{
					exception = fe;
					return null;
				}
			}

			if ((value != null && !toType.IsAssignableFrom(value.GetType())) || (value != null && toType.IsAssignableFrom(typeof(View))))
			{
				if (TypeConversionHelper.TryConvert(value, toType, out var convertedValue))
				{
					return convertedValue;
				}
			}

			var platformValueConverterService = DependencyService.Get<INativeValueConverterService>();

			object platformValue = null;
			if (platformValueConverterService != null && platformValueConverterService.ConvertTo(value, toType, out platformValue))
				return platformValue;

			return value;
		}
	}
}