// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class LayoutTests
	{
		void ValidateInputTransparentOnPlatformView(IView view)
		{
			var handler = (IPlatformViewHandler)view.ToHandler(MauiContext);

			if (handler.PlatformView is LayoutPanel lp)
			{
				Assert.True(lp.IsHitTestVisible);
			}
			else
			{
				Assert.Equal(view.InputTransparent, !handler.PlatformView.IsHitTestVisible);
			}
		}
	}
}
