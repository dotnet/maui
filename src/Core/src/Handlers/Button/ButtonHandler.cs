namespace Microsoft.Maui.Handlers
{
	public partial class ButtonHandler
	{
		public static PropertyMapper<IButton, ButtonHandler> ButtonMapper = new PropertyMapper<IButton, ButtonHandler>(ViewHandler.ViewMapper)
		{
#if WINDOWS
			[nameof(IButton.Background)] = MapBackground,
#endif
			[nameof(IButton.Text)] = MapText,
			[nameof(IButton.TextColor)] = MapTextColor,
			[nameof(IButton.Font)] = MapFont,
			[nameof(IButton.Padding)] = MapPadding,
		};

		public ButtonHandler() : base(ButtonMapper)
		{

		}

		public ButtonHandler(PropertyMapper? mapper = null) : base(mapper ?? ButtonMapper)
		{
		}
	}
}
