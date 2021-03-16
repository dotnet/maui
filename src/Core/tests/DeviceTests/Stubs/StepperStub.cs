namespace Microsoft.Maui.DeviceTests.Stubs
{
	public partial class StepperStub : StubBase, IStepper
	{
		public double Increment { get; set; }

		public double Minimum { get; set; }

		public double Maximum { get; set; }

		public double Value { get; set; }
	}
}