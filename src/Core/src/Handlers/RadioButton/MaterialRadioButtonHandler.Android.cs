using Android.Widget;
using Google.Android.Material.RadioButton;

namespace Microsoft.Maui.Handlers;

internal partial class MaterialRadioButtonHandler : ViewHandler<IRadioButton, MaterialRadioButton>
{
	public static PropertyMapper<IRadioButton, MaterialRadioButtonHandler> Mapper =
		new(ViewMapper)
		{
			[nameof(IRadioButton.Background)] = MapBackground,
			[nameof(IRadioButton.IsChecked)] = MapIsChecked,
			[nameof(IRadioButton.Content)] = MapContent,
			[nameof(ITextStyle.TextColor)] = MapTextColor,
			[nameof(ITextStyle.CharacterSpacing)] = MapCharacterSpacing,
			[nameof(ITextStyle.Font)] = MapFont,
			[nameof(IRadioButton.StrokeColor)] = MapStrokeColor,
			[nameof(IRadioButton.StrokeThickness)] = MapStrokeThickness,
			[nameof(IRadioButton.CornerRadius)] = MapCornerRadius,
		};

	public static CommandMapper<IRadioButton, MaterialRadioButtonHandler> CommandMapper =
		new(ViewCommandMapper);

	public MaterialRadioButtonHandler() : base(Mapper, CommandMapper)
	{
	}

	public override void PlatformArrange(Graphics.Rect frame)
	{
		this.PrepareForTextViewArrange(frame);
		base.PlatformArrange(frame);
	}

	protected override MaterialRadioButton CreatePlatformView()
	{
		return new MaterialRadioButton(Context)
		{
			SoundEffectsEnabled = false
		};
	}

	protected override void ConnectHandler(MaterialRadioButton platformView)
	{
		if (platformView is not null)
		{
			platformView.CheckedChange += OnCheckChanged;
		}
	}

	protected override void DisconnectHandler(MaterialRadioButton platformView)
	{
		if (platformView is not null)
		{
			platformView.CheckedChange -= OnCheckChanged;
		}
	}

	public static void MapBackground(MaterialRadioButtonHandler handler, IRadioButton radioButton)
	{
		handler.PlatformView?.UpdateBackground(radioButton);
	}

	public static void MapIsChecked(MaterialRadioButtonHandler handler, IRadioButton radioButton)
	{
		handler.PlatformView?.UpdateIsChecked(radioButton);
	}

	public static void MapContent(MaterialRadioButtonHandler handler, IRadioButton radioButton)
	{
		handler.PlatformView?.UpdateContent(radioButton);
	}

	public static void MapTextColor(MaterialRadioButtonHandler handler, ITextStyle textStyle)
	{
		handler.PlatformView?.UpdateTextColor(textStyle);
	}

	public static void MapCharacterSpacing(MaterialRadioButtonHandler handler, ITextStyle textStyle)
	{
		handler.PlatformView?.UpdateCharacterSpacing(textStyle);
	}

	public static void MapFont(MaterialRadioButtonHandler handler, ITextStyle textStyle)
	{
		var fontManager = handler.GetRequiredService<IFontManager>();

		handler.PlatformView?.UpdateFont(textStyle, fontManager);
	}

	public static void MapStrokeColor(MaterialRadioButtonHandler handler, IRadioButton radioButton)
	{
		handler.PlatformView?.UpdateStrokeColor(radioButton);
	}

	public static void MapStrokeThickness(MaterialRadioButtonHandler handler, IRadioButton radioButton)
	{
		handler.PlatformView?.UpdateStrokeThickness(radioButton);
	}

	public static void MapCornerRadius(MaterialRadioButtonHandler handler, IRadioButton radioButton)
	{
		handler.PlatformView?.UpdateCornerRadius(radioButton);
	}

	void OnCheckChanged(object? sender, CompoundButton.CheckedChangeEventArgs e)
	{
		if (VirtualView is null)
		{
			return;
		}

		VirtualView.IsChecked = e.IsChecked;
	}
}