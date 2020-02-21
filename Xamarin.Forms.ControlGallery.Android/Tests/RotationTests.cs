using System;
using System.Collections;
using NUnit.Framework;
using NUnit.Framework.Internal;
using Xamarin.Forms.CustomAttributes;
using AView = Android.Views.View;

namespace Xamarin.Forms.ControlGallery.Android.Tests
{
	[TestFixture]
	public class RotationTests : PlatformTestFixture
	{
		static IEnumerable RotationXCases
		{
			get
			{
				foreach (var element in BasicElements)
				{
					element.RotationX = 33.0;
					yield return CreateTestCase(element);
				}
			}
		}

		static IEnumerable RotationYCases
		{
			get
			{
				foreach (var element in BasicElements)
				{
					element.RotationY = 87.0;
					yield return CreateTestCase(element);
				}
			}
		}

		static IEnumerable RotationCases
		{
			get
			{
				foreach (var element in BasicElements)
				{
					element.Rotation = 23.0;
					yield return CreateTestCase(element);
				}
			}
		}

		void AssertRotationConsistent(VisualElement element, Func<VisualElement, double> getRotation,
			Func<AView, float> getNativeRotation)
		{
			using (var renderer = GetRenderer(element))
			{
				var expected = getRotation(element);
				var nativeView = renderer.View;

				ParentView(nativeView);

				var actual = getNativeRotation(renderer.View);

				Assert.That((double)actual, Is.EqualTo(expected).Within(0.0001d));

				UnparentView(nativeView);
			}
		}

		[Test, Category("RotationX"), TestCaseSource(nameof(RotationXCases))]
		[Description("VisualElement X rotation should match renderer X rotation")]
		public void RotationXConsistent(VisualElement element)
		{
			AssertRotationConsistent(element, e => e.RotationX, v => v.RotationX);
		}

		[Test, Category("RotationY"), TestCaseSource(nameof(RotationYCases))]
		[Description("VisualElement Y rotation should match renderer Y rotation")]
		public void RotationYConsistent(VisualElement element)
		{
			AssertRotationConsistent(element, e => e.RotationY, v => v.RotationY);
		}

		[Test, Category("Rotation"), TestCaseSource(nameof(RotationCases))]
		[Description("VisualElement rotation should match renderer rotation")]
		public void RotationConsistent(VisualElement element)
		{
			AssertRotationConsistent(element, e => e.Rotation, v => v.Rotation);
		}
	}
}