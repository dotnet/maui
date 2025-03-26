// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using System.Reflection;

namespace Microsoft.Maui.Controls;

/// <summary>
/// Type converter for converting instances of other types to and from <see cref="CultureInfo"/>.
/// </summary>
/// <remarks>
/// This class differs in two ways from System.ComponentModel.CultureInfoConverter, the default type converter 
/// for the CultureInfo class. First, it uses a string representation based on the IetfLanguageTag property 
/// rather than the Name property (i.e., RFC 3066 rather than RFC 1766). Second, when converting from a string, 
/// the properties of the resulting CultureInfo object depend only on the string and not on user overrides set 
/// in the operating system. This makes it possible to create documents the appearance of which do not depend on 
/// local settings.
/// </remarks>
// This class is based on the WPF code for this class, which is licensed under the MIT license.
public class CultureInfoIetfLanguageTagConverter : TypeConverter
{
	/// <summary>
	/// Returns whether or not this class can convert from a given type.
	/// </summary>
	/// <returns>
	/// <see langword="true"/> if this converter can convert from the provided type, <see langword="false"/> if not.
	/// </returns>
	/// <param name="context">The ITypeDescriptorContext for this call.</param>
	/// <param name="sourceType">The Type being queried for support.</param>
	public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
		=> sourceType == typeof(string);

	/// <summary>
	/// Returns whether or not this class can convert to a given type.
	/// </summary>
	/// <returns>
	/// <see langword="true"/> if this converter can convert from the provided type, <see langword="false"/> if not.
	/// </returns>
	/// <param name="context"> The ITypeDescriptorContext for this call.</param>
	/// <param name="destinationType"> The Type being queried for support.</param>
	public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
		=> destinationType == typeof(string);

	/// <summary>
	/// Attempt to convert to a <see cref="CultureInfo"/> from the given object
	/// </summary>
	/// <returns>
	/// A CultureInfo object based on the specified culture name.
	/// </returns>
	/// <exception cref="ArgumentNullException">
	/// An ArgumentNullException is thrown if the example object is null.
	/// </exception>
	/// <exception cref="ArgumentException">
	/// An ArgumentException is thrown if the example object is not null and is not a valid type
	/// which can be converted to a CultureInfo.
	/// </exception>
	/// <param name="context">The ITypeDescriptorContext for this call.</param>
	/// <param name="culture">The CultureInfo which is respected when converting.</param>
	/// <param name="value">The object to convert to a CultureInfo.</param>
	public override object ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object? value)
	{
		if (value is string cultureName)
		{
			return CultureInfo.GetCultureInfoByIetfLanguageTag(cultureName);
		}

		throw GetConvertFromException(value);
	}

	/// <summary>
	/// Attempt to convert a <see cref="CultureInfo"/> to the given type
	/// </summary>
	/// <returns>
	/// The object which was constructed.
	/// </returns>
	/// <exception cref="ArgumentNullException">
	/// An ArgumentNullException is thrown if the example object is <see langword="null"/>.
	/// </exception>
	/// <exception cref="ArgumentException">
	/// An ArgumentException is thrown if the example object is not <see langword="null"/> and is not a <see cref="CultureInfo"/>,
	/// or if the destinationType isn't one of the valid destination types.
	/// </exception>
	/// <param name="context"> The ITypeDescriptorContext for this call. </param>
	/// <param name="culture"> The CultureInfo which is respected when converting. </param>
	/// <param name="value"> The double to convert. </param>
	/// <param name="destinationType">The type to which to convert the CultureInfo. </param>
	public override object ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type? destinationType)
	{
		if (destinationType is null)
		{
			throw new ArgumentException("destinationType is null", nameof(destinationType));
		}

		if (value is CultureInfo cultureInfo)
		{
			if (destinationType == typeof(string))
			{
				return cultureInfo.IetfLanguageTag;
			}
			else if (destinationType == typeof(InstanceDescriptor))
			{
				MethodInfo? method = typeof(CultureInfo).GetMethod(
					"GetCultureInfo",
					BindingFlags.Static | BindingFlags.InvokeMethod | BindingFlags.Public,
					null, // use default binder
					[typeof(string)],
					null  // default binder doesn't use parameter modifiers
					);

				return new InstanceDescriptor(method, new object[] { cultureInfo.Name });
			}
		}

		throw GetConvertToException(value, destinationType);
	}
}