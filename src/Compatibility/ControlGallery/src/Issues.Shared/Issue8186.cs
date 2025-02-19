using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;


#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility.UITests;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 8186, "[UWP] Setting IsRefreshing from OnAppearing on RefreshView crashes UWP",
		PlatformAffected.UWP)]
#if UITEST
	[NUnit.Framework.Category(UITestCategories.RefreshView)]
#endif
	public class Issue8186 : TestNavigationPage
	{
		RefreshView _refreshView;
		protected override void Init()
		{
			_refreshView = new RefreshView()
			{
				Content = new ScrollView()
				{
					Content = new StackLayout()
					{
						Children =
						{
							new Label()
							{
								Text = "If you are reading this and see a refresh circle test has succeeded",
								AutomationId = "Success"
							},
							new Button()
							{
								Text = "Push Page then return to this page.",
								Command = new Command(() =>
								{
									Navigation.PushAsync(new ContentPage()
									{
										Content = new Button()
										{
											Text = "Pop Page",
											AutomationId = "PopPage",
											Command = new Command(()=> Navigation.PopAsync())
										}
									});
								}),
								AutomationId = "PushPage"
							}
						}
					}
				}
			};

			var page = new ContentPage() { Content = _refreshView };
			page.Appearing += (_, __) => _refreshView.IsRefreshing = true;
			page.Disappearing += (_, __) => _refreshView.IsRefreshing = false;
			PushAsync(page);
		}

#if UITEST
		[Test]
		public void SetIsRefreshingToTrueInOnAppearingDoesntCrash()
		{
			RunningApp.WaitForElement("Success");
			RunningApp.Tap("PushPage");
			RunningApp.Tap("PopPage");
			RunningApp.WaitForElement("Success");
		}
#endif
	}
}
