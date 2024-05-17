using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using CategoryAttribute = NUnit.Framework.CategoryAttribute;

// Thanks to GitHub user [@Matmork](https://github.com/Matmork) for this reproducible test case.
// https://github.com/xamarin/Xamarin.Forms/issues/9711#issuecomment-602520024

#if UITEST
using Microsoft.Maui.Controls.Compatibility.UITests;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 9711, "[Bug] iOS Failed to marshal the Objective-C object HeaderWrapperView", PlatformAffected.iOS)]
	public partial class Issue9711 : TestContentPage
	{
		protected override void Init()
		{
#if APP
			InitializeComponent();

			List<ListGroup<string>> groups = new List<ListGroup<string>>();
			for (int i = 0; i < 105; i++)
			{
				var group = new ListGroup<string> { Title = $"Group{i}" };
				for (int j = 0; j < 5; j++)
				{
					group.Add($"Group {i} Item {j}");
				}

				groups.Add(group);
			}

			TestListView.AutomationId = "9711TestListView";
			TestListView.ItemsSource = groups;
#endif
		}

		private void ViewCell_OnBindingContextChanged(object sender, EventArgs e)
		{
			if (sender is ViewCell cell && cell.BindingContext is ListGroup<string> list)
			{
				list.Cell = cell;
			}
		}

		private async void TapGestureRecognizer_OnTapped(object sender, EventArgs e)
		{
			if (sender is ContentView cnt && cnt.BindingContext is ListGroup<string> list)
			{
				for (int i = 0; i <= 50; i++)
				{
					await Task.Delay(25);
					list.IsExpanded = !list.IsExpanded;
				}
			}
		}


#if UITEST
		[Category(UITestCategories.ListView)]
		[Category(UITestCategories.UwpIgnore)]
		[Test]
		[Compatibility.UITests.FailsOnMauiIOS]
		public void TestTappingHeaderDoesNotCrash()
		{
			// Usually, tapping one header is sufficient to produce the exception.
			// However, sometimes it takes two taps, and rarely, three.  If the app
			// crashes, one of the RunningApp queries will throw, failing the test.
			Assert.DoesNotThrowAsync(async () =>
			{
				RunningApp.Tap(x => x.Marked("Group2"));
				await Task.Delay(3000);
				RunningApp.Tap(x => x.Marked("Group1"));
				await Task.Delay(3000);
				RunningApp.Tap(x => x.Marked("Group0"));
				await Task.Delay(3000);
				RunningApp.Query(x => x.Marked("9711TestListView"));
			});
		}
#endif
	}

	[Preserve(AllMembers = true)]
	public sealed class ListGroup<T> : List<T>, INotifyPropertyChanged, INotifyCollectionChanged
	{
		public string Title { get; set; }
		public string AutomationId => Title;
		private bool _isExpanded = true;

		public bool IsExpanded
		{
			get => _isExpanded;
			set
			{
				if (_isExpanded == value)
					return;

				if (Cell != null)
					Cell.Height = value ? 75 : 40;

				_isExpanded = value;
				OnPropertyChanged();
				OnCollectionChanged();
			}
		}

		public ViewCell Cell { get; set; }
		public event NotifyCollectionChangedEventHandler CollectionChanged;
		public event PropertyChangedEventHandler PropertyChanged;

		private void OnCollectionChanged()
		{
			CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}

		private void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
