namespace Microsoft.Maui.Handlers
{
	public partial class RadioButtonHandler
	{
		public static IPropertyMapper<IRadioButton, RadioButtonHandler> Mapper = new PropertyMapper<IRadioButton, RadioButtonHandler>(ViewHandler.ViewMapper)
		{
			[nameof(IRadioButton.IsChecked)] = MapIsChecked,
		};

		public RadioButtonHandler() : base(Mapper)
		{

		}

		public RadioButtonHandler(IPropertyMapper mapper) : base(mapper ?? Mapper)
		{

		}
	}
}