using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Maui.CustomAttributes;
using System.Maui.Internals;

#if UITEST
using System.Maui.Core.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace System.Maui.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.CollectionView)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.None, 0, "CollectionView ItemsSource Types", PlatformAffected.All)]
	public class CollectionViewItemsSourceTypes : TestContentPage
	{
		protected override void Init()
		{
			Content = new StackLayout()
			{
				Children =
				{
					new Label()
					{
						Text = "If you see three 900s this test has passed"
					},
					new CollectionView()
					{
						ItemsSource = new[] { 900 },
						HeightRequest = 50
					},
					new CollectionView()
					{
						ItemsSource = new[] { "900" }.ToList<object>(),
						HeightRequest = 50
					},
					new CollectionView()
					{
						ItemsSource = new ObservableCollection<string>(new[] { "900" }),
						HeightRequest = 50
					}
				}
			};
		}

#if UITEST
		[Test]
		public void CollectionViewItemsSourceTypesDisplayAndDontCrash()
		{
			RunningApp.QueryUntilPresent(() =>
			{
				var result = RunningApp.WaitForElement("900");

				if (result.Length == 3)
					return result;

				return null;
			});
		}
#endif
	}
}
