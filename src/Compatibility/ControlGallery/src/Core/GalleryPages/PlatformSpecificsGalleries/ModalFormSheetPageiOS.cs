using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.GalleryPages.PlatformSpecificsGalleries
{
	public class ModalFormSheetPageiOS : ContentPage
	{
		public ModalFormSheetPageiOS()
		{
			Title = "Modal FormSheet";
			BackgroundColor = Color.Azure;

			On<iOS>().SetModalPresentationStyle(UIModalPresentationStyle.FormSheet);

			var restoreButton = new Button { Text = "Back To Gallery" };
			restoreButton.Clicked += (sender, args) => Navigation.PopModalAsync();

			Content = new StackLayout
			{
				Children =
				{
					restoreButton
				}
			};
		}
	}
}