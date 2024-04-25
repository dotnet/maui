using System.Threading.Tasks;
using Mopups.Pages;
using Mopups.Services;

namespace WebViewCrashMidLoad;

public partial class PopupPage1 : PopupPage
{
	public PopupPage1()
	{
		InitializeComponent();
	}

    private async void OnCloseButtonClicked(System.Object sender, System.EventArgs e)
    {
        await Task.Delay(2000);
        await MopupService.Instance.PopAsync();
    }

    private async void OnCallActionSheetClicked(System.Object sender, System.EventArgs e)
    {
        await DisplayActionSheet("Action Sheet Clicked", "Cancel", "Destruction", "Action1", "Action2");
    }


}