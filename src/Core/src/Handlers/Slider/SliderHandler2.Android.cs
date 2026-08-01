using Google.Android.Material.Slider;

namespace Microsoft.Maui.Handlers;

public class SliderHandler2 : ViewHandler<ISlider, Slider>
{
    public static PropertyMapper<ISlider, SliderHandler2> Mapper =
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

    public static CommandMapper<ISlider, SliderHandler2> CommandMapper =
            new(ViewCommandMapper);

    public SliderHandler2() : base(Mapper, CommandMapper)
    {
    }

    protected override Slider CreatePlatformView()
    {
        return new Slider(MauiMaterialContextThemeWrapper.Create(Context))
        {
            DuplicateParentStateEnabled = false,
            LabelBehavior = LabelFormatter.LabelGone,
        };
    }

    protected override void ConnectHandler(Slider platformView)
    {
        base.ConnectHandler(platformView);
        platformView.Change += OnChange;
        platformView.StartTrackingTouch += OnStartTrackingTouch;
        platformView.StopTrackingTouch += OnStopTrackingTouch;
    }

    protected override void DisconnectHandler(Slider platformView)
    {
        platformView.Change -= OnChange;
        platformView.StartTrackingTouch -= OnStartTrackingTouch;
        platformView.StopTrackingTouch -= OnStopTrackingTouch;
        base.DisconnectHandler(platformView);
    }

    // Slider.ChangeEventArgs: P0 = slider (Slider), P1 = value (float), P2 = fromUser (bool)
    void OnChange(object? sender, Slider.ChangeEventArgs e)
    {
        if (e.P2 && VirtualView is not null && (float)VirtualView.Value != e.P1)
        {
            VirtualView.Value = e.P1;
        }
    }

    void OnStartTrackingTouch(object? sender, Slider.StartTrackingTouchEventArgs e)
    {
        VirtualView?.DragStarted();
    }

    void OnStopTrackingTouch(object? sender, Slider.StopTrackingTouchEventArgs e)
    {
        VirtualView?.DragCompleted();
    }

    public static void MapValue(SliderHandler2 handler, ISlider slider)
    {
        handler.PlatformView?.UpdateValue(slider);
    }

    public static void MapMinimum(SliderHandler2 handler, ISlider slider)
    {
        handler.PlatformView?.UpdateMinimum(slider);
    }

    public static void MapMaximum(SliderHandler2 handler, ISlider slider)
    {
        handler.PlatformView?.UpdateMaximum(slider);
    }

    public static void MapMinimumTrackColor(SliderHandler2 handler, ISlider slider)
    {
        handler.PlatformView?.UpdateMinimumTrackColor(slider);
    }

    public static void MapMaximumTrackColor(SliderHandler2 handler, ISlider slider)
    {
        handler.PlatformView?.UpdateMaximumTrackColor(slider);
    }

    public static void MapThumbColor(SliderHandler2 handler, ISlider slider)
    {
        handler.PlatformView?.UpdateThumbColor(slider);
    }

    public static void MapThumbImageSource(SliderHandler2 handler, ISlider slider)
    {
        var provider = handler.GetRequiredService<IImageSourceServiceProvider>();

        handler.PlatformView?.UpdateThumbImageSourceAsync(slider, provider)
            .FireAndForget(handler);
    }
}
