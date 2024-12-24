using Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific;
using Button = Microsoft.Maui.Controls.Button;

namespace Maui.Controls.Sample.Issues;


[Issue(IssueTracker.Bugzilla, 39331, "[Android] BoxView Is InputTransparent Even When Set to False")]
public class Bugzilla39331 : TestContentPage
{
	View _busyBackground;
	Button _btnLogin;

	protected override void Init()
	{
#pragma warning disable CS0618 // Type or member is obsolete
#pragma warning disable CS0618 // Type or member is obsolete
		AbsoluteLayout layout = new AbsoluteLayout
		{
			HorizontalOptions = LayoutOptions.FillAndExpand,
			VerticalOptions = LayoutOptions.FillAndExpand,
		};
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning restore CS0618 // Type or member is obsolete

		BackgroundColor = Color.FromUint(0xFFDBDBDB);

#pragma warning disable CS0618 // Type or member is obsolete
		_btnLogin = new Button
		{
			HorizontalOptions = LayoutOptions.FillAndExpand,
			AutomationId = "btnLogin",
			Text = "Press me",
			BackgroundColor = Color.FromUint(0xFF6E932D),
			TextColor = Colors.White,
		};
#pragma warning restore CS0618 // Type or member is obsolete
		_btnLogin.Clicked += BtnLogin_Clicked;
		layout.Children.Add(_btnLogin);

		_busyBackground = new BoxView
		{
			BackgroundColor = new Color(0, 0, 0, 0.5f),
			IsVisible = false,
			InputTransparent = false
		};

		// Bump up elevation on Android to cover FastRenderer Button
		((BoxView)_busyBackground).On<Microsoft.Maui.Controls.PlatformConfiguration.Android>().SetElevation(10f);

		layout.Children.Add(_busyBackground);

		Content = layout;
	}

	void BtnLogin_Clicked(object sender, EventArgs e)
	{

		if (!_busyBackground.IsVisible)
		{
			_btnLogin.Text = "Blocked?";
			_busyBackground.IsVisible = true;
		}
		else
		{
			_btnLogin.Text = "Guess Not";
			_busyBackground.IsVisible = false;
		}
	}
}
