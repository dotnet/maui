using System;
using System.Collections;
using NUnit.Framework;
using UIKit;
using static Xamarin.Forms.Core.UITests.NumericExtensions;
using static Xamarin.Forms.Core.UITests.ParsingUtils;

namespace Xamarin.Forms.ControlGallery.iOS.Tests
{
	[TestFixture]
	public class ScaleTests : PlatformTestFixture
	{
		static IEnumerable ScaleCases
		{
			get
			{
				foreach (var element in BasicViews)
				{
					element.Scale = 2.0;
					yield return CreateTestCase(element);
				}
			}
		}

		void AssertScaleConsistent(View view, Func<View, Core.UITests.Matrix> getScale,
			Func<UIView, Core.UITests.Matrix> getNativeScale)
		{
			var page = new ContentPage() { Content = view };

			using (var pageRenderer = GetRenderer(page))
			{
				using (var uiView = GetRenderer(view).NativeView)
				{
					page.Layout(new Rectangle(0, 0, 200, 200));

					var expected = getScale(view);
					var actual = getNativeScale(uiView);

					Assert.That(actual, Is.EqualTo(expected));
				}
			}
		}

		[Test, Category("Scale"), TestCaseSource(nameof(ScaleCases))]
		[Description("View scale should match renderer scale")]
		public void ScaleConsistent(View view)
		{
			AssertScaleConsistent(view,
			e => BuildScaleMatrix((float)e.Scale),
			v => ParseCATransform3D(v.Layer.Transform.ToString()));
		}
	}
}