using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Xamarin.Forms.CustomAttributes;
using AColor = Android.Graphics.Color;

namespace Xamarin.Forms.Platform.Android.UnitTests
{

	[TestFixture]
	public class ButtonTests : PlatformTestFixture
	{
		[Test, Category("Button")]
		[Description("Default Button Disabled color on Android doesn't look disabled after retrieving default VSM")]
		[Issue(IssueTracker.Github, 10040, "[Bug] Button IsEnabled color Android")]
		public async Task ButtonDisabledColorWorks()
		{
			Button myButton = new Button()
			{
				BackgroundColor = Color.Green,
				TextColor = Color.White,
				BindingContext = new object(),
				Text = "test text",
				IsEnabled = false
			};

			var vsm = myButton.GetValue(VisualStateManager.VisualStateGroupsProperty);
			var textColors = await GetControlProperty(myButton, (b) => b.TextColors);
			var disabledColor = textColors.GetColorForState(new[] { -global::Android.Resource.Attribute.StateEnabled }, AColor.Green);

			int compareTo = Color.White.ToAndroid();
			Assert.AreNotEqual(compareTo, disabledColor);
		}
	}
}
