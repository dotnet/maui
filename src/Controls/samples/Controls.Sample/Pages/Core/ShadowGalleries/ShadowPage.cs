using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;

namespace Maui.Controls.Sample.Pages
{
	[Preserve(AllMembers = true)]
	public class ShadowPage : ContentPage
	{
		public ShadowPage()
		{
			var descriptionLabel =
				new Label { Text = "Shadow Galleries", Margin = new Thickness(2, 2, 2, 2) };

			Title = "Shadow Galleries";

			Content = new ScrollView
			{
				Content = new StackLayout
				{
					Children =
					{
						descriptionLabel,
						GalleryBuilder.NavButton("Shadow Playground", () =>
							new ShadowPlayground(), Navigation),
#if ANDROID
						GalleryBuilder.NavButton("Shadow Benchmark", () =>
							new ShadowBenchmark(), Navigation),
#endif
					}
				}
			};
		}
	}
}