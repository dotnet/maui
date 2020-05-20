namespace System.Maui.Platform
{
	public partial class StepperRenderer
	{
		public static PropertyMapper<IStepper> StepperMapper = new PropertyMapper<IStepper>(ViewRenderer.ViewMapper)
		{
			[nameof(IStepper.Minimum)] = MapPropertyMinimum,
			[nameof(IStepper.Maximum)] = MapPropertyMaximum,
			[nameof(IStepper.Increment)] = MapPropertyIncrement,
			[nameof(IStepper.Value)] = MapPropertyValue
#if __ANDROID__ || NETCOREAPP
			,[nameof(IStepper.IsEnabled)] = MapPropertyIsEnabled
#endif
		};

		public StepperRenderer() : base(StepperMapper)
		{

		}

		public StepperRenderer(PropertyMapper mapper) : base(mapper ?? StepperMapper)
		{

		}
	}
}