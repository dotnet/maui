using System;
using System.Collections;
using NUnit.Framework;
using AView = Android.Views.View;
using Xamarin.Forms.Platform.Android;

namespace Xamarin.Forms.ControlGallery.Android.Tests
{
	[TestFixture]
	public class TranslationTests : PlatformTestFixture
	{
		static IEnumerable TranslationXCases
		{
			get
			{
				foreach (var element in BasicElements)
				{
					element.TranslationX = -100;
					yield return CreateTestCase(element);
				}
			}
		}

		static IEnumerable TranslationYCases
		{
			get
			{
				foreach (var element in BasicElements)
				{
					element.TranslationY = -40;
					yield return CreateTestCase(element);
				}
			}
		}

		void AssertTranslationConsistent(View view, Func<View, double> getTranslation,
			Func<AView, double> getNativeTranslation)
		{
			using (var renderer = GetRenderer(view))
			{
				var expected =  Context.ToPixels(getTranslation(view));
				var nativeView = renderer.View;

				ParentView(nativeView);

				Assert.That(getNativeTranslation(nativeView), Is.EqualTo(expected).Within(0.01));

				UnparentView(nativeView);
			}
		}

		[Test, Category("TranslateX"), TestCaseSource(nameof(TranslationXCases))]
		[Description("View X translation should match renderer X translation")]
		public void TranslationXConsistent(View view)
		{
			AssertTranslationConsistent(view, e => e.TranslationX, v => v.TranslationX);
		}

		[Test, Category("ScaleY"), TestCaseSource(nameof(TranslationYCases))]
		[Description("View Y translation should match renderer Y translation")]
		public void TranslationYConsistent(View view)
		{
			AssertTranslationConsistent(view, e => e.TranslationY, v => v.TranslationY);
		}
	}
}