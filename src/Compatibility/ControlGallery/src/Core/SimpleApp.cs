using System.Diagnostics;
using Microsoft.Maui.Storage;

namespace Microsoft.Maui.Controls.ControlGallery
{
	public class SimpleApp : Application
	{
		public SimpleApp()
		{
			var label = new Label { VerticalOptions = LayoutOptions.CenterAndExpand };

			var labelText = Preferences.Get("LabelText", string.Empty);
			if (!string.IsNullOrEmpty(labelText))
			{
				label.Text = labelText + " Restored!";
				Debug.WriteLine("Initialized");
			}
			else
			{
				Preferences.Set("LabelText", "Wowza");
				label.Text = Preferences.Get("LabelText", string.Empty) + " Set!";
				Debug.WriteLine("Saved");
			}

			MainPage = new ContentPage
			{
				Content = new StackLayout
				{
					Children =
					{
						label
					}
				}
			};
		}
	}
}