using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using WThickness = Microsoft.UI.Xaml.Thickness;

namespace Microsoft.Maui.Platform
{
	public class MauiButton : Button
	{
		public MauiButton()
		{
			VerticalAlignment = VerticalAlignment.Stretch;
			HorizontalAlignment = HorizontalAlignment.Stretch;
			Content = new StackPanel
			{
				HorizontalAlignment = HorizontalAlignment.Center,
				VerticalAlignment = VerticalAlignment.Center,
				Orientation = Orientation.Horizontal,
				Margin = new WThickness(0),
				Children =
				{
					new Image
					{
						VerticalAlignment = VerticalAlignment.Center,
						HorizontalAlignment = HorizontalAlignment.Center,
						Stretch = Stretch.Uniform,
						Margin = new WThickness(0),
						Visibility = UI.Xaml.Visibility.Collapsed,
					},
					new TextBlock
					{
						VerticalAlignment = VerticalAlignment.Center,
						HorizontalAlignment = HorizontalAlignment.Center,
						Margin = new WThickness(0),
						Visibility = UI.Xaml.Visibility.Collapsed,
					}
				}
			};
		}

		protected override AutomationPeer OnCreateAutomationPeer()
		{
			return new MauiButtonAutomationPeer(this);
		}
	}
}