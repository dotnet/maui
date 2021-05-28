#nullable enable
namespace Microsoft.Maui.Handlers
{
	public partial class ButtonHandler
	{
		public static PropertyMapper<IButton, ButtonHandler> ButtonMapper = new PropertyMapper<IButton, ButtonHandler>(ViewHandler.ViewMapper)
		{
#if WINDOWS || __ANDROID__
			[nameof(IButton.Background)] = MapBackground,
#endif
			[nameof(IButton.CharacterSpacing)] = MapCharacterSpacing,
			[nameof(IButton.Font)] = MapFont,
			[nameof(IButton.Padding)] = MapPadding,
			[nameof(IButton.Text)] = MapText,
			[nameof(IButton.TextColor)] = MapTextColor,
		};

		public ButtonHandler() : base(ButtonMapper)
		{

		}

		public ButtonHandler(PropertyMapper? mapper = null) : base(mapper ?? ButtonMapper)
		{
		}
	}
}
