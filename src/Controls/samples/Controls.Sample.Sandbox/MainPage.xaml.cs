#nullable disable
using Microsoft.Maui.Platform;

#if ANDROID
using Android.Content;
using Android.OS;
using View = Android.Views.View;
using Google.Android.Material.BottomSheet;
#endif
namespace Maui.Controls.Sample;

public partial class MainPage : ContentPage
{
	public MainPage()
	{
		InitializeComponent();
	}
	/*
	 The problem is in the MauiMaterialButton constructor - it's directly inheriting from MaterialButton but doesn't use the MaterialContextThemeWrapper that other Material components in MAUI use to ensure the correct theme is applied.

Looking at the MaterialButtonRenderer and other Material components, they use MaterialContextThemeWrapper.Create(context) to wrap the context and ensure the Material Components theme is properly applied.
	 */

	private void Button_Clicked(object sender, EventArgs e)
	{
#if ANDROID
		var droidContext = Handler.MauiContext.Context;
		var myDialog = new MyDialog(droidContext);
		// If you replace the Button with a Label, it will work.
		var dialogContent = new Button { Text = "Hello from the dialog", Padding = new Thickness(30) };
		myDialog.Content = dialogContent.ToPlatform(Application.Current.Handler.MauiContext);
		myDialog.ShowDialog();
#endif
	}
}

#if ANDROID
public class MyDialog : BottomSheetDialog {
    public View Content { get; set; }

    public MyDialog(Context context) : base(context) {}

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