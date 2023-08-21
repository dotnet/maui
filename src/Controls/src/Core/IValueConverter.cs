// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable
using System;
using System.Globalization;

namespace Microsoft.Maui.Controls
{
	public interface IValueConverter
	{
		object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture);
		object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture);
	}
}