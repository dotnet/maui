using System;
using System.Threading.Tasks;
using Android.Content;
using Android.Content.Res;
using Android.Runtime;
using Android.Support.V7.Text;
using Android.Support.V7.View;
using Android.Support.V7.Widget;
using Android.Text.Method;
using Android.Views;
using NUnit.Framework;
using Xamarin.Forms;
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

		[Test, Category("Button")]
		[Description("Account for user's setting of styles property textAllCaps")]
		[Issue(IssueTracker.Github, 11703, "[Bug] Android textAllCaps no longer works")]
		public void Issue11703Test()
		{
			var button = new Button { Text = "foo" };
			var buttonControl = GetNativeControl(button);

			var initialTextTransform = buttonControl.TransformationMethod;
			Assert.IsNotNull(initialTextTransform);
			button.TextTransform = TextTransform.Uppercase;
			Assert.IsNull(buttonControl.TransformationMethod);
			button.TextTransform = TextTransform.Default;
			Assert.AreEqual(initialTextTransform, buttonControl.TransformationMethod);
		}

		[Test, Category("Button")]
		[Description("Test Text Transform property works")]
		[Issue(IssueTracker.Github, 0, "Text Transform Tests")]
		public void TextTransformUpperCase()
		{
			var button = new Button { Text = "foo" };
			var buttonControl = GetNativeControl(button);
			button.TextTransform = TextTransform.Uppercase;
			Assert.AreEqual("FOO", buttonControl.Text);
		}

		[Test, Category("Button")]
		[Description("Test Text Transform property works")]
		[Issue(IssueTracker.Github, 0, "Text Transform Tests")]
		public void TextTransformLowerCase()
		{
			var button = new Button { Text = "FOO" };
			var buttonControl = GetNativeControl(button);
			button.TextTransform = TextTransform.Lowercase;
			Assert.AreEqual("foo", buttonControl.Text);
		}

		[Category("Button")]
		[Description("Account for user's setting of styles property textAllCaps")]
		[Issue(IssueTracker.Github, 11703, "[Bug] Android textAllCaps no longer works", issueTestNumber: 1)]
		[Test]
		public void StyleTextAllCapsSettingIsRespected()
		{
			var button = new ClearTextTransform { Text = "foo" };
			var buttonControl = GetNativeControl(button);
			Assert.IsNull(buttonControl.TransformationMethod);
			button.TextTransform = TextTransform.Uppercase;
			Assert.IsNull(buttonControl.TransformationMethod);
			button.TextTransform = TextTransform.Default;
			Assert.IsNull(buttonControl.TransformationMethod);
		}

		// This is the ideal test for Issue11703. It's currently being tabled due to a Resource linking bug that we're working out with Android team
		//[Category("Button")]
		//[Description("Account for user's setting of styles property textAllCaps")]
		//[Issue(IssueTracker.Github, 11703, "[Bug] Android textAllCaps no longer works", issueTestNumber: 1)]
		//[TestCase(false)]
		//[TestCase(true)]
		//public void StyleTextAllCapsSettingIsRespected(bool allCaps)
		//{
		//	ContextThemeWrapper contextThemeWrapper = null;
		//	if (allCaps)
		//		contextThemeWrapper = new ContextThemeWrapper(Context, Resource.Style.TextAllCapsStyleTrue);
		//	else
		//		contextThemeWrapper = new ContextThemeWrapper(Context, Resource.Style.TextAllCapsStyleFalse);

		//	var button = new Button { Text = "foo" };
		//	var buttonControl = GetRenderer(button, contextThemeWrapper).View as AppCompatButton;
		//	var initialTextTransform = buttonControl.TransformationMethod;

		//	Assert.AreEqual(allCaps, initialTextTransform is AllCapsTransformationMethod);
		//}

	}
}
