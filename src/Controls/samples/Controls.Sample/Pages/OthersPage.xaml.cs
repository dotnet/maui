using System;

namespace Maui.Controls.Sample.Pages
{
	public partial class OthersPage
	{
		TestWindowOverlay? overlay;

		public OthersPage()
		{
			InitializeComponent();
		}

		void TestAddOverlayWindow(object sender, EventArgs e)
		{
			var window = GetParentWindow();
			overlay ??= new TestWindowOverlay(window);
			window.AddOverlay(overlay);
		}

		void TestRemoveOverlayWindow(object sender, EventArgs e)
		{
			if (overlay is not null)
			{
				GetParentWindow().RemoveOverlay(overlay);
				overlay = null;
			}
		}

		void TestVisualTreeHelper(object sender, EventArgs e)
		{
			var overlay = GetParentWindow().VisualDiagnosticsOverlay;
			overlay.RemoveAdorners();
			overlay.AddAdorner(TestButton, false);
		}

		void EnableElementPicker(object sender, EventArgs e)
		{
			GetParentWindow().VisualDiagnosticsOverlay.EnableElementSelector = true;
		}
	}
}