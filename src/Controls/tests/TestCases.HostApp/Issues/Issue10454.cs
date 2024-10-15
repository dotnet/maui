﻿using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Github, 10454, "CollectionView ChildAdded", PlatformAffected.All)]
	public class Issue10454 : TestContentPage
	{
		const string Success = "Success";

		protected override void Init()
		{
			Title = "Issue 10454";

			BindingContext = new Issue10454ViewModel();

			var layout = new StackLayout();

			var collectionView = new CollectionView();
			collectionView.SetBinding(ItemsView.ItemsSourceProperty, "Items");

			collectionView.ItemTemplate = new DataTemplate(() =>
			{
				var template = new DataTemplate();
				var content = new Grid
				{
					BackgroundColor = Colors.LightGray
				};
				var label = new Label();
				label.SetBinding(Label.TextProperty, ".");
				content.Children.Add(label);

				return content;
			});

			var labelInfo = new Label
			{
				FontSize = 18
			};

			var successLabel = new Label();

			layout.Children.Add(labelInfo);
			layout.Children.Add(successLabel);
			layout.Children.Add(collectionView);

			Content = layout;

			collectionView.ChildAdded += (sender, args) =>
			{
				labelInfo.Text = $"ChildAdded {args.Element}";
				Console.WriteLine(labelInfo.Text);

				successLabel.Text = Success;
			};

			collectionView.ChildRemoved += (sender, args) =>
			{
				labelInfo.Text = $"ChildRemoved  {args.Element}";
				Console.WriteLine(labelInfo.Text);
			};
		}
	}


	public class Issue10454ViewModel : BindableObject
	{
		public Issue10454ViewModel()
		{
			LoadItems();
		}

		public ObservableCollection<string> Items { get; set; }

		void LoadItems()
		{
			Items = new ObservableCollection<string>();

			for (int i = 0; i < 100; i++)
			{
				Items.Add($"Item {i + 1}");
			}
		}
	}
}