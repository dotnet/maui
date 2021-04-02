using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

#if UITEST
using Microsoft.Maui.Controls.Compatibility.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
#if UITEST
	[Category(UITestCategories.ListView)]
	[Category(UITestCategories.ManualReview)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 5503, "[iOS] UITableView.Appearance.BackgroundColor ignored or overridden for ListView",
		PlatformAffected.iOS)]
	public class Issue5503 : TestContentPage
	{
		const string ChangeBackgroundButtonAutomationId = "ChangeBackgroundButton";
		const string ListViewAutomationId = "TheListView";

		public static string ChangeUITableViewAppearanceBgColor = "BIBBIDYBOBBIDIBOOOO";

		ObservableCollection<string> _items = new ObservableCollection<string>();
		public ObservableCollection<string> Items
		{
			get => _items;
			set
			{
				_items = value;
				OnPropertyChanged();
			}
		}

		protected override void Init()
		{
			BindingContext = this;

			var listView = new ListView
			{
				AutomationId = ListViewAutomationId
			};

			listView.SetBinding(ItemsView<Cell>.ItemsSourceProperty, new Binding(nameof(Items)));

			for (int i = 0; i < 100; i++)
			{
				Items.Add($"Item {i}");
			}

			Padding = new Thickness(10);
			BackgroundColor = Colors.LightGray;

			var changeAppearanceButton = new Button()
			{
				Text = "Change Background through Appearance API",
				AutomationId = ChangeBackgroundButtonAutomationId
			};

			changeAppearanceButton.Clicked += (s, a) =>
			{
				MessagingCenter.Send(this, ChangeUITableViewAppearanceBgColor);
			};

			var stack = new StackLayout();

			stack.Children.Add(changeAppearanceButton);
			stack.Children.Add(listView);

			Content = stack;
		}

#if UITEST && __IOS__
		[Test]
		public void ToggleAppearanceApiBackgroundColorListView()
		{
			RunningApp.WaitForElement(ChangeBackgroundButtonAutomationId);

			RunningApp.Screenshot("ListView cells have clear background, default color from code");

			RunningApp.Tap(ChangeBackgroundButtonAutomationId);
			RunningApp.NavigateBack();
			RunningApp.WaitForNoElement(ChangeBackgroundButtonAutomationId);
			AppSetup.NavigateToIssue(typeof(Issue5503), RunningApp);
			RunningApp.WaitForElement(ChangeBackgroundButtonAutomationId);

			RunningApp.Screenshot("ListView cells have Red background, set by Appearance API");

		}
#endif
	}
}
