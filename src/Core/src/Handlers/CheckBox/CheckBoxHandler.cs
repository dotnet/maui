namespace Microsoft.Maui.Handlers
{
	public partial class CheckBoxHandler
	{
		public static PropertyMapper<ICheckBox, CheckBoxHandler> CheckBoxMapper = new PropertyMapper<ICheckBox, CheckBoxHandler>(ViewHandler.ViewMapper)
		{
			[nameof(ICheckBox.IsChecked)] = MapIsChecked,
#if MONOANDROID
			[nameof(ICheckBox.BackgroundColor)] = MapBackgroundColor,
#endif
		};

		public CheckBoxHandler() : base(CheckBoxMapper)
		{

		}

		public CheckBoxHandler(PropertyMapper mapper) : base(mapper ?? CheckBoxMapper)
		{

		}

		public static void MapIsChecked(CheckBoxHandler handler, ICheckBox check)
		{
			handler.TypedNativeView?.UpdateIsChecked(check);
		}
	}
}