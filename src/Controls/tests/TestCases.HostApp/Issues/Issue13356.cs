#if ANDROID
using Android.Content;
using Android.OS;
using View = Android.Views.View;
using Google.Android.Material.BottomSheet;
#endif
using Microsoft.Maui.Platform;
namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 13356, "Java.Lang.IllegalArgumentException: The style on this component requires your app theme to be Theme.MaterialComponent.", PlatformAffected.Android)]
public class Issue13356 : TestContentPage
{
	protected override void Init()
	{
		var button = new Button
		{
			Text = "Show me",
			AutomationId = "showButton",
			HorizontalOptions = LayoutOptions.Center
		};
		button.Clicked += Button_Clicked;

		var stack = new VerticalStackLayout
		{
			Spacing = 25,
			Padding = new Thickness(30, 0),
			VerticalOptions = LayoutOptions.Center,
			Children =
			{
				button
			}
		};

		Content = new ScrollView
		{
			Content = stack
		};
	}

	void Button_Clicked(object sender, EventArgs e)
	{
#if ANDROID
		var droidContext = Handler.MauiContext.Context;
		var myDialog = new Issue13356Dialog(droidContext);
		var dialogContent = new Button { Text = "Dialog Button", Padding = new Thickness(30) };
		myDialog.Content = dialogContent.ToPlatform(Application.Current.Handler.MauiContext);
		myDialog.ShowDialog();
#endif
	}

#if ANDROID
public class Issue13356Dialog : BottomSheetDialog {
    public View Content { get; set; }

    public Issue13356Dialog(Context context) : base(context) {}

    protected override void OnCreate(Bundle savedInstanceState) {
        base.OnCreate(savedInstanceState);
        SetContentView(Content);
    }

    public void ShowDialog() {
        if (!IsShowing)
            Show();
    }
}
#endif
}
