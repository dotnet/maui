// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	public interface ICustomImageSourceStub : IImageSource
	{
		Color Color { get; }
	}

	public partial class CustomImageSourceStub : ImageSourceStub, ICustomImageSourceStub
	{
		public CustomImageSourceStub()
		{
		}

		public CustomImageSourceStub(Color color)
		{
			Color = color;
		}

		public Color Color { get; set; }
	}
}