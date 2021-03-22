namespace Microsoft.Maui.Handlers
{
	public partial class StepperHandler
	{
		public static PropertyMapper<IStepper, StepperHandler> StepperMapper = new PropertyMapper<IStepper, StepperHandler>(ViewHandler.ViewMapper)
		{
			[nameof(IStepper.Minimum)] = MapMinimum,
			[nameof(IStepper.Maximum)] = MapMaximum,
			[nameof(IStepper.Interval)] = MapIncrement,
			[nameof(IStepper.Value)] = MapValue
		};

		public StepperHandler() : base(StepperMapper)
		{

		}

		public StepperHandler(PropertyMapper mapper) : base(mapper ?? StepperMapper)
		{

		}
	}
}