using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 23029, "RefreshView doesn't use correct layout mechanisms", PlatformAffected.All)]
	public partial class Issue23029 : ContentPage
	{
		public Issue23029()
		{
			InitializeComponent();
			ObservableCollection<AssessmentModel> list = [];
			for (int i = 0; i < 100; i++)
			{
				list.Add(new AssessmentModel()
				{
					ProgramName = "International Assessment " + i.ToString(),
					CourseName = "English " + i.ToString(),
					DueDate = DateTime.Now.ToShortDateString()
				});
			}
			CView.ItemsSource = list;

			CView.SizeChanged += OnSizeChanged;
		}

		private void OnSizeChanged(object sender, EventArgs e)
		{
			if (CView.Frame.Height > 0 && CView.Width > 0 && grid.Children.Count == 1)
			{
				// Works around a bug with modifying the grid while it's being measured
				this.Dispatcher.Dispatch(() =>
				{
					grid.Children.Insert(0, new Label() { Text = "Size Changed", AutomationId = "SizeChangedLabel" });
				});
			}

			if (sender is CollectionView collectionView && collectionView.ItemsLayout is GridItemsLayout gridItemsLayout)
			{
				double maxWidthPerItem = 300;
				gridItemsLayout.Span = Math.Max(1, (int)(collectionView.Width / maxWidthPerItem));
			}
		}

		public class AssessmentModel
		{
			public string ProgramName { get; set; } = string.Empty;
			public string CourseName { get; set; } = string.Empty;
			public string DueDate { get; set; } = string.Empty;
		}
	}
}
