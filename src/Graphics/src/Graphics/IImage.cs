// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.IO;
using System.Threading.Tasks;

namespace Microsoft.Maui.Graphics
{
	public enum ResizeMode
	{
		Fit,
		Bleed,
		Stretch
	}

	public interface IImage : IDrawable, IDisposable
	{
		float Width { get; }
		float Height { get; }
		IImage Downsize(float maxWidthOrHeight, bool disposeOriginal = false);
		IImage Downsize(float maxWidth, float maxHeight, bool disposeOriginal = false);
		IImage Resize(float width, float height, ResizeMode resizeMode = ResizeMode.Fit, bool disposeOriginal = false);
		void Save(Stream stream, ImageFormat format = ImageFormat.Png, float quality = 1);
		Task SaveAsync(Stream stream, ImageFormat format = ImageFormat.Png, float quality = 1);
		IImage ToPlatformImage();
	}
}
