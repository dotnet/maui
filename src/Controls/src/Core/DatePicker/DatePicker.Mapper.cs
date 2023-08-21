// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable
using System;

namespace Microsoft.Maui.Controls
{
	public partial class DatePicker
	{
		[Obsolete("Use DatePickerHandler.Mapper instead.")]
		public static IPropertyMapper<IDatePicker, DatePickerHandler> ControlsDatePickerMapper = new PropertyMapper<DatePicker, DatePickerHandler>(DatePickerHandler.Mapper);

		internal static new void RemapForControls()
		{
			// Adjust the mappings to preserve Controls.DatePicker legacy behaviors
#if IOS
			DatePickerHandler.Mapper.ReplaceMapping<DatePicker, IDatePickerHandler>(PlatformConfiguration.iOSSpecific.DatePicker.UpdateModeProperty.PropertyName, MapUpdateMode);
#endif
		}
	}
}