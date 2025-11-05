using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 32359, 
		"FlowDirection RightToLeft not applied to CollectionView with VerticalGrid multi-column layout",
		PlatformAffected.iOS)]
	public partial class Issue32359 : ContentPage
	{
		private ObservableCollection<string> _items;

		public Issue32359()
		{
			InitializeComponent();
			
			// Create test data - using numbers to make column order clearly visible
			_items = new ObservableCollection<string>();
			for (int i = 1; i <= 20; i++)
			{
				_items.Add($"{i}");
			}
			
			TestCollectionView.ItemsSource = _items;
			
			// Wire up buttons
			SetRtlButton.Clicked += OnSetRtlClicked;
			SetLtrButton.Clicked += OnSetLtrClicked;
		}

		private void OnSetRtlClicked(object? sender, EventArgs e)
		{
			this.FlowDirection = FlowDirection.RightToLeft;
			StatusLabel.Text = "Current: RTL";
		}

		private void OnSetLtrClicked(object? sender, EventArgs e)
		{
			this.FlowDirection = FlowDirection.LeftToRight;
			StatusLabel.Text = "Current: LTR";
		}
	}
}
