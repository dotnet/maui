using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;


#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility.UITests;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.None, 0, "Shell Modal Behavior Test",
		PlatformAffected.All)]
#if UITEST
	[NUnit.Framework.Category(UITestCategories.Shell)]
#endif
	public class ShellModal : TestShell
	{
		protected override void Init()
		{
			Routing.RegisterRoute(nameof(ModalTestPage), typeof(ModalTestPage));
			Routing.RegisterRoute(nameof(ModalNavigationTestPage), typeof(ModalNavigationTestPage));

			AddContentPage(new ContentPage()
			{
				Content = new StackLayout()
				{
					Children =
					{
						new Button()
						{
							Text = "Push Modal",
							Command = new Command(async () =>
							{
								await GoToAsync(nameof(ModalNavigationTestPage));
							})
						},
						new Button()
						{
							Text = "Push Two Modals",
							Command = new Command(async () =>
							{
								await GoToAsync($"{nameof(ModalNavigationTestPage)}/{nameof(ModalNavigationTestPage)}");
							})
						},
						new Button()
						{
							Text = "Push Modal And Then A Page Onto That Modal Navigation Stack",
							Command = new Command(async () =>
							{
								await GoToAsync($"{nameof(ModalNavigationTestPage)}/{nameof(ModalTestPage)}");
							})
						},
						new Button()
						{
							Text = "Goto Different Tab And Push Modal",
							Command = new Command(async () =>
							{
								await GoToAsync("///OtherTab/ModalNavigationTestPage");
							})
						}
					}
				}
			}, title: "MainContent");

			AddTopTab("OtherTab");
		}

		[Preserve(AllMembers = true)]
		[QueryProperty("IsModal", "IsModal")]
		public class ModalNavigationTestPage : NavigationPage
		{
			public ModalNavigationTestPage() : base(new ModalTestPage())
			{
				IsModal = "true";
			}
			public string IsModal
			{
				set
				{
					if (Convert.ToBoolean(value))
						Shell.SetPresentationMode(this, PresentationMode.Modal);
					else
						Shell.SetPresentationMode(this, PresentationMode.Animated);
				}
			}

			protected override void OnAppearing()
			{
				base.OnAppearing();
			}

			protected override void OnDisappearing()
			{
				base.OnDisappearing();
			}
		}

		[Preserve(AllMembers = true)]
		public class ModalTestPage : ContentPage
		{
			public ModalTestPage()
			{
				Content = new StackLayout()
				{
					Children =
					{
						new Label()
						{
							Text = "Hello I am a modal page"
						},
						new Button()
						{
							Text = "Clicking me should go back to the MainContent Page",
							Command = new Command(async () =>
							{
								await Shell.Current.GoToAsync("//MainContent");
							})
						},
						new Button()
						{
							Text = "Push Another Modal Page",
							Command = new Command(async () =>
							{
								await Shell.Current.GoToAsync($"{nameof(ModalNavigationTestPage)}?IsModal=true");
							})
						},
						new Button()
						{
							Text = "Push Content Page Onto Visible Modal Navigation Stack",
							Command = new Command(async () =>
							{
								await Shell.Current.GoToAsync($"{nameof(ModalTestPage)}?IsModal=false");
							})
						}
					}
				};
			}
		}
	}
}
