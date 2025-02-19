using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using Microsoft.Maui.Controls.Compatibility.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[Category(UITestCategories.Shell)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 5132, "Unable to specify automation properties on the hamburger/flyout icon", PlatformAffected.Default)]
	public class Issue5132 : TestShell
	{
		string _idIconElement = "shellIcon";
		string _titleElement = "Connect";
		protected override void Init()
		{
			Title = "Shell";
			FlyoutIcon = new FontImageSource
			{
				Glyph = "\uf2fb",
				FontFamily = DefaultFontFamily(),
				Size = 20,
				AutomationId = _idIconElement
			};
			FlyoutIcon.SetValue(AutomationProperties.HelpTextProperty, "This as Shell FlyoutIcon");
			FlyoutIcon.SetValue(AutomationProperties.NameProperty, "Shell Icon");
			Items.Add(new FlyoutItem
			{
				Title = _titleElement,
				Items = {
					new Tab { Title = "library",
						Items = {
									new ContentPage { Title = "Library",  Content = new ScrollView { Content = new Label  { Text = "Turn accessibility on and make sure the help text is read on iOS, on Android it will read the AutomationID if specified and then the HelpText this allows UITest to work " } } }
								}
						}
				}
			});
		}

		static string DefaultFontFamily()
		{
			var fontFamily = "";
			switch (Device.RuntimePlatform)
			{
				case Device.iOS:
					fontFamily = "Ionicons";
					break;
				case Device.WinUI:
					fontFamily = "Assets/Fonts/ionicons.ttf#ionicons";
					break;
				case Device.Android:
				default:
					fontFamily = "fonts/ionicons.ttf#";
					break;
			}

			return fontFamily;
		}

#if UITEST
#if !(__ANDROID__ || __IOS__)
		[Ignore("Shell test is only supported on Android and iOS")]
#endif
		[Test]
		public void ShellFlyoutAndHamburgerAutomationProperties()
		{
			RunningApp.WaitForElement(q => q.Marked(_idIconElement));
			TapInFlyout(_titleElement, _idIconElement);
		}
#endif
	}
}
