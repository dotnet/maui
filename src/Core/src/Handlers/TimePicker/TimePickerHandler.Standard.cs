// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Microsoft.Maui.Handlers
{
	public partial class TimePickerHandler : ViewHandler<ITimePicker, object>
	{
		protected override object CreatePlatformView() => throw new NotImplementedException();

		public static void MapFormat(ITimePickerHandler handler, ITimePicker view) { }
		public static void MapTime(ITimePickerHandler handler, ITimePicker view) { }
		public static void MapCharacterSpacing(ITimePickerHandler handler, ITimePicker view) { }
		public static void MapFont(ITimePickerHandler handler, ITimePicker view) { }
		public static void MapTextColor(ITimePickerHandler handler, ITimePicker timePicker) { }
	}
}