using System.Threading.Tasks;
using Android.Graphics.Drawables;
using Android.Views;
using Android.Widget;
using Google.Android.Material.ImageView;

namespace Microsoft.Maui.Handlers;

internal partial class MaterialImageHandler : ViewHandler<IImage, ShapeableImageView>, IImageHandler
{
    public static PropertyMapper<IImage, MaterialImageHandler> Mapper =
      new(ViewMapper)
      {
          [nameof(IImage.Background)] = MapBackground,
          [nameof(IImage.Aspect)] = MapAspect,
          [nameof(IImage.IsAnimationPlaying)] = MapIsAnimationPlaying,
          [nameof(IImage.Source)] = MapSource,
      };

    public static CommandMapper<IImage, MaterialImageHandler> CommandMapper =
    new(ViewCommandMapper)
    {
    };

    ImageSourcePartLoader? _imageSourcePartLoader;

    public virtual ImageSourcePartLoader SourceLoader =>
        _imageSourcePartLoader ??= new ImageSourcePartLoader(new ImageImageSourcePartSetter(this));

    IImage IImageHandler.VirtualView => VirtualView;

    ImageView IImageHandler.PlatformView => PlatformView;

    public MaterialImageHandler() : base(Mapper, CommandMapper)
    {
    }

    protected override ShapeableImageView CreatePlatformView()
    {
        var imageView = new ShapeableImageView(MauiMaterialContextThemeWrapper.Create(Context));

        // Enable view bounds adjustment on measure.
        // This allows the ImageView's OnMeasure method to account for the image's intrinsic
        // aspect ratio during measurement, which gives us more useful values during constrained
        // measurement passes.
        imageView.SetAdjustViewBounds(true);

        return imageView;
    }

    protected override void ConnectHandler(ShapeableImageView platformView)
    {
        base.ConnectHandler(platformView);
        platformView.ViewAttachedToWindow += OnPlatformViewAttachedToWindow;
    }

    protected override void DisconnectHandler(ShapeableImageView platformView)
    {
        base.DisconnectHandler(platformView);
        platformView.ViewAttachedToWindow -= OnPlatformViewAttachedToWindow;
        SourceLoader.Reset();
    }

    public override bool NeedsContainer =>
        VirtualView?.Background != null ||
        base.NeedsContainer;

    static void MapSource(MaterialImageHandler handler, IImage image)
    {
        MapSourceAsync(handler, image).FireAndForget(handler);
    }

    public static async Task MapSourceAsync(IImageHandler handler, IImage image)
    {
        await handler
            .SourceLoader
            .UpdateImageSourceAsync();


        // This indicates that the image has finished loading
        // So, now if the attached event fires again then we need to see if Glide has cleared the image out
        handler.SourceLoader.CheckForImageLoadedOnAttached = true;

        // Because this resolves from a task we should validate that the
        // handler hasn't been disconnected
        if (handler.IsConnected())
        {
            handler.UpdateValue(nameof(IImage.IsAnimationPlaying));
        }
    }

    static void MapBackground(MaterialImageHandler handler, IImage image)
    {
        handler.UpdateValue(nameof(IViewHandler.ContainerView));

        handler.PlatformView?.UpdateBackground(image);
        handler.PlatformView?.UpdateOpacity(image);
    }

    static void MapIsAnimationPlaying(MaterialImageHandler handler, IImage image)
    {
        handler.PlatformView?.UpdateIsAnimationPlaying(image);
    }

    static void MapAspect(MaterialImageHandler handler, IImage image)
    {
        handler.PlatformView?.UpdateAspect(image);
    }

    public override void PlatformArrange(Graphics.Rect frame)
    {
        if (PlatformInterop.IsImageViewCenterCrop(PlatformView))
        {
            var (left, top, right, bottom) = PlatformView.ToPixels(frame);
            PlatformInterop.SetClipBounds(PlatformView, 0, 0, right - left, bottom - top);
        }
        else
        {
            PlatformView.ClipBounds = null;
        }
        base.PlatformArrange(frame);
    }

    void OnPlatformViewAttachedToWindow(object? sender, View.ViewAttachedToWindowEventArgs e)
    {
        if (sender is not View platformView)
        {
            return;
        }

        if (!this.IsConnected())
        {
            platformView.ViewAttachedToWindow -= OnPlatformViewAttachedToWindow;
            return;
        }

        ImageHandler.OnPlatformViewAttachedToWindow(this);
    }

    partial class ImageImageSourcePartSetter : ImageSourcePartSetter<IImageHandler>
    {
        public ImageImageSourcePartSetter(IImageHandler handler)
            : base(handler)
        {
        }

        public override void SetImageSource(Drawable? platformImage)
        {
            if (Handler?.PlatformView is not ShapeableImageView image)
            {
                return;
            }

            image.SetImageDrawable(platformImage);
        }
    }
}