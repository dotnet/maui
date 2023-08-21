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

using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 5724, "Use Android Fast Renderers by Default", PlatformAffected.Android)]
	public class Issue5724 : TestContentPage
	{
		public class CustomButton : Button { }
		public class CustomImage : Image { }
		public class CustomLabel : Label { }
		public class CustomFrame : Frame { }

		protected override void Init()
		{
			Content = new StackLayout
			{
				Children =
				{
					new CustomLabel
					{
						Text = "See if I'm here"
					},
					new CustomButton
					{
						Text = "See if I'm here"
					},
					new CustomFrame
					{
					},
					new CustomImage
					{
						Source = "coffee.png"
					},
				}
			};
		}
	}
}