using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 1583, "NavigationPage.TitleIcon broken", PlatformAffected.Android | PlatformAffected.iOS | PlatformAffected.WinPhone)]
	public class Issue1583 : TestContentPage
	{
		protected override void Init()
		{
			Title = "Test";
			BackgroundColor = Colors.Pink;
			Content = new Label { Text = "Hello", AutomationId = "lblHello" };
			NavigationPage.SetTitleIconImageSource(this, "bank.png");
		}
	}
}

