using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Xamarin.Forms.Core.UITests;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.Layout)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 6932, "EmptyView for BindableLayout (string)", PlatformAffected.All, issueTestNumber: 2)]
	public partial class Issue6932_emptyviewstring : TestContentPage
	{
		readonly Page6932ViewModel _viewModel = new Page6932ViewModel();

		public Issue6932_emptyviewstring()
		{
#if APP
			InitializeComponent();
			BindingContext = _viewModel;

			BindableLayout.SetEmptyView(TheStack, _viewModel.EmptyViewStringDescription);
#endif
		}

		protected override void Init()
		{

		}

#if UITEST
		[Test]
		public void EmptyViewStringBecomesVisibleWhenItemsSourceIsCleared()
		{
			RunningApp.Screenshot("Screen opens, items are shown");

			RunningApp.WaitForElement(_viewModel.LayoutAutomationId);
			RunningApp.Tap(_viewModel.ClearAutomationId);
			RunningApp.WaitForElement(_viewModel.EmptyViewStringDescription);

			RunningApp.Screenshot("Empty view is visible");
		}

		[Test]
		public void EmptyViewStringBecomesVisibleWhenItemsSourceIsEmptiedOneByOne()
		{
			RunningApp.Screenshot("Screen opens, items are shown");

			RunningApp.WaitForElement(_viewModel.LayoutAutomationId);

			for (var i = 0; i < _viewModel.ItemsSource.Count; i++)
				RunningApp.Tap(_viewModel.RemoveAutomationId);

			RunningApp.WaitForElement(_viewModel.EmptyViewStringDescription);

			RunningApp.Screenshot("Empty view is visible");
		}

		[Test]
		public void EmptyViewStringHidesWhenItemsSourceIsFilled()
		{
			RunningApp.Screenshot("Screen opens, items are shown");

			RunningApp.WaitForElement(_viewModel.LayoutAutomationId);
			RunningApp.Tap(_viewModel.ClearAutomationId);
			RunningApp.WaitForElement(_viewModel.EmptyViewStringDescription);

			RunningApp.Screenshot("Items are cleared, empty view visible");

			RunningApp.Tap(_viewModel.AddAutomationId);
			RunningApp.WaitForNoElement(_viewModel.EmptyViewStringDescription);

			RunningApp.Screenshot("Item is added, empty view is not visible");
		}
#endif
	}
}