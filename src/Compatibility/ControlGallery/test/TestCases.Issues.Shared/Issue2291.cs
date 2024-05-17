using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Github5000)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 2291, "DatePicker.IsVisible = false issue", PlatformAffected.WinPhone)]
	public class Issue2291 : ContentPage
	{
		public Issue2291()
		{
			var btnTest = new Button
			{
				Text = "Fundo"
			};

			var dtPicker = new DatePicker
			{
				IsVisible = false
			};

			Content = new StackLayout
			{
				Children = {
					btnTest,
					dtPicker
				}
			};

			btnTest.Clicked += (s, e) =>
			{
				dtPicker.Focus();
			};
		}
	}
}
