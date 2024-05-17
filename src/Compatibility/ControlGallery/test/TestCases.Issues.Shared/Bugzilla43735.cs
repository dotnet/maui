using System.Collections.Generic;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Bugzilla)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 43735, "Multiple Swipe on ContextActions", PlatformAffected.iOS)]
	public class Bugzilla43735 : TestContentPage // or TestFlyoutPage, etc ...
	{
		protected override void Init()
		{
			var stackLayout = new StackLayout();

			var l = new Label
			{
				Text = "Swipe multiple cells at the same time. Only one cell should show its context actions."
			};
			stackLayout.Children.Add(l);

			var list = new List<int>();
			for (var i = 0; i < 20; i++)
				list.Add(i);

			var listView = new ListView
			{
				ItemsSource = list,
				ItemTemplate = new DataTemplate(() =>
				{
					var label = new Label();
					label.SetBinding(Label.TextProperty, new Binding("."));

					return new ViewCell
					{
						View = new ContentView
						{
							Content = label,
						},
						ContextActions = { new MenuItem
						{
							Text = "Action"
						},
						new MenuItem
						{
							Text = "Delete",
							IsDestructive = true
						} }
					};
				})
			};
			stackLayout.Children.Add(listView);

			Content = stackLayout;
		}
	}
}