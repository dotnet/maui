using System;

using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.Bugzilla, 36009, "Children of Layouts with data bound IsVisible are not displayed")]
	public class Bugzilla36009 : TestContentPage // or TestMasterDetailPage, etc ...
	{
		[Preserve (AllMembers = true)]
		public class SampleViewModel: ViewModelBase
		{
			public bool IsContentVisible {
				get{ return GetProperty<bool> (); }
				set{ SetProperty (value); }
			}
		}

		protected override void Init ()
		{
			var boxview = new BoxView{ BackgroundColor = Color.Aqua, AutomationId = "Victory" };

			var contentView = new ContentView { 
				Content = boxview
			};

			contentView.SetBinding (IsVisibleProperty, "IsContentVisible");

			var layout = new AbsoluteLayout {
				Children = { contentView }
			};

			Content = layout;

			var vm = new SampleViewModel ();

			BindingContext = vm;

			vm.IsContentVisible = true;
		}

#if UITEST
		[Test]
		public void Bugzilla36009Test ()
		{
			RunningApp.WaitForElement(q => q.Marked("Victory"));
		}
#endif
	}
}
