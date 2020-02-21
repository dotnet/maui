using System;
using System.Collections;
using NUnit.Framework;
using UIKit;
using static Xamarin.Forms.Core.UITests.NumericExtensions;
using static Xamarin.Forms.Core.UITests.ParsingUtils;

namespace Xamarin.Forms.ControlGallery.iOS.Tests
{
	[TestFixture]
	public class RotationTests : PlatformTestFixture 
	{
		static IEnumerable RotationXCases
		{
			get
			{
				foreach (var element in BasicViews)
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
				foreach (var element in BasicViews)
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
				foreach (var element in BasicViews)
				{
					element.Rotation = 23.0;
					yield return CreateTestCase(element);
				}
			}
		}

		void AssertRotationConsistent(View view, Func<View, Core.UITests.Matrix> getRotation,
			Func<UIView, Core.UITests.Matrix> getNativeRotation)
		{
			var page = new ContentPage() { Content = view };

			using (var pageRenderer = GetRenderer(page))
			{
				using (var uiView = GetRenderer(view).NativeView)
				{
					page.Layout(new Rectangle(0, 0, 200, 200));

					var expected = getRotation(view);
					var actual = getNativeRotation(uiView);

					Assert.That(actual, Is.EqualTo(expected));
				}
			}
		}

		[Test, Category("RotationX"), TestCaseSource(nameof(RotationXCases))]
		[Description("VisualElement X rotation should match renderer X rotation")]
		public void RotationXConsistent(View view)
		{
			AssertRotationConsistent(view, 
			e => CalculateRotationMatrixForDegrees((float)e.RotationX, Core.UITests.Axis.X), 
			v => ParseCATransform3D(v.Layer.Transform.ToString()));
		}

		[Test, Category("RotationY"), TestCaseSource(nameof(RotationYCases))]
		[Description("VisualElement Y rotation should match renderer Y rotation")]
		public void RotationYConsistent(View view)
		{
			AssertRotationConsistent(view,
			e => CalculateRotationMatrixForDegrees((float)e.RotationY, Core.UITests.Axis.Y),
			v => ParseCATransform3D(v.Layer.Transform.ToString()));
		}

		[Test, Category("Rotation"), TestCaseSource(nameof(RotationCases))]
		[Description("VisualElement rotation should match renderer rotation")]
		public void RotationConsistent(View view)
		{
			AssertRotationConsistent(view,
			e => CalculateRotationMatrixForDegrees((float)e.Rotation, Core.UITests.Axis.Z),
			v => ParseCATransform3D(v.Layer.Transform.ToString()));
		}
	}
}