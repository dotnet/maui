using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues;

[XamlCompilation(XamlCompilationOptions.Compile)]
[Issue(IssueTracker.Github, 22715, "Page should not scroll when focusing element above keyboard", PlatformAffected.iOS)]
public partial class Issue22715 : ContentPage
{
	public Issue22715()
	{
		InitializeComponent();
	}

	private void OnPageLoaded(object sender, EventArgs e)
	{
		EntNumber.Focus();
	}

	void EntNumber_Focused(object sender, FocusEventArgs e)
	{
#if IOS
		var entry = (Entry)sender;
		var field = entry.Handler?.PlatformView as UIKit.UITextField;
		if (field is not null)
		{
			field.TintColor = UIKit.UIColor.Clear;
		}
#endif
	}
}