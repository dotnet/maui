using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Input;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Xaml;
#if UITEST && __WINDOWS__
using Xamarin.UITest;
using Microsoft.Maui.Controls.Compatibility.UITests;
using NUnit.Framework;
#endif


namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
#if APP
	[XamlCompilation(XamlCompilationOptions.Compile)]
#endif

	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 7167,
		"[Bug] improved observablecollection. a lot of collectionchanges. a reset is sent and listview scrolls to the top", PlatformAffected.UWP)]
	public partial class Issue7167 : TestContentPage
	{

		protected override void Init()
		{
#if APP
			InitializeComponent();
#endif
			BindingContext = new Issue7167ViewModel();
		}

		void MyListView_OnItemSelected(object sender, SelectedItemChangedEventArgs e)
		{
			var item = e.SelectedItem;
			var index = e.SelectedItemIndex;

		}

#if UITEST && __WINDOWS__
		const string ListViewId = "ListViewId";
		const string AddCommandID = "AddCommandID";
		const string ClearListCommandId = "ClearListCommandId";
		const string AddRangeCommandId = "AddRangeCommandId";
		const string AddRangeWithCleanCommandId = "AddRangeWithCleanCommandId";

		[Test]
		[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.UwpIgnore)]
		public  void Issue7167Test()
		{
			// arrange
			// add items to the list and scroll down till item "25"
			RunningApp.Screenshot("Empty ListView");
			RunningApp.Tap(q => q.Button(AddRangeCommandId));
			RunningApp.Tap(q => q.Button(AddRangeCommandId));
			RunningApp.WaitForElement(c => c.Index(25).Property("Enabled", true));
			RunningApp.Print.Tree();
			RunningApp.ScrollDownTo(a => a.Marked("25").Property("text").Contains("25"),
				b => b.Marked(ListViewId), ScrollStrategy.Auto);
			RunningApp.WaitForElement(x => x.Marked("25"));

			// act
			// when adding additional items via a addrange and a CollectionChangedEventArgs.Action.Reset is sent
			// then the listview shouldnt reset or it should not scroll to the top
			RunningApp.Tap(q => q.Marked(AddRangeCommandId));

			// assert
			// assert that item "25" is still visible
			var result = RunningApp.Query(x => x.Marked(ListViewId).Child().Marked("25"));
			Assert.That(result?.Length <= 0);
		}
#endif


	}

	[Preserve(AllMembers = true)]
	internal class Issue7167ViewModel
	{
		IEnumerable<string> CreateItems()
		{
			var itemCount = Items.Count();
			return Enumerable.Range(itemCount, 50).Select(num => num.ToString());
		}

		public ImprovedObservableCollection<string> Items { get; set; } = new ImprovedObservableCollection<string>();

		public ICommand AddCommand => new Command(_ => Items.Add(CreateItems().First()));
		public ICommand ClearListCommand => new Command(_ => Items.Clear());
		public ICommand AddRangeCommand => new Command(_ => Items.AddRange(CreateItems()));
		public ICommand AddRangeWithCleanCommand => new Command(_ =>
		{
			Items.Clear();
			Items.AddRange(CreateItems());
		});

	}

	[Preserve(AllMembers = true)]
	internal class ImprovedObservableCollection<T> : ObservableCollection<T>
	{
		bool _isActivated = true;
		public void AddRange(IEnumerable<T> source)
		{
			_isActivated = false;

			foreach (var item in source)
			{
				base.Items.Add(item);
			}

			_isActivated = true;

			this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}

		protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
		{
			if (_isActivated)
				base.OnCollectionChanged(e);
		}

	}



}


