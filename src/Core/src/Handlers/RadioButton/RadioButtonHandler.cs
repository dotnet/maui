namespace Microsoft.Maui.Handlers
{
	public partial class RadioButtonHandler
	{
		public static PropertyMapper<IRadioButton, RadioButtonHandler> RadioButtonMapper = new PropertyMapper<IRadioButton, RadioButtonHandler>(ViewHandler.ViewMapper)
		{
			[nameof(IRadioButton.IsChecked)] = MapIsChecked
		};

		public RadioButtonHandler() : base(RadioButtonMapper)
		{

		}

		public RadioButtonHandler(PropertyMapper mapper) : base(mapper ?? RadioButtonMapper)
		{

		}
	}
}