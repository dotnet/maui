using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	public class RefreshViewStub : StubBase, IRefreshView
	{
		IView _content;
		bool _isRefreshEnabled = true;

		public bool IsRefreshing { get; set; }

		public bool IsRefreshEnabled
		{
			get => _isRefreshEnabled;
			set => SetProperty(ref _isRefreshEnabled, value);
		}

		public Paint RefreshColor { get; set; }

		public IView Content
		{
			get => _content;
			set => SetProperty(ref _content, value);
		}
	}
}