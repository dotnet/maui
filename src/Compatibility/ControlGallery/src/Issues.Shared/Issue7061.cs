//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Android.App;
//using Android.Content;
//using Android.Content.PM;
//using Android.OS;
//using Android.Renderscripts;
//using Android.Runtime;
//using Android.Views;
//using Android.Widget;
//using Microsoft.Maui.Controls.ControlGallery.Issues;

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 7061, "[Bug] NullReferenceException Closing Window During Animation", PlatformAffected.UWP)]
	public class Issue7061 : TestContentPage
	{
		Label _animatedLabel = new Label { Text = "Scaling out" };
		Label _explanationLabel = new Label { Text = "When you close the app while the animation is still running you should not get an exception" };

		protected override void Init()
		{
			var stack = new StackLayout
			{
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center
			};

			stack.Children.Add(_explanationLabel);
			stack.Children.Add(_animatedLabel);

			Content = stack;
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();

			_animatedLabel.ScaleTo(10, length: (uint)TimeSpan.FromMinutes(1).TotalMilliseconds);
		}
	}
}
