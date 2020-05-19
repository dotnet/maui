using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;

namespace Xamarin.Forms.Controls.GalleryPages.PlatformSpecificsGalleries
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