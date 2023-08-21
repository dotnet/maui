// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Microsoft.Maui.Controls.Platform;

namespace Microsoft.Maui.Controls
{
	public partial class Picker
	{
		public static void MapHorizontalOptions(IPickerHandler handler, Picker picker)
		{
			handler.PlatformView?.UpdateHorizontalOptions(picker);
		}

		public static void MapVerticalOptions(IPickerHandler handler, Picker picker)
		{
			handler.PlatformView?.UpdateVerticalOptions(picker);
		}
	}
}