using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using Microsoft.Maui.Controls.Compatibility.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[Category(UITestCategories.CollectionView)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 7510, "CollectionView crashes on adding item", PlatformAffected.All)]
	public class Issue7510 : TestContentPage
	{
		public Issue7510()
		{
			Title = "Issue 7510";
			BindingContext = new Issue7510ModelViewModel();
		}

		protected override void OnAppearing()
		{
			if (this.BindingContext is Issue7510ModelViewModel vm)
			{
				vm.Init();
			}

			base.OnAppearing();
		}

		protected override void Init()
		{
			var infoLabel = new Label
			{
				Text = "If you press the button and there is no crash, everything goes correctly."
			};

			var addButton = new Button
			{
				Text = "Add items",
				AutomationId = "AddBtn"
			};

			addButton.SetBinding(Button.CommandProperty, "MoreCommand");

			var collectionView = new CollectionView
			{
				AutomationId = "Collection",
				SelectionMode = SelectionMode.None,
				ItemTemplate = CreateDataTemplate()
			};

			collectionView.SetBinding(ItemsView.ItemsSourceProperty, "Items");

			var stack = new StackLayout();

			stack.Children.Add(infoLabel);
			stack.Children.Add(addButton);
			stack.Children.Add(collectionView);

			Content = stack;
		}

		private DataTemplate CreateDataTemplate()
		{
			DataTemplate template = new DataTemplate(() =>
			{
				var grid = new Grid() { Padding = new Thickness(0), Margin = 0, RowSpacing = 0, ColumnSpacing = 0 };

				grid.RowDefinitions.Clear();
				grid.ColumnDefinitions.Clear();
				grid.Children.Clear();

				var cell = new Label();
				cell.SetBinding(Label.TextProperty, "Title");
				grid.Add(cell, 0, 0);

				return grid;
			});

			return template;
		}
	}

	[Preserve(AllMembers = true)]
	public class Issue7510Model
	{
		public Guid Id { get; set; }
		public string Title { get; set; }
	}

	[Preserve(AllMembers = true)]
	public class Issue7510ModelViewModel : BindableObject
	{
		private ObservableCollection<Issue7510Model> _items;

		public ObservableCollection<Issue7510Model> Items
		{
			get { return _items; }
			set
			{
				_items = value;
				OnPropertyChanged();
			}
		}

		public Command MoreCommand => new Command(async () => await LoadMore());

		private async Task LoadMore()
		{
			await Task.Delay(100);

			var postCount = Items.Count();

			for (var i = 0; i < 20; i++)
			{
				Items.Add(new Issue7510Model { Id = Guid.NewGuid(), Title = $"Added Item {postCount + i}" });
				Debug.WriteLine(postCount + i);
			}
		}

		internal void Init()
		{
			var posts = new List<Issue7510Model>();

			for (var i = 0; i < 20; i++)
			{
				posts.Add(new Issue7510Model { Id = Guid.NewGuid(), Title = $"Item {i}" });
			}

			Items = new ObservableCollection<Issue7510Model>(posts);
		}
	}
}