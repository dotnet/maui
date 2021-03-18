namespace Microsoft.Maui.Handlers
{
	public partial class ButtonHandler
	{
		public static PropertyMapper<IButton, ButtonHandler> ButtonMapper = new PropertyMapper<IButton, ButtonHandler>(ViewHandler.ViewMapper)
		{
			[nameof(IButton.Text)] = MapText,
			[nameof(IButton.TextColor)] = MapTextColor,
			[nameof(ILabel.Font)] = MapFont,
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
