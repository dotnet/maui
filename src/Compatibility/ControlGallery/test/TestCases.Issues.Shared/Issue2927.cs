using System;
using System.ComponentModel;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Github5000)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 2927, "ListView item tapped not firing multiple times")]
	public class Issue2927 : TestContentPage // or TestFlyoutPage, etc .
	{
		[Preserve(AllMembers = true)]
		public class Issue2927Cell : TextCell, INotifyPropertyChanged
		{
			int _numberOfTimesTapped;
			string _cellId;

			public Issue2927Cell(string id)
			{
				_cellId = id;
				NumberOfTimesTapped = 0;
			}

			public int NumberOfTimesTapped
			{
				get { return _numberOfTimesTapped; }
				set
				{
					_numberOfTimesTapped = value;
					Text = _cellId + " " + _numberOfTimesTapped.ToString();
				}
			}
		}

		protected override void Init()
		{
			var cells = new[] {
				new Issue2927Cell ("Cell1"),
				new Issue2927Cell ("Cell2"),
				new Issue2927Cell ("Cell3"),
			};

			BindingContext = cells;
			var template = new DataTemplate(typeof(TextCell));
			template.SetBinding(TextCell.TextProperty, "Text");

			var listView = new ListView
			{
				ItemTemplate = template,
				ItemsSource = cells
			};

			listView.ItemTapped += (s, e) =>
			{
				var obj = (Issue2927Cell)e.Item;
				obj.NumberOfTimesTapped += 1;
			};

			Content = listView;
		}

#if UITEST
		[Test]
		[Compatibility.UITests.FailsOnMauiIOS]
		public void Issue2927Test()
		{
			RunningApp.Screenshot("I am at Issue 2927");
			RunningApp.WaitForElement(q => q.Marked("Cell1 0"));
			RunningApp.Tap(q => q.Marked("Cell1 0"));
			RunningApp.WaitForElement(q => q.Marked("Cell1 1"));
			RunningApp.Screenshot("Tapped Once");
			RunningApp.Tap(q => q.Marked("Cell1 1"));
			RunningApp.WaitForElement(q => q.Marked("Cell1 2"));
			RunningApp.Screenshot("Tapped Twice");
			RunningApp.Tap(q => q.Marked("Cell3 0"));
			RunningApp.WaitForElement(q => q.Marked("Cell3 1"));
			RunningApp.Screenshot("Click other cell");
			RunningApp.Tap(q => q.Marked("Cell1 2"));
			RunningApp.WaitForElement(q => q.Marked("Cell1 3"));
			RunningApp.Screenshot("Click first cell again");
		}
#endif
	}
}
