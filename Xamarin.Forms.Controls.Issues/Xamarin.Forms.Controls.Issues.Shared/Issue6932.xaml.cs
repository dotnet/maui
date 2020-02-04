using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
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
	[Issue(IssueTracker.Github, 6932, "EmptyView for BindableLayout (view)", PlatformAffected.All)]
	public partial class Issue6932 : TestContentPage
	{
		readonly Page6932ViewModel _viewModel = new Page6932ViewModel();

		public Issue6932()
		{
#if APP
			InitializeComponent();
			BindingContext = _viewModel;
#endif
		}

		protected override void Init()
		{

		}

#if UITEST
		[Test]
		public void EmptyViewBecomesVisibleWhenItemsSourceIsCleared()
		{
			RunningApp.Screenshot("Screen opens, items are shown");

			RunningApp.WaitForElement(_viewModel.LayoutAutomationId);
			RunningApp.Tap(_viewModel.ClearAutomationId);
			RunningApp.WaitForElement(_viewModel.EmptyViewAutomationId);

			RunningApp.Screenshot("Empty view is visible");
		}

		[Test]
		public void EmptyViewBecomesVisibleWhenItemsSourceIsEmptiedOneByOne()
		{
			RunningApp.Screenshot("Screen opens, items are shown");

			RunningApp.WaitForElement(_viewModel.LayoutAutomationId);

			for (var i = 0; i < _viewModel.ItemsSource.Count; i++)
				RunningApp.Tap(_viewModel.RemoveAutomationId);

			RunningApp.WaitForElement(_viewModel.EmptyViewAutomationId);

			RunningApp.Screenshot("Empty view is visible");
		}

		[Test]
		public void EmptyViewHidesWhenItemsSourceIsFilled()
		{
			RunningApp.Screenshot("Screen opens, items are shown");

			RunningApp.WaitForElement(_viewModel.LayoutAutomationId);
			RunningApp.Tap(_viewModel.ClearAutomationId);
			RunningApp.WaitForElement(_viewModel.EmptyViewAutomationId);

			RunningApp.Screenshot("Items are cleared, empty view visible");

			RunningApp.Tap(_viewModel.AddAutomationId);
			RunningApp.WaitForNoElement(_viewModel.EmptyViewAutomationId);

			RunningApp.Screenshot("Item is added, empty view is not visible");
		}
#endif
	}

	[Preserve(AllMembers = true)]
	public class Page6932ViewModel
	{
		public string LayoutAutomationId { get => "StackLayoutThing"; }
		public string AddAutomationId { get => "AddButton"; }
		public string RemoveAutomationId { get => "RemoveButton"; }
		public string ClearAutomationId { get => "ClearButton"; }
		public string EmptyViewAutomationId { get => "EmptyViewId"; }
		public string EmptyTemplateAutomationId { get => "EmptyTemplateId"; }
		public string EmptyViewStringDescription { get => "Nothing to see here"; }

		public ObservableCollection<object> ItemsSource { get; set; }
		public ICommand AddItemCommand { get; }
		public ICommand RemoveItemCommand { get; }
		public ICommand ClearCommand { get; }

		public Page6932ViewModel()
		{
			ItemsSource = new ObservableCollection<object>(Enumerable.Range(0, 10).Cast<object>().ToList());

			int i = ItemsSource.Count;
			AddItemCommand = new Command(() => ItemsSource.Add(i++));
			RemoveItemCommand = new Command(() =>
			{
				if (ItemsSource.Count > 0)
					ItemsSource.RemoveAt(0);
			});

			ClearCommand = new Command(() =>
			{
				ItemsSource.Clear();
			});
		}
	}
}