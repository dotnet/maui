// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable
using System;
using Microsoft.Maui.Controls.Platform;
using Microsoft.UI.Xaml.Input;

namespace Microsoft.Maui.Controls.Platform.Compatibility
{
	public class KeyboardConverter : Microsoft.UI.Xaml.Data.IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			var keyboard = value as Keyboard;
			if (keyboard == null)
				return null;


			var result = new InputScope();
			result.Names.Add(keyboard.ToInputScopeName());
			return result;
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new NotImplementedException();
		}
	}
}