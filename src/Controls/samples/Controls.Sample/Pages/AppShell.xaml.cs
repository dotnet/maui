using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Pages
{
	public partial class AppShell
	{
		public AppShell()
		{
			InitializeComponent();
			SetTabBarBackgroundColor(this, Color.FromRgba(3, 169, 244, 255));
		}

		protected override void OnNavigating(ShellNavigatingEventArgs args)
		{
			base.OnNavigating(args);
			if (args.Source == ShellNavigationSource.Pop)
				args.Cancel();
		}
	}
}
