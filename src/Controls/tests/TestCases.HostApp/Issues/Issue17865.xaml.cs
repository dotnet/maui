using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues
{
	internal class Issue17865Model
	{
		public Issue17865Model(int itemIndex)
		{
			ItemText = $"Item #{itemIndex}";
		}

		public string ItemText { get; }

	}

	internal class Issue17865ViewModel : BindableObject
	{
		ObservableCollection<Issue17865Model> items = new();

		public Issue17865ViewModel()
		{
			Populate();
		}

		void Populate()
		{
			for (int i = 0; i < 100; i++)
			{
				items.Add(new Issue17865Model(i));
			}
		}

		public ObservableCollection<Issue17865Model> Items => items;
	}

	[Issue(IssueTracker.Github, 17865, "CollectionView throws NRE when ScrollTo method is called from a handler of event Window.Created", PlatformAffected.UWP)]
	public partial class Issue17865 : ContentPage
	{
		readonly Issue17865ViewModel _viewModel;

		public Issue17865()
		{
			InitializeComponent();

			collectionView.Loaded += CollectionView_Loaded;

			BindingContext = _viewModel = new Issue17865ViewModel();
		}

		private void CollectionView_Loaded(object sender, EventArgs e)
		{
			RevealLastItem();
		}

		public void RevealLastItem()
		{
			var item = _viewModel.Items.Last();
			collectionView.ScrollTo(item, null, ScrollToPosition.MakeVisible, false);
		}

		private void OnButtonClicked(object sender, EventArgs e)
		{
			RevealLastItem();
		}
	}
}