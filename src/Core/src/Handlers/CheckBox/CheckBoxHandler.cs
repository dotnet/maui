namespace Microsoft.Maui.Handlers
{
	public partial class CheckBoxHandler
	{
		public static PropertyMapper<ICheckBox, CheckBoxHandler> CheckBoxMapper = new PropertyMapper<ICheckBox, CheckBoxHandler>(ViewHandler.ViewMapper)
		{
#if MONOANDROID
			[nameof(ICheckBox.Background)] = MapBackground,
#endif
			[nameof(ICheckBox.IsChecked)] = MapIsChecked,
			[nameof(ICheckBox.Foreground)] = MapForeground,
		};

		public CheckBoxHandler() : base(CheckBoxMapper)
		{

		}

		public CheckBoxHandler(PropertyMapper mapper) : base(mapper ?? CheckBoxMapper)
		{

		}
	}
}