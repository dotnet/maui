#nullable enable
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
	/// <summary>
	/// Represents a <see cref="Paint"/> that fills an area with the contents of an <see cref="IImageSource"/>.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Platform backends receive backgrounds as a <see cref="Paint"/> through <see cref="IView.Background"/>.
	/// Pattern matching that paint against this interface is the supported way to detect an image-source
	/// background and to obtain the <see cref="ImageSource"/> so it can be resolved with an
	/// <see cref="IImageSourceServiceProvider"/>.
	/// </para>
	/// <para>
	/// This interface is implemented by the paint that .NET MAUI produces for an image background, and it may
	/// also be implemented by custom <see cref="Paint"/> types so that they are treated as image backgrounds by
	/// the built-in handlers.
	/// </para>
	/// <example>
	/// The following example shows how an out-of-tree handler can render an image background:
	/// <code language="csharp"><![CDATA[
	/// public static void MapBackground(IViewHandler handler, IView view)
	/// {
	///     if (view.Background is IImageSourcePaint imagePaint)
	///     {
	///         var provider = handler.GetRequiredService<IImageSourceServiceProvider>();
	///         ApplyImageBackgroundAsync(handler.PlatformView, imagePaint.ImageSource, provider);
	///     }
	///     else
	///     {
	///         ApplyPaintBackground(handler.PlatformView, view.Background);
	///     }
	/// }
	/// ]]></code>
	/// </example>
	/// </remarks>
	public interface IImageSourcePaint
	{
		/// <summary>
		/// Gets the image source that is used to fill the area.
		/// </summary>
		IImageSource? ImageSource { get; }
	}
}
