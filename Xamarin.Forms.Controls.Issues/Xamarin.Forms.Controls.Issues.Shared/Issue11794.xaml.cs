using System;
using System.Collections.ObjectModel;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 11794, "[Bug] RefreshView with CollectionView child cannot contain GridItemsLayout with Span greater than 1 in combination with EmptyView on UWP",
		PlatformAffected.UWP)]
	public partial class Issue11794 : ContentPage
	{
		public Issue11794()
		{
#if APP
			InitializeComponent();
			BindingContext = new Issue11794ViewModel();
#endif
		}
	}

	[Preserve(AllMembers = true)]
	public class Issue11794ViewModel : BindableObject
	{
		ObservableCollection<string> _items;

		public Issue11794ViewModel()
		{
			LoadItems();
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

		void LoadItems()
		{
			Items = new ObservableCollection<string>
			{
				"Item 1",
				"Item 2",
				"Item 3"
			};
		}
	}
}