using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.None, 0, "Modal ContentPage", PlatformAffected.All, NavigationBehavior.PushModalAsync)]
	public class ModalContentPage : ContentPage
	{
		public ModalContentPage()
		{
			Title = "Test Modal";
			Content = new Button
			{
				Text = "I am button",
				VerticalOptions = LayoutOptions.CenterAndExpand,
				HorizontalOptions = LayoutOptions.CenterAndExpand
			};
		}
	}
}
