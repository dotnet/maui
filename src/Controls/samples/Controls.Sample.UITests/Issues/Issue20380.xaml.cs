using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 20380, "Pickers should not scroll and should not focus while hidden.", PlatformAffected.iOS)]
	public partial class Issue20380 : ContentPage
	{
		public Issue20380()
		{
			InitializeComponent();
		}

		void Button_Clicked_1(System.Object sender, System.EventArgs e)
		{
			datePickerVisible.Focus();
		}

		void Button_Clicked_2(System.Object sender, System.EventArgs e)
		{
			datePickerHidden.Focus();
		}

		void Button_Clicked_3(System.Object sender, System.EventArgs e)
		{
			timeHidden.Focus();
		}

		void Button_Clicked_4(System.Object sender, System.EventArgs e)
		{
			timeVisible.Focus();
		}
	}
}
