namespace Microsoft.Maui.UnitTests
{
	class ApplicationStub : IApplication
	{
		public string Property { get; set; } = "Default";

		public IWindow CreateWindow(IActivationState activationState)
		{
			throw new System.NotImplementedException();
		}
	}
}