using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Platform;
using Microsoft.Maui;
using System.Linq;
using System;

#if IOS
using UIKit;
using CoreGraphics;
#endif

namespace Maui.Controls.Sample.Issues
{
	//[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 12604, "Setter showing a Build Error when using XAML OnPlatform Markup Extension", PlatformAffected.All)]
	public partial class Issue12604 : ContentPage
	{
		public Issue12604()
		{
			InitializeComponent();
		}
	}
}
