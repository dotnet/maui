using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Issues
{
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
	}
}
