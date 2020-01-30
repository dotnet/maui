using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using NUnit.Framework;
using Xamarin.Forms.Core.UITests;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.CollectionView)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 8203, 
	"CollectionView fires SelectionChanged x (number of items selected +1) times, while incrementing SelectedItems from 0 " +
	"to number of items each time", 
	PlatformAffected.UWP)]

	public class Issue8203 : TestContentPage
	{
		int _raisedCount;
		Label _eventRaisedCount;

		protected override void Init()
		{
			var instructions = new Label { Text = "Select an item below. Then select another one. The SelectionChanged " +
				"event should have been raised twice. If not, this test has failed." };

			_eventRaisedCount = new Label();

			var layout = new StackLayout();
			var cv = new CollectionView();

			var source = new List<string> { "one", "two", "three" };

			cv.ItemsSource = source;
			cv.SelectionMode = SelectionMode.Multiple;

			cv.SelectionChanged += SelectionChangedHandler;

			layout.Children.Add(instructions);
			layout.Children.Add(_eventRaisedCount);
			layout.Children.Add(cv);

			Content = layout;
		}

		void UpdateRaisedCount() 
		{
			_eventRaisedCount.Text = $"SelectionChanged has been raised {_raisedCount} times.";
		}

		void SelectionChangedHandler(object sender, SelectionChangedEventArgs e)
		{
			_raisedCount += 1;
			UpdateRaisedCount();
		}

#if UITEST
		[Test]
		public void SelectionChangedShouldBeRaisedOnceWhenSelectionChanges()
		{
			RunningApp.WaitForElement("one");
			RunningApp.Tap("one");
			RunningApp.Tap("two");
			RunningApp.WaitForElement("SelectionChanged has been raised 2 times.");
		}
#endif
	}
}
