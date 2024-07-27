using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Xaml;


namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.None, 5354, "[CollectionView] Updating the ItemsLayout type should refresh the layout", PlatformAffected.All)]
	public partial class Issue5354 : ContentPage
	{
		int count = 0;

		public Issue5354()
		{
			InitializeComponent();

			BindingContext = new ViewModel5354();
		}

		void ButtonClicked(object sender, EventArgs e)
		{
			var button = sender as Button;
			var stackLayout = button.Parent as StackLayout;
			var grid = stackLayout.Parent as Grid;
			var collectionView = grid.Children[1] as CollectionView;

			if (count % 2 == 0)
			{
				collectionView.ItemsLayout = new GridItemsLayout(ItemsLayoutOrientation.Vertical)
				{
					Span = 2,
					HorizontalItemSpacing = 5,
					VerticalItemSpacing = 5
				};

				button.Text = "Switch to linear layout";
			}
			else
			{
				collectionView.ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Vertical)
				{
					ItemSpacing = 5
				};

				button.Text = "Switch to grid layout";
			}

			++count;
		}
	}

	[Preserve(AllMembers = true)]
	public class ViewModel5354
	{
		public ObservableCollection<Model5354> Items { get; set; }

		public ViewModel5354()
		{
			var collection = new ObservableCollection<Model5354>();
			var pageSize = 50;

			for (var i = 0; i < pageSize; i++)
			{
				collection.Add(new Model5354
				{
					Text = "Image" + i,
					Source = i % 2 == 0 ?
					"groceries.png" :
					"dotnet_bot.png",
					AutomationId = "Image" + i
				});
			}

			Items = collection;
		}
	}

	[Preserve(AllMembers = true)]
	public class Model5354
	{
		public string Text { get; set; }

		public string Source { get; set; }

		public string AutomationId { get; set; }

		public Model5354()
		{

		}
	}
}