// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Layouts;

namespace Microsoft.Maui.Controls
{
	public abstract partial class Layout
	{
		internal static new void RemapForControls()
		{
		}

		[Obsolete("Use LayoutHandler.Mapper instead.")]
		public static IPropertyMapper<IView, IViewHandler> ControlsLayoutMapper = new PropertyMapper<IView, IViewHandler>(ControlsVisualElementMapper);
	}
}
