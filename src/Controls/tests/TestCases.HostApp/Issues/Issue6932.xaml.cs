using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 6932, "EmptyView for BindableLayout (view)", PlatformAffected.All)]
	public partial class Issue6932 : TestContentPage
	{
		readonly Page6932ViewModel _viewModel = new Page6932ViewModel();

		public Issue6932()
		{
			InitializeComponent();
			BindingContext = _viewModel;
		}

		protected override void Init()
		{

		}
	}

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