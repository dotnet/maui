using System;

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
	[Issue(IssueTracker.Bugzilla, 36009, "Children of Layouts with data bound IsVisible are not displayed")]
	public class Bugzilla36009 : TestContentPage // or TestFlyoutPage, etc ...
	{
		[Preserve(AllMembers = true)]
		public class SampleViewModel : ViewModelBase
		{
			public bool IsContentVisible
			{
				get { return GetProperty<bool>(); }
				set { SetProperty(value); }
			}
		}

		protected override void Init()
		{
			var boxview = new BoxView { BackgroundColor = Colors.Aqua, AutomationId = "Victory" };

			var contentView = new ContentView
			{
				Content = boxview
			};

			contentView.SetBinding(IsVisibleProperty, "IsContentVisible");

			var layout = new AbsoluteLayout
			{
				Children = { contentView }
			};

			Content = layout;

			var vm = new SampleViewModel();

			BindingContext = vm;

			vm.IsContentVisible = true;
		}

#if UITEST
[Microsoft.Maui.Controls.Compatibility.UITests.FailsOnMauiAndroid]
[Microsoft.Maui.Controls.Compatibility.UITests.FailsOnMauiIOS]
		[Test]
		public void Bugzilla36009Test ()
		{
			RunningApp.WaitForElement(q => q.Marked("Victory"));
		}
#endif
	}
}
