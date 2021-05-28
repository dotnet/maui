using System.Collections.ObjectModel;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

#if UITEST
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility.UITests;
#endif

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 9833, "[Bug] [UWP] Propagate CollectionView BindingContext to EmptyView",
		PlatformAffected.UWP)]
#if UITEST
	[Category(UITestCategories.CollectionView)]
#endif
	public class Issue9833 : TestContentPage
	{
		readonly CollectionView _collectionView;
		readonly Label _emptyLabel;

		public Issue9833()
		{
			Title = "Issue 9833";

			BindingContext = new Issue9833ViewModel();

			var layout = new StackLayout
			{
				Padding = 0
			};

			var instructions = new Label
			{
				Text = "If the EmptyView BindingContext is not null, the test has passed.",
				BackgroundColor = Colors.Black,
				TextColor = Colors.White
			};

			_collectionView = new CollectionView();
			_collectionView.SetBinding(ItemsView.ItemsSourceProperty, "Items");

			var emptyView = new StackLayout
			{
				BackgroundColor = Colors.LightGray
			};

			_emptyLabel = new Label
			{
				Text = "This is the EmptyView. "
			};

			emptyView.Children.Add(_emptyLabel);

			_collectionView.EmptyView = emptyView;

			layout.Children.Add(instructions);
			layout.Children.Add(_collectionView);

			Content = layout;
		}

		protected override void Init()
		{

		}

		protected override void OnAppearing()
		{
			base.OnAppearing();

			var emptyView = (View)_collectionView.EmptyView;
			_emptyLabel.Text += $"BindingContext = {emptyView.BindingContext}";
		}
	}

	[Preserve(AllMembers = true)]
	public class Issue9833ViewModel : BindableObject
	{
		ObservableCollection<string> _items;

		public Issue9833ViewModel()
		{
			LoadData();
		}

		public ObservableCollection<string> Items
		{
			get { return _items; }
			set
			{
				_items = value;
				OnPropertyChanged();
			}
		}

		void LoadData()
		{
			Items = new ObservableCollection<string>();
		}
	}
}
