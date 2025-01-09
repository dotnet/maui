#if IOS
using UIKit;
#endif

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 5669, "Windows SearchBar MaxLength > 0 not working properly", PlatformAffected.UWP)]
	public partial class Issue5669 : ContentPage
	{
		public Issue5669()
		{
			InitializeComponent();
		}

		private void Button_Clicked(object sender, EventArgs e)
		{
			searchbar.MaxLength = 4;
		}

		private void HideKeyboardButton_Clicked(object sender, EventArgs e)
		{
#if IOS
			foreach (var scene in UIApplication.SharedApplication.ConnectedScenes)
			{
				if (scene is UIWindowScene windowScene)
				{
					var window = windowScene.Windows.FirstOrDefault();
					window?.EndEditing(true);
				}
			}
#endif
		}
	}
}