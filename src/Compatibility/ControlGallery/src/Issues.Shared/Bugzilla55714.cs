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
using Microsoft.Maui.Graphics;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Bugzilla)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 55714, "[UWP] Cannot set Editor text color", PlatformAffected.UWP)]
	public class Bugzilla55714 : TestContentPage
	{
		protected override void Init()
		{
			var editor = new Editor
			{
				TextColor = Colors.Yellow,
				BackgroundColor = Colors.Black
			};

			Content = new StackLayout
			{
				VerticalOptions = LayoutOptions.Center,
				Children =
				{
					new Label
					{
						Text = "The below Editor should have visible yellow text when entered"
					},
					editor,
					new Button
					{
						Text = "Change Editor text color to white",
						Command = new Command(() => editor.TextColor = Colors.White)
					}
				}
			};
		}
	}
}