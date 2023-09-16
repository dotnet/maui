using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Xaml;

#if UITEST
using Xamarin.UITest;
using Xamarin.UITest.Queries;
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility.UITests;
using System.Linq;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(UITestCategories.CollectionView)]
	[NUnit.Framework.Category(UITestCategories.UwpIgnore)]
#endif
#if APP
	[XamlCompilation(XamlCompilationOptions.Compile)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 7803, "[Bug] CarouselView/RefreshView pull to refresh command firing twice on a single pull", PlatformAffected.All)]
	public partial class Issue7803 : TestContentPage
	{
#if APP
		public Issue7803()
		{

			InitializeComponent();

			BindingContext = new ViewModel7803();
		}
#endif

		protected override void Init()
		{

		}

#if UITEST
		[Test]
		[Compatibility.UITests.FailsOnMauiIOS]
		public void DelayedIsRefreshingAndCommandTest_SwipeDown()
		{
			var collectionView = RunningApp.WaitForElement(q => q.Marked("CollectionView7803"))[0];

			RunningApp.Pan(new Drag(collectionView.Rect, Drag.Direction.TopToBottom, Drag.DragLength.Medium));

			RunningApp.WaitForElement(q => q.Marked("Count: 20"));
			RunningApp.WaitForNoElement(q => q.Marked("Count: 30"));

			AppResult[] lastCellResults = null;

			RunningApp.QueryUntilPresent(() =>
			{
				RunningApp.DragCoordinates(collectionView.Rect.CenterX, collectionView.Rect.Y + collectionView.Rect.Height - 50, collectionView.Rect.CenterX, collectionView.Rect.Y + 5);

				lastCellResults = RunningApp.Query("19");

				return lastCellResults;
			}, 10, 1);

			Assert.IsTrue(lastCellResults?.Any() ?? false);
		}
#endif
	}

	[Preserve(AllMembers = true)]
	public class ViewModel7803 : INotifyPropertyChanged
	{
		public ObservableCollection<Model7803> Items { get; set; } = new ObservableCollection<Model7803>();

		private bool _isRefreshing;

		public bool IsRefreshing
		{
			get
			{
				return _isRefreshing;
			}
			set
			{
				_isRefreshing = value;

				OnPropertyChanged("IsRefreshing");
			}
		}

		private string _text;

		public string Text
		{
			get
			{
				return _text;
			}
			set
			{
				_text = value;

				OnPropertyChanged("Text");
			}
		}

		public Command RefreshCommand { get; set; }

		public ViewModel7803()
		{
			PopulateItems();

			RefreshCommand = new Command(async () =>
			{
				IsRefreshing = true;

				await Task.Delay(2000);
				PopulateItems();

				IsRefreshing = false;
			});
		}

		void PopulateItems()
		{
			var count = Items.Count;

			for (var i = count; i < count + 10; i++)
				Items.Add(new Model7803() { Position = i });

			Text = "Count: " + Items.Count;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged(string name)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}
	}

	[Preserve(AllMembers = true)]
	public class Model7803 : INotifyPropertyChanged
	{
		private int _position;

		public int Position
		{
			get
			{
				return _position;
			}
			set
			{
				_position = value;

				OnPropertyChanged("Position");
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged(string name)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}
	}
}
