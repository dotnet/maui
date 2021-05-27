namespace Microsoft.Maui.Handlers
{
	public partial class StepperHandler
	{
		public static PropertyMapper<IStepper, StepperHandler> StepperMapper = new PropertyMapper<IStepper, StepperHandler>(ViewHandler.ViewMapper)
		{
			[nameof(IStepper.Interval)] = MapIncrement,
			[nameof(IStepper.Maximum)] = MapMaximum,
			[nameof(IStepper.Minimum)] = MapMinimum,
			[nameof(IStepper.Value)] = MapValue,
#if WINDOWS
			[nameof(IStepper.Background)] = MapBackground,
#endif
		};

		public StepperHandler() : base(StepperMapper)
		{

		}

		public StepperHandler(PropertyMapper mapper) : base(mapper ?? StepperMapper)
		{

		}
	}
}