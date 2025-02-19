using System.Windows.Input;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific;

namespace Microsoft.Maui.Controls.ControlGallery.GalleryPages.PlatformSpecificsGalleries
{
	public class ApplicationAndroid : ContentPage
	{
		public ApplicationAndroid(ICommand restore)
		{
			var restoreButton = new Button { Text = "Back To Gallery" };
			restoreButton.Clicked += (sender, args) => restore.Execute(null);

			var button1 = GetButton(WindowSoftInputModeAdjust.Pan);
			var button2 = GetButton(WindowSoftInputModeAdjust.Resize);
			var buttons = new StackLayout { Orientation = StackOrientation.Horizontal, Children = { button1, button2 }, VerticalOptions = LayoutOptions.Start };
			var entry = new Entry { Text = "1", VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.StartAndExpand };
			var layout = new Compatibility.RelativeLayout
			{
				VerticalOptions = LayoutOptions.StartAndExpand,
				HorizontalOptions = LayoutOptions.Center,
			};
			layout.Children.Add(buttons, yConstraint: Compatibility.Constraint.RelativeToParent(parent => { return parent.Y; }));
			layout.Children.Add(entry, yConstraint: Compatibility.Constraint.RelativeToParent(parent => { return parent.Height - 100; }));

			Content = layout;
			Title = "Application Features";
		}

		static Button GetButton(WindowSoftInputModeAdjust value)
		{
			var button = new Button { Text = value.ToString(), Margin = 20 };
			button.Clicked += (sender, args) =>
			{
				Application.Current.On<Android>().UseWindowSoftInputModeAdjust(value);
			};
			return button;
		}
	}
}