using System.Diagnostics;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using NUnit.Framework;
using Xamarin.UITest;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	public class Person
	{
		public string Name { get; set; }

		public Person(string name)
		{
			Name = name;
		}
	}


	[Preserve(AllMembers = true)]
	public class CustomViewCell : ViewCell
	{
		public CustomViewCell()
		{
			int tapsFired = 0;

			Height = 50;

			var label = new Label
			{
				Text = "I have been selected:"
			};

			if (this is CustomViewCellBindingContext)
				label.Text = "If you can read this text the UI Test has failed";

			Tapped += (s, e) =>
			{
				tapsFired++;
				label.Text = "I have been selected:" + tapsFired;

				var cell = (CustomViewCell)s;
			};

			View = label;
		}
	}


	[Preserve(AllMembers = true)]
	public class CustomViewCellBindingContext : CustomViewCell
	{
		public CustomViewCellBindingContext()
		{
		}
	}

#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.UwpIgnore)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 935, "ViewCell.ItemTapped only fires once for ListView.SelectedItem", PlatformAffected.Android)]
	public class Issue935 : TestContentPage
	{
		protected override void Init()
		{
			Title = "List Page";

			var items = new[] {
				new CustomViewCellBindingContext()
			};

			var cellTemplate = new DataTemplate(typeof(CustomViewCell));

			var list = new ListView()
			{
				ItemTemplate = cellTemplate,
				ItemsSource = items
			};

			Content = list;
		}

#if UITEST
		[Test]
		[Description ("Verify that OnTapped is fired every time a ViewCell is tapped")]
		[UiTest (typeof(ViewCell))]
		[UiTest (typeof(ListView))]
		[UiTest (typeof(ListView), "SelectedItem")]
		[Compatibility.UITests.FailsOnMauiIOS]
		public void Issue935TestsMultipleOnTappedViewCell ()
		{
			RunningApp.Tap (q => q.Marked ("I have been selected:"));
			RunningApp.Screenshot ("Tapped Cell Once");
			RunningApp.Tap (q => q.Marked ("I have been selected:1"));
			RunningApp.WaitForElement (q => q.Marked ("I have been selected:2"));
			RunningApp.Screenshot ("Tapped Cell Twice");
		}
#endif

	}
}
