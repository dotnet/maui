using Android.Views;
using Google.Android.Material.Slider;

namespace Microsoft.Maui.Handlers;

// TODO: Material3: Make it public in .NET 11
internal class SliderHandler2 : ViewHandler<ISlider, Slider>
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
        };
    }

    protected override void ConnectHandler(Slider platformView)
    {
        // TODO: Material3: Add listeners when https://github.com/dotnet/android-libraries/issues/230 is resolved
        // Using Touch event as a workaround for missing addOnChangeListener binding
        // See: https://github.com/dotnet/android-libraries/issues/230#issuecomment-891341936
        platformView.Touch += Slider_Touch;
    }

    void Slider_Touch(object? sender, View.TouchEventArgs e)
    {
        if (sender is not Slider slider)
        {
            return;
        }

        switch (e.Event?.Action)
        {
            case MotionEventActions.Down:
                {
                    OnStartTrackingTouch(slider);
                    break;
                }
            case MotionEventActions.Move:
                {
                    OnValueChanged(slider, slider.Value);
                    break;
                }
            case MotionEventActions.Up:
                {
                    OnValueChanged(slider, slider.Value);
                    OnStopTrackingTouch(slider);
                    break;
                }
        }
        // Pass through to Material3 Slider so it can update its own visual state
        e.Handled = false;
    }

    protected override void DisconnectHandler(Slider platformView)
    {
        // TODO: Material3: Cleanup listeners when implemented
        platformView.Touch -= Slider_Touch;
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

    void OnStartTrackingTouch(Slider slider) =>
        VirtualView?.DragStarted();

    void OnStopTrackingTouch(Slider slider) =>
        VirtualView?.DragCompleted();

    void OnValueChanged(Slider slider, float value)
    {
        if (VirtualView is null)
        {
            return;
        }

        if ((float)VirtualView.Value != value)
        {
            VirtualView.Value = value;
        }
    }
}
