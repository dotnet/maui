namespace Microsoft.Maui.Handlers
{
	public partial class CheckBoxHandler
	{
		public static PropertyMapper<ICheck, CheckBoxHandler> CheckBoxMapper = new PropertyMapper<ICheck, CheckBoxHandler>(ViewHandler.ViewMapper)
		{
#if MONOANDROID
			[nameof(ICheck.BackgroundColor)] = MapBackgroundColor,
#endif
			[nameof(ICheck.IsChecked)] = MapIsChecked,
			[nameof(ICheck.Color)] = MapColor
		};

		public CheckBoxHandler() : base(CheckBoxMapper)
		{

		}

		public CheckBoxHandler(PropertyMapper mapper) : base(mapper ?? CheckBoxMapper)
		{

		}
#if MONOANDROID
		public static void MapBackgroundColor(CheckBoxHandler handler, ICheck check)
		{
			handler.TypedNativeView?.UpdateBackgroundColor(check);
		}
#endif
		public static void MapIsChecked(CheckBoxHandler handler, ICheck check)
		{
			handler.TypedNativeView?.UpdateIsChecked(check);
		}

		public static void MapColor(CheckBoxHandler handler, ICheck check)
		{
			handler.TypedNativeView?.UpdateColor(check);
		}
	}
}