using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 21706, "ImageButton stuck in PointerOver state", PlatformAffected.UWP)]
	public partial class Issue21706 : ContentPage
	{
		public Issue21706()
		{
			InitializeComponent();
		}
	}
}