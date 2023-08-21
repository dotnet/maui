// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Globalization;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Converters;

namespace Controls.Sample.Converters
{
	public class ThicknessConverter : IValueConverter
	{
		readonly ThicknessTypeConverter _converter = new();

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			try
			{
				return _converter.ConvertFrom(value);
			}
			catch
			{
				return Thickness.Zero;
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
			_converter.ConvertTo(value, targetType);
	}
}