#nullable enable
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
	/// <summary>
	/// Exposes the <see cref="IImageSource"/> of a <see cref="Paint"/> that .NET MAUI uses to fill an area
	/// with an image.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This is a <b>consumption-only</b> contract. It exists so that code receiving a <see cref="Paint"/> -
	/// most commonly a platform backend handling <see cref="IView.Background"/> - can recognize an image
	/// background and read its image source without reflection.
	/// </para>
	/// <para>
	/// <b>Implementing this interface outside of .NET MAUI is not supported.</b> .NET MAUI reserves the right
	/// to add members to this interface in future releases, which would be a breaking change for external
	/// implementers. Only paints created by .NET MAUI are guaranteed to be recognized and rendered by the
	/// built-in handlers; a custom <see cref="Paint"/> implementing this interface is not guaranteed to be
	/// honored, because not every built-in handler routes backgrounds through the image-source path.
	/// </para>
	/// <para>
	/// Values implementing this interface are obtained by pattern matching an existing <see cref="Paint"/>,
	/// typically from <see cref="IView.Background"/>. In .NET MAUI a paint of this kind is produced when a
	/// background is set from an image - for example a <c>Microsoft.Maui.Controls.ImageBrush</c>, or
	/// <c>Page.BackgroundImageSource</c>.
	/// </para>
	/// <para>
	/// <see cref="ImageSource"/> can be <see langword="null"/>, which represents an image background with
	/// nothing to draw. Treat it the same as having no image background: clear any previously applied image
	/// rather than attempting to resolve it. A non-<see langword="null"/> value should be resolved through an
	/// <see cref="IImageSourceServiceProvider"/>; note that resolution is asynchronous and may still yield no
	/// image.
	/// </para>
	/// <para>
	/// This is distinct from <see cref="ImagePaint"/>. <see cref="ImagePaint"/> carries an already-loaded
	/// <see cref="IImage"/> for drawing operations, whereas this contract carries an unresolved
	/// <see cref="IImageSource"/> that describes where an image comes from (a file, URI, stream, or font glyph)
	/// and must be loaded through an image source service. A paint will not implement both.
	/// </para>
	/// <example>
	/// The following example shows how a platform backend can render an image background:
	/// <code language="csharp"><![CDATA[
	/// public static void MapBackground(IViewHandler handler, IView view)
	/// {
	///     if (view.Background is IImageSourcePaint imagePaint)
	///     {
	///         // May be null, in which case any existing image background is cleared.
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
		/// Gets the image source used to fill the area, or <see langword="null"/> when there is no image to draw.
		/// </summary>
		IImageSource? ImageSource { get; }
	}
}
