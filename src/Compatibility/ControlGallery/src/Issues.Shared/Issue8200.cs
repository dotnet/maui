using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Input;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility.UITests;
#endif

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
#if UITEST
	[Category(UITestCategories.CollectionView)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 8200, "CollectionView on iOS assumes INotifyCollectionChanged is an IList", PlatformAffected.iOS)]
	public class Issue8200 : TestContentPage
	{
		CollectionView _collectionView;

		public Issue8200()
		{
			Title = "Issue 8200";
			BindingContext = new Issue8200ViewModel();
		}

		protected override void Init()
		{
			var instructions = new Label
			{
				Text = "If you can see a CollectionView below with items, the test has passed."
			};

			var buttonsLayout = new StackLayout
			{
				Orientation = StackOrientation.Horizontal
			};

			var addButton = new Button
			{
				Text = "Add Item"
			};

			addButton.SetBinding(Button.CommandProperty, "AddItemCommand");

			buttonsLayout.Children.Add(addButton);

			_collectionView = new CollectionView
			{
				BackgroundColor = Color.LightGreen,
				ItemTemplate = CreateDataGridTemplate(),
				SelectionMode = SelectionMode.None
			};

			_collectionView.SetBinding(ItemsView.ItemsSourceProperty, "Items");

			var stack = new StackLayout();

			stack.Children.Add(instructions);
			stack.Children.Add(buttonsLayout);
			stack.Children.Add(_collectionView);

			Content = stack;
		}

		DataTemplate CreateDataGridTemplate()
		{
			var template = new DataTemplate(() =>
			{
				var grid = new Grid();
				var cell = new Label();
				cell.SetBinding(Label.TextProperty, "Text");
				cell.FontSize = 20;
				cell.BackgroundColor = Color.LightBlue;
				grid.Children.Add(cell);

				return grid;
			});

			return template;
		}
	}

	[Preserve(AllMembers = true)]
	public class Issue8200Model
	{
		public string Text { get; set; }
	}

	[Preserve(AllMembers = true)]
	public class Issue8200ViewModel : BindableObject
	{
		Issue8200Collection _items;

		public Issue8200ViewModel()
		{
			LoadItems();
		}

		public Issue8200Collection Items
		{
			get { return _items; }
			set
			{
				_items = value;
				OnPropertyChanged();
			}
		}

		public ICommand AddItemCommand => new Command(AddItem);

		void LoadItems()
		{
			Items = new Issue8200Collection();

			for (int i = 0; i < 30; i++)
			{
				Items.AddNewItem(new Issue8200Model { Text = i.ToString() });
			}
		}

		void AddItem()
		{
			var itemsCount = Items.Count();
			Items.AddNewItem(new Issue8200Model { Text = itemsCount.ToString() });
		}
	}

	public class Issue8200Collection : IEnumerable<Issue8200Model>, INotifyCollectionChanged
	{
		readonly List<Issue8200Model> _internalList = new List<Issue8200Model>();

		public IEnumerable<Issue8200Model> GetItems()
		{
			foreach (var item in _internalList)
			{
				yield return item;
			}
		}

		public IEnumerator<Issue8200Model> GetEnumerator()
		{
			return GetItems().GetEnumerator();
		}

		public void AddNewItem(Issue8200Model newItem)
		{
			int index = _internalList.Count;
			_internalList.Add(newItem);
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, newItem, index));
		}

		public event NotifyCollectionChangedEventHandler CollectionChanged;

		protected void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
		{
			CollectionChanged?.Invoke(this, e);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}