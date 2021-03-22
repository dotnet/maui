using System.Linq;

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.GalleryPages
{
	public class MaterialEntryGalleryPage : ContentPage
	{
		public MaterialEntryGalleryPage()
		{
			Visual = VisualMarker.Material;

			StackLayout layout = null;
			layout = new StackLayout()
			{
				Children =
				{
					new Entry()
					{
						Text = "With Text",
						Placeholder = "Placeholder",
					},
					new Entry()
					{
						Placeholder = "Placeholder"
					},
					new Entry()
					{
						Placeholder = "Green Placeholder",
						PlaceholderColor = Color.Green,
					},
					new Entry()
					{
						Placeholder = "Purple Placeholder",
						PlaceholderColor = Color.Purple,
					},
					new Entry()
					{
						Text = "Green TextColor",
						TextColor = Color.Green
					},
					new Entry()
					{
						Text = "Purple TextColor",
						TextColor = Color.Purple
					},
					new Entry()
					{
						Text = "With Text larger font",
						Placeholder = "Placeholder",
						FontSize = 24
					},
					new Entry()
					{
						Text = "Yellow BackgroundColor",
						BackgroundColor = Color.Yellow
					},
					new Entry()
					{
						Text = "Cyan BackgroundColor",
						BackgroundColor = Color.Cyan
					},
					new Button()
					{
						Text = "Toggle Entry clear button mode",
						Command = new Command(() =>
						{
							foreach(Entry e in layout.Children.OfType<Entry>())
							{
								e.ClearButtonVisibility = e.ClearButtonVisibility == ClearButtonVisibility.Never ? ClearButtonVisibility.WhileEditing : ClearButtonVisibility.Never;
							}
						})
					}
				}
			};

			Content = new ScrollView()
			{
				Content = layout
			};
		}
	}
}
