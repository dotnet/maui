using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	public class ActivityIndicatorStub : StubBase, IActivityIndicator
	{
		public bool IsRunning { get; set; }

		public Color Color { get; set; }
	}
}