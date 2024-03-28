using System;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

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

	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 17865, "CollectionView throws NRE when ScrollTo method is called from a handler of event Window.Created", PlatformAffected.UWP)]
	public partial class Issue17865 : ContentPage
	{
		readonly Issue17865ViewModel _viewModel;

		public Issue17865()
		{
			InitializeComponent();

			BindingContext = _viewModel = new Issue17865ViewModel();
			Instance = this;
		}

		public void RevealLastItem()
		{
			var item = _viewModel.Items.Last();
			collectionView.ScrollTo(item, null, ScrollToPosition.MakeVisible, false);
		}

		public static Issue17865 Instance { get; private set; }

		private void OnButtonClicked(object sender, EventArgs e)
		{
			RevealLastItem();
		}
	}
}