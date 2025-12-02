namespace Microsoft.Maui.Handlers;

internal partial class MaterialSliderHandler : ViewHandler<ISlider, MauiMaterialSlider>
{
	public static PropertyMapper<ISlider, MaterialSliderHandler> Mapper =
			new(ElementMapper)
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

	protected override MauiMaterialSlider CreatePlatformView()
	{
		return new MauiMaterialSlider(Context)
		{
			DuplicateParentStateEnabled = false,
		};
	}

	protected override void ConnectHandler(MauiMaterialSlider platformView)
	{
		//https://github.com/dotnet/android-libraries/issues/230
	}

	protected override void DisconnectHandler(MauiMaterialSlider platformView)
	{

	}

	internal static void MapValue(MaterialSliderHandler handler, ISlider slider)
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

	internal void OnValueChanged(MauiMaterialSlider slider, float value, bool fromUser)
	{
		if (VirtualView == null || !fromUser)
			return;

		var min = VirtualView.Minimum;
		var max = VirtualView.Maximum;
		var sliderValue = min + (max - min) * (value / slider.ValueTo);

		VirtualView.Value = sliderValue;
	}

	internal void OnStartTrackingTouch(MauiMaterialSlider slider) =>
		VirtualView?.DragStarted();

	internal void OnStopTrackingTouch(MauiMaterialSlider slider) =>
		VirtualView?.DragCompleted();
}