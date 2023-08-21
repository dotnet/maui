// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ShapeViewHandlerTests
	{
		MauiShapeView GetPlatformShapeView(ShapeViewHandler shapeViewHandler) =>
			shapeViewHandler.PlatformView;

		Task ValidateNativeFill(IShapeView shapeView, Color color)
		{
			return InvokeOnMainThreadAsync(() =>
			{
				return GetPlatformShapeView(CreateHandler(shapeView)).AssertContainsColor(color, MauiContext);
			});
		}
	}
}