using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues
{
	// ClearingGroupedCollectionViewShouldNotCrash (src\Compatibility\ControlGallery\src\Issues.Shared\Issue8899.cs)
	[Issue(IssueTracker.None, 8899, "Clearing Grouped CollectionView crashes application")]
	public class ClearGroupedNoCrash : ContentPage
	{
		const string Go = "Go";
		const string Success = "Success";
		const string Running = "Running...";

		public ClearGroupedNoCrash()
		{
			var layout = new StackLayout();

			var instructions = new Label
			{
				Text = $"Tap the button marked '{Go}'. The CollectionView below should clear," +
				$" and the '{Running}' label should change to {Success}. If this does not happen, the test has failed."
			};
			var result = new Label { Text = "running..." };

			var viewModel = new ClearGroupedNoCrashViewModel();

			var button = new Button { Text = Go, AutomationId = Go };
			button.Clicked += (obj, args) =>
			{
				viewModel.Groups.Clear();
				result.Text = Success;
			};

			var cv = new CollectionView { };
			cv.GroupHeaderTemplate = new DataTemplate(() =>
			{
				var label = new Label { };
				label.SetBinding(Label.TextProperty, new Binding("GroupName"));
				return label;
			});
			cv.ItemTemplate = new DataTemplate(() =>
			{
				var label = new Label { };
				label.SetBinding(Label.TextProperty, new Binding("Text"));
				return label;
			});
			cv.IsGrouped = true;
			cv.ItemsSource = viewModel.Groups;

			layout.Children.Add(instructions);
			layout.Children.Add(result);
			layout.Children.Add(button);
			layout.Children.Add(cv);

			Content = layout;
		}

		public class ClearGroupedNoCrashViewModel
		{
			public ObservableCollection<ClearGroupedNoCrashGroup> Groups { get; set; }

			public ClearGroupedNoCrashViewModel()
			{
				Groups = new ObservableCollection<ClearGroupedNoCrashGroup>();
				for (int n = 0; n < 3; n++)
				{
					Groups.Add(new ClearGroupedNoCrashGroup(n));
				}
			}
		}

		public class ClearGroupedNoCrashGroup : List<ClearGroupedNoCrashItem>
		{
			public string GroupName { get; set; }

			public ClearGroupedNoCrashGroup(int n)
			{
				GroupName = $"Group {n}";

				Add(new ClearGroupedNoCrashItem { Text = $"Group {n} Item" });
			}
		}

		public class ClearGroupedNoCrashItem
		{
			public string Text { get; set; }
		}
	}
}