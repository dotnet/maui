using Google.Android.Material.Slider;

namespace Microsoft.Maui.Handlers;

internal partial class MaterialSliderHandler : ViewHandler<ISlider, Slider>
{
    public static PropertyMapper<ISlider, MaterialSliderHandler> Mapper =
            new(ViewMapper)
            {
                [nameof(ISlider.Value)] = MapValue,
                [nameof(ISlider.Minimum)] = MapMinimum,
                [nameof(ISlider.Maximum)] = MapMaximum,
                [nameof(ISlider.MinimumTrackColor)] = MapMinimumTrackColor,
                [nameof(ISlider.MaximumTrackColor)] = MapMaximumTrackColor,
                [nameof(ISlider.ThumbColor)] = MapThumbColor,
                [nameof(ISlider.ThumbImageSource)] = MapThumbImageSource,

            };

    public static CommandMapper<ISlider, MaterialSliderHandler> CommandMapper =
            new(ViewCommandMapper);

    public MaterialSliderHandler() : base(Mapper, CommandMapper)
    {
    }

    protected override Slider CreatePlatformView()
    {
        return new Slider(Context)
        {
            DuplicateParentStateEnabled = false,
        };
    }

    protected override void ConnectHandler(Slider platformView)
    {
        // TODO: Material3: Add listeners when https://github.com/dotnet/android-libraries/issues/230 is resolved

    }

    protected override void DisconnectHandler(Slider platformView)
    {
        // TODO: Material3: Cleanup listeners when implemented
    }

    public static void MapValue(MaterialSliderHandler handler, ISlider slider)
    {
        handler.PlatformView.UpdateValue(slider);
    }

    public static void MapMinimum(MaterialSliderHandler handler, ISlider slider)
    {
        handler.PlatformView?.UpdateMinimum(slider);
    }

    public static void MapMaximum(MaterialSliderHandler handler, ISlider slider)
    {
        handler.PlatformView?.UpdateMaximum(slider);
    }

    public static void MapMinimumTrackColor(MaterialSliderHandler handler, ISlider slider)
    {
        handler.PlatformView?.UpdateMinimumTrackColor(slider);
    }

    public static void MapMaximumTrackColor(MaterialSliderHandler handler, ISlider slider)
    {
        handler.PlatformView?.UpdateMaximumTrackColor(slider);
    }

    public static void MapThumbColor(MaterialSliderHandler handler, ISlider slider)
    {
        handler.PlatformView?.UpdateThumbColor(slider);
    }

    public static void MapThumbImageSource(MaterialSliderHandler handler, ISlider slider)
    {
        var provider = handler.GetRequiredService<IImageSourceServiceProvider>();

        handler.PlatformView?.UpdateThumbImageSourceAsync(slider, provider)
            .FireAndForget(handler);
    }

    void OnStartTrackingTouch(Slider slider) =>
        VirtualView?.DragStarted();

    void OnStopTrackingTouch(Slider slider) =>
        VirtualView?.DragCompleted();

}