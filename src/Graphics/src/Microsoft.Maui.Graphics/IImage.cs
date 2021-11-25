using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics.Platform;

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

		PlatformImage ToPlatformImage();
	}
}
