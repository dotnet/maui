using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Maui.CustomAttributes;
using System.Maui.Internals;

namespace System.Maui.Controls.Issues
{
	[Preserve (AllMembers=true)]
	[Issue (IssueTracker.Github, 2291, "DatePicker.IsVisible = false issue", PlatformAffected.WinPhone)]
	public class Issue2291 : ContentPage
	{
		public Issue2291 ()
		{
			var btnTest = new Button {
				Text = "Fundo"
			};

			var dtPicker = new DatePicker {
				IsVisible = false
			};

			Content = new StackLayout {
				Children = {
					btnTest,
					dtPicker
				}
			};

			btnTest.Clicked += (s, e) => { 
				dtPicker.Focus (); 
			};
		}
	}
}
