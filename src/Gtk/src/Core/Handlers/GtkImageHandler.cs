using System;
using Microsoft.Maui.Gtk.Hosting;
using Microsoft.Maui.Gtk.ImageSources;
using Microsoft.Maui.Gtk.Platform;
using Microsoft.Maui.Handlers;
using GtkPicture = Gtk.Picture;

namespace Microsoft.Maui.Gtk.Handlers
{
	/// <summary>
	/// <see cref="ImageHandler"/> over a <c>Gtk.Picture</c>. The source is decoded to a <c>Gdk.Texture</c> by the
	/// <see cref="IGtkImageSourceService"/> registered for the source type; <see cref="Aspect"/> maps to <c>ContentFit</c>.
	/// </summary>
	/// <remarks>Images are static: <c>IsAnimationPlaying</c> is accepted and ignored.</remarks>
	public class GtkImageHandler : ImageHandler
	{
		readonly GtkImageLoader _loader = new();

		/// <summary>Creates the handler with the stock mappers.</summary>
		public GtkImageHandler()
		{
		}

		/// <summary>Creates the handler with custom mappers.</summary>
		/// <param name="mapper">The property mapper, or <see langword="null"/> for the stock one.</param>
		/// <param name="commandMapper">The command mapper, or <see langword="null"/> for the stock one.</param>
		public GtkImageHandler(IPropertyMapper? mapper, CommandMapper? commandMapper = null)
			: base(mapper, commandMapper)
		{
		}

		/// <inheritdoc/>
		protected override object CreatePlatformView() => new GtkPicture { CanShrink = true, ContentFit = global::Gtk.ContentFit.Contain };

		/// <inheritdoc/>
		protected override void DisconnectHandler(object platformView)
		{
			_loader.Reset();
			base.DisconnectHandler(platformView);
		}

		internal static void ApplyMappers()
		{
			var mapper = Mapper;

			mapper.MapGtk<IImage, IImageHandler, GtkPicture>(nameof(IImage.Aspect), (h, v, w) => w.ContentFit = ToContentFit(v.Aspect));
			mapper.MapGtk<IImage, IImageHandler, GtkPicture>(nameof(IImage.IsAnimationPlaying), (h, v, w) => { });
			mapper.MapGtk<IImage, IImageHandler, GtkPicture>(nameof(IImage.Background), (h, v, w) => GtkStyle.For(w).SetBackground(v.Background));
			mapper.MapGtk<IImage, IImageHandler, GtkPicture>(nameof(IImage.Source), (h, v, w) =>
			{
				if (h is GtkImageHandler handler)
					_ = handler._loader.UpdateAsync(h, v, texture => w.Paintable = texture);
			});
			// Explicit sizes are applied by the layout (GetDesiredSize/PlatformArrange), not by the widget.
			mapper.MapGtk<IImage, IImageHandler, GtkPicture>(nameof(IImage.Width), (h, v, w) => { });
			mapper.MapGtk<IImage, IImageHandler, GtkPicture>(nameof(IImage.Height), (h, v, w) => { });
		}

		internal static global::Gtk.ContentFit ToContentFit(Aspect aspect) => aspect switch
		{
			Aspect.AspectFill => global::Gtk.ContentFit.Cover,
			Aspect.Fill => global::Gtk.ContentFit.Fill,
			Aspect.Center => global::Gtk.ContentFit.ScaleDown,
			_ => global::Gtk.ContentFit.Contain,
		};
	}
}
