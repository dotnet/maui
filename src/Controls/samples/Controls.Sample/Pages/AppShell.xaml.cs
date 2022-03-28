using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Pages
{
	public partial class AppShell
	{
		public AppShell()
		{
			InitializeComponent();
			SetTabBarBackgroundColor(this, Color.FromRgba(3, 169, 244, 255));

			cv1.ItemsSource = new[] { 1, 2, 3, 4, 5 };
			cv2.ItemsSource = new[] { 1, 2, 3, 4, 5 };
		}
	}
}
