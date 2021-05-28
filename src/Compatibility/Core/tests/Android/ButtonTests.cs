using System;
using System.Threading.Tasks;
using Android.Content;
using Android.Content.Res;
using Android.Runtime;
using Android.Text.Method;
using Android.Views;
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Graphics;
using AColor = Android.Graphics.Color;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Android.UnitTests
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
				BackgroundColor = Colors.Green,
				TextColor = Colors.White,
				BindingContext = new object(),
				Text = "test text",
				IsEnabled = false
			};

			var vsm = myButton.GetValue(VisualStateManager.VisualStateGroupsProperty);
			var textColors = await GetControlProperty(myButton, (b) => b.TextColors);
			var disabledColor = textColors.GetColorForState(new[] { -global::Android.Resource.Attribute.StateEnabled }, AColor.Green);

			int compareTo = Colors.White.ToAndroid();
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
		/*[Category("Button")]
		[Description("Account for user's setting of styles property textAllCaps")]
		[Issue(IssueTracker.Github, 11703, "[Bug] Android textAllCaps no longer works", issueTestNumber: 1)]
		[TestCase(false)]
		[TestCase(true)]
		public async Task StyleTextAllCapsSettingIsRespected(bool allCaps)
		{
			AContextThemeWrapper contextThemeWrapper = null;
			if (allCaps)
				contextThemeWrapper = new AContextThemeWrapper(Context, Context.GetStyle("TextAllCapsStyleTrue"));
			else
				contextThemeWrapper = new AContextThemeWrapper(Context, Context.GetStyle("TextAllCapsStyleFalse"));

			var button = new Button { Text = "foo" };
			var initialTextTransform = await GetControlProperty(button, x => x.TransformationMethod);

			// when set through a style the type is an internal version of AllCapsTransformationMethod
			string typeName = $"{initialTextTransform}";
			Assert.AreEqual(allCaps, typeName.Contains("AllCapsTransformationMethod"));
		}*/

	}
}
