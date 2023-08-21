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

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Bugzilla)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 33890, "Setting CancelButtonColor does not have any effect", PlatformAffected.iOS)]
	public class Bugzilla33890 : TestContentPage
	{
		protected override void Init()
		{
			Label header = new Label
			{
				Text = "Search Bar",
				FontAttributes = FontAttributes.Bold,
				FontSize = 50,
				HorizontalOptions = LayoutOptions.Center
			};

			SearchBar searchBar = new SearchBar
			{
				Placeholder = "Enter anything",
				CancelButtonColor = Colors.Red
			};

			Label reproSteps = new Label
			{
				Text =
					"Tap on the search bar and enter some text. The 'Cancel' button should appear. If the 'Cancel' button is not red, this is broken.",
				HorizontalOptions = LayoutOptions.Center
			};

			Content = new StackLayout
			{
				Children = {
					header,
					searchBar,
					reproSteps
				}
			};
		}
	}
}