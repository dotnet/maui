using System.Diagnostics;

namespace Xamarin.Forms.Controls
{
	public class SimpleApp : Application
	{
		public SimpleApp()
		{
			var label = new Label { VerticalOptions = LayoutOptions.CenterAndExpand };

			if (Current.Properties.ContainsKey("LabelText"))
			{
				label.Text = (string)Current.Properties["LabelText"] + " Restored!";
				Debug.WriteLine("Initialized");
			}
			else
			{
				Current.Properties["LabelText"] = "Wowza";
				label.Text = (string)Current.Properties["LabelText"] + " Set!";
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

			SerializeProperties();
		}

		static async void SerializeProperties()
		{
			await Current.SavePropertiesAsync();
		}
	}
}