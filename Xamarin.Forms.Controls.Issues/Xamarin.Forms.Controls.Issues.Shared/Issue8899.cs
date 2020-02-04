using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.Forms.Core.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 8899, "Clearing CollectionView IsGrouped=\"True\" crashes application iOS ")]
	public class Issue8899 : TestContentPage
	{
		const string Go = "Go";
		const string Success = "Success";
		const string Running = "Running...";

		protected override void Init()
		{
			var layout = new StackLayout();

			var instructions = new Label { Text = $"Tap the button marked '{Go}'. The CollectionView below should clear," +
				$" and the '{Running}' label should change to {Success}. If this does not happen, the test has failed."};
			var result = new Label { Text = "running..." };

			var viewModel = new _8899ViewModel();

			var button = new Button { Text = Go, AutomationId = Go };
			button.Clicked += (obj, args) => {
				viewModel.Groups.Clear();
				result.Text = Success;
			};

			var cv = new CollectionView { };
			cv.GroupHeaderTemplate = new DataTemplate(() => {
				var label = new Label { };
				label.SetBinding(Label.TextProperty, new Binding("GroupName"));
				return label;
			});
			cv.ItemTemplate = new DataTemplate(() => {
				var label = new Label { };
				label.SetBinding(Label.TextProperty, new Binding("Text"));
				return label;
			});
			cv.IsGrouped = true;
			cv.ItemsSource = viewModel.Groups;

			layout.Children.Add(instructions);
			layout.Children.Add(result);
			layout.Children.Add(button);
			layout.Children.Add(cv);

			Content = layout;
		}

		[Preserve(AllMembers = true)]
		public class _8899ViewModel
		{ 
			public ObservableCollection<_8899Group> Groups { get; set; }

			public _8899ViewModel() 
			{
				Groups = new ObservableCollection<_8899Group>();
				for (int n = 0; n < 3; n++)
				{
					Groups.Add(new _8899Group(n));
				}
			}
		}

		[Preserve(AllMembers = true)]
		public class _8899Group : List<_8899Item>
		{ 
			public string GroupName { get; set; }

			public _8899Group(int n) 
			{
				GroupName = $"Group {n}";

				Add(new _8899Item { Text = $"Group {n} Item" });
			}
		}

		public class _8899Item
		{ 
			public string Text { get; set; }
		}

#if UITEST
		[Test, Category(UITestCategories.CollectionView)]
		public void ClearingGroupedCollectionViewShouldNotCrash()
		{
			RunningApp.WaitForElement(Go);
			RunningApp.Tap(Go);
			RunningApp.WaitForElement(Success);
		}
#endif
	}


}
