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
	/// <remarks>
	/// Animated sources (GIF, APNG, animated WebP) show their first frame and play while <c>IsAnimationPlaying</c> is <see langword="true"/>, driven by the
	/// widget's frame clock (<see cref="GtkImageAnimator"/>).
	/// </remarks>
	public class GtkImageHandler : ImageHandler
	{
		readonly GtkImageLoader _loader = new();
		GtkImageAnimator? _animator;

		/// <summary>Creates the handler with the stock mappers.</summary>
		public GtkImageHandler()
			: base(GtkMapper, GtkCommandMapper)
		{
		}

		/// <summary>Creates the handler with custom mappers.</summary>
		/// <param name="mapper">The property mapper, or <see langword="null"/> for the stock one.</param>
		/// <param name="commandMapper">The command mapper, or <see langword="null"/> for the stock one.</param>
		public GtkImageHandler(IPropertyMapper? mapper, CommandMapper? commandMapper = null)
			: base(mapper ?? GtkMapper, commandMapper ?? GtkCommandMapper)
		{
		}

		internal GtkImageAnimator? Animator => _animator;

		/// <inheritdoc/>
		protected override object CreatePlatformView()
		{
			var picture = new GtkPicture { CanShrink = true, ContentFit = global::Gtk.ContentFit.Contain };
			_animator = new GtkImageAnimator(picture, texture => picture.Paintable = texture);
			return picture;
		}

		/// <inheritdoc/>
		protected override void DisconnectHandler(object platformView)
		{
			_loader.Reset();
			_animator?.Dispose();
			base.DisconnectHandler(platformView);
		}

		internal static readonly IPropertyMapper<IImage, IImageHandler> GtkMapper = CreateMapper();

		internal static readonly CommandMapper<IImage, IImageHandler> GtkCommandMapper = CreateCommandMapper();

		static IPropertyMapper<IImage, IImageHandler> CreateMapper()
		{
			var mapper = new GtkMapperBuilder<IImage, IImageHandler>(ImageHandler.Mapper).AddViewMappings();
			var commands = new GtkCommandMapperBuilder<IImage, IImageHandler>(ImageHandler.CommandMapper).AddViewCommands();
			Configure(mapper, commands);
			return mapper.Build();
		}

		static CommandMapper<IImage, IImageHandler> CreateCommandMapper()
		{
			var mapper = new GtkMapperBuilder<IImage, IImageHandler>(ImageHandler.Mapper).AddViewMappings();
			var commands = new GtkCommandMapperBuilder<IImage, IImageHandler>(ImageHandler.CommandMapper).AddViewCommands();
			Configure(mapper, commands);
			return commands.Build();
		}

		static void Configure(GtkMapperBuilder<IImage, IImageHandler> mapper, GtkCommandMapperBuilder<IImage, IImageHandler> commands)
		{
			mapper.MapGtk<GtkPicture>(nameof(IImage.Aspect), (h, v, w) => w.ContentFit = ToContentFit(v.Aspect));
			mapper.MapGtk<GtkPicture>(nameof(IImage.IsAnimationPlaying), (h, v, w) => (h as GtkImageHandler)?._animator?.SetPlaying(v.IsAnimationPlaying));
			mapper.MapGtk<GtkPicture>(nameof(IImage.Background), (h, v, w) => GtkBackgroundImage.Update(h, w, v.Background));
			mapper.MapGtk<GtkPicture>(nameof(IImage.Source), (h, v, w) =>
			{
				if (h is GtkImageHandler handler)
					_ = handler._loader.UpdateAsync(h, v, texture => w.Paintable = texture, animation =>
					{
						handler._animator?.SetAnimation(animation);
						handler._animator?.SetPlaying(v.IsAnimationPlaying);
					});
			});
			// Explicit sizes are applied by the layout (GetDesiredSize/PlatformArrange), not by the widget.
			mapper.MapGtk<GtkPicture>(nameof(IImage.Width), (h, v, w) => { });
			mapper.MapGtk<GtkPicture>(nameof(IImage.Height), (h, v, w) => { });
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
