using Android.Text;
using Android.Text.Method;
using Android.Views;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Platform;
using AView = Android.Views.View;

namespace Maui.Controls.Sample.Platform;

public class Issue21109NumericKeyListener : NumberKeyListener
{
	public override InputTypes InputType { get; }

	protected override char[] GetAcceptedChars() => "0123456789-,.".ToCharArray();

	public Issue21109NumericKeyListener(InputTypes inputType)
	{
		InputType = inputType;
	}

	public override bool OnKeyDown(AView view, IEditable content, Keycode keyCode, KeyEvent e)
	{
		((Microsoft.Maui.Controls.Window)view.Context.GetWindow()).Page.DisplayAlert("OnKeyDown", string.Empty, "Ok");
		return base.OnKeyDown(view, content, keyCode, e);
	}
}