using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 51238,
		"Transparent Grid causes Java.Lang.IllegalStateException: Unable to create layer for Platform_DefaultRenderer",
		PlatformAffected.Android)]
	public class Bugzilla51238 : TestContentPage
	{
		protected override void Init()
		{
			var grid = new Grid();
			grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });
			grid.RowDefinitions.Add(new RowDefinition { Height = 50 });

			var transparentLayer = new Grid();
			transparentLayer.IsVisible = false;
			transparentLayer.BackgroundColor = Colors.Lime;
			transparentLayer.Opacity = 0.5;

			var label = new Label
			{
				Text = "Foo",
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center
			};

			Grid.SetRow(label, 0);
			Grid.SetRow(transparentLayer, 0);

			var button = new Button
			{
				AutomationId = "TapMe",
				Text = "Tap Me!",
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center
			};

			Grid.SetRow(button, 1);

			button.Clicked += (sender, args) => { transparentLayer.IsVisible = !transparentLayer.IsVisible; };

			grid.Children.Add(label);
			grid.Children.Add(transparentLayer);
			grid.Children.Add(button);

			Content = grid;
		}
	}
}