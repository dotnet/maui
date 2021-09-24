using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	public class RefreshViewStub : StubBase, IRefreshView
	{
		public bool IsRefreshing { get; set; }

		public Paint RefreshColor { get; set; }

		public IView Content { get; set; }
	}
}