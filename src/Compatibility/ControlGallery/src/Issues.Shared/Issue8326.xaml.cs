using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Xaml;

#if UITEST
using Xamarin.UITest;
using Xamarin.UITest.Queries;
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility.UITests;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[Category(UITestCategories.CollectionView)]
#endif
#if APP
	[XamlCompilation(XamlCompilationOptions.Compile)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 8326, "[Bug] CollectionView: empty view doesn't show if collection view contains a header", PlatformAffected.Android)]
	public partial class Issue8326 : TestContentPage
	{
#if APP
		public Issue8326()
		{
			InitializeComponent();
			BindingContext = new Issue8326ViewModel();
		}
#endif

		protected override void Init()
		{
			Title = "Issue 8326";
		}
	}

	[Preserve(AllMembers = true)]
	public class Issue8326Model
	{
		public string Text { get; set; }
	}

	[Preserve(AllMembers = true)]
	public class Issue8326ViewModel : BindableObject
	{
		ObservableCollection<Issue8326Model> _items;

		public Issue8326ViewModel()
		{
			Items = new ObservableCollection<Issue8326Model>();
		}

		public ObservableCollection<Issue8326Model> Items
		{
			get { return _items; }
			set
			{
				_items = value;
				OnPropertyChanged();
			}
		}

		public ICommand ClearItemsCommand => new Command(ClearItems);
		public ICommand AddItemsCommand => new Command(AddItems);

		void ClearItems()
		{
			Items.Clear();
		}

		void AddItems()
		{
			for (int i = 0; i < 20; i++)
			{
				Items.Add(new Issue8326Model { Text = $"Item {i + 1}" });
			}
		}
	}
}