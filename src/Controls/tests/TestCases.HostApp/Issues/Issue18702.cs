using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Github, 18702, "When the CollectionView has a group footer template it should not crash the application")]
	public class Issue18702 : TestContentPage
	{
		protected override void Init()
		{
			var layout = new StackLayout();
			var bindingContext = new _18702viewModel();
			var collectionView = new CollectionView();
			collectionView.BindingContext = bindingContext;
			collectionView.IsGrouped = true;
			collectionView.AutomationId = "collectionView";

			collectionView.ItemTemplate = new DataTemplate(() =>
			{
				var border = new Border
				{
					BackgroundColor = Colors.Red,
					HeightRequest = 20
				};

				var label = new Label();
				label.SetBinding(Label.TextProperty, ".");

				border.Content = label;

				return border;
			});

			collectionView.GroupHeaderTemplate = new DataTemplate(() =>
			{
				var border = new Border
				{
					BackgroundColor = Colors.Yellow,
					HeightRequest = 20
				};

				var label = new Label();
				label.SetBinding(Label.TextProperty, "Key");

				border.Content = label;

				return border;
			});

			collectionView.GroupFooterTemplate = new DataTemplate(() =>
			{
				var border = new Border
				{
					BackgroundColor = Colors.Green,
					HeightRequest = 20
				};

				var label = new Label();
				label.SetBinding(Label.TextProperty, "Total");

				border.Content = label;

				return border;
			});

			// Set the ItemsSource binding
			collectionView.SetBinding(CollectionView.ItemsSourceProperty, "Data");
			layout.Children.Add(collectionView);

			Content = layout;

		}

		public class _18702viewModel
		{
			public ObservableCollection<_18702Group> Data { get; set; } = [new("group A", [1, 2, 3])];
		}


		public class _18702Group : Grouping<string, int>
		{
			public _18702Group(string key, IEnumerable<int> items) : base(key, items)
			{
			}

			public int Total => Items.Sum();
		}


		public class Grouping<TKey, TItem> : ObservableRangeCollection<TItem>, INotifyPropertyChanged
		{
			public TKey Key { get; }

			public new ICollection<TItem> Items => base.Items;

			public Grouping(TKey key, IEnumerable<TItem> items)
			{
				Key = key;
				AddRange(items);
			}
		}

		/// <summary> 
		/// Represents a dynamic data collection that provides notifications when items get added, removed, or when the whole list is refreshed. 
		/// </summary> 
		/// <typeparam name="T"></typeparam> 

		public class ObservableRangeCollection<T> : ObservableCollection<T>
		{

			/// <summary> 
			/// Initializes a new instance of the System.Collections.ObjectModel.ObservableCollection(Of T) class. 
			/// </summary> 
			public ObservableRangeCollection()
				: base()
			{
			}

			/// <summary> 
			/// Initializes a new instance of the System.Collections.ObjectModel.ObservableCollection(Of T) class that contains elements copied from the specified collection. 
			/// </summary> 
			/// <param name="collection">collection: The collection from which the elements are copied.</param> 
			/// <exception cref="System.ArgumentNullException">The collection parameter cannot be null.</exception> 
			public ObservableRangeCollection(IEnumerable<T> collection)
				: base(collection)
			{
			}

			/// <summary> 
			/// Adds the elements of the specified collection to the end of the ObservableCollection(Of T). 
			/// </summary> 
			public void AddRange(IEnumerable<T> collection, NotifyCollectionChangedAction notificationMode = NotifyCollectionChangedAction.Add)
			{
				if (collection == null)
					throw new ArgumentNullException("collection");

				CheckReentrancy();

				if (notificationMode == NotifyCollectionChangedAction.Reset)
				{
					foreach (var i in collection)
					{
						Items.Add(i);
					}

					OnPropertyChanged(new PropertyChangedEventArgs("Count"));
					OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
					OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));

					return;
				}

				int startIndex = Count;
				var changedItems = collection is List<T> ? (List<T>)collection : new List<T>(collection);
				foreach (var i in changedItems)
				{
					Items.Add(i);
				}

				OnPropertyChanged(new PropertyChangedEventArgs("Count"));
				OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
				OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, changedItems, startIndex));
			}

			/// <summary> 
			/// Removes the first occurrence of each item in the specified collection from ObservableCollection(Of T). 
			/// </summary> 
			public void RemoveRange(IEnumerable<T> collection)
			{
				if (collection == null)
					throw new ArgumentNullException("collection");

				foreach (var i in collection)
					Items.Remove(i);
				OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
			}

			/// <summary> 
			/// Clears the current collection and replaces it with the specified item. 
			/// </summary> 
			public void Replace(T item)
			{
				ReplaceRange(new T[] { item });
			}

			/// <summary> 
			/// Clears the current collection and replaces it with the specified collection. 
			/// </summary> 
			public void ReplaceRange(IEnumerable<T> collection)
			{
				if (collection == null)
					throw new ArgumentNullException("collection");

				Items.Clear();
				AddRange(collection, NotifyCollectionChangedAction.Reset);
			}
		}
	}
}
