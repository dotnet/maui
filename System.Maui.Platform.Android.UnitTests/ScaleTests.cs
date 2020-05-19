using System;
using System.Collections;
using System.Threading.Tasks;
using NUnit.Framework;
using AView = Android.Views.View;

namespace System.Maui.Platform.Android.UnitTests
{
	[TestFixture]
	public class ScaleTests : PlatformTestFixture
	{
		static IEnumerable ScaleXCases
		{
			get
			{
				foreach (var element in BasicElements)
				{
					element.ScaleX = 0.45;
					yield return CreateTestCase(element);
				}
			}
		}

		static IEnumerable ScaleYCases
		{
			get
			{
				foreach (var element in BasicElements)
				{
					element.ScaleY = 1.23;
					yield return CreateTestCase(element);
				}
			}
		}

		void AssertScaleConsistent(View view, Func<View, double> getScale,
			Func<AView, double> getNativeScale)
		{
			using (var renderer = GetRenderer(view))
			{
				var expected = getScale(view);
				var nativeView = renderer.View;

				ParentView(nativeView);

				Assert.That(getNativeScale(nativeView), Is.EqualTo(expected).Within(0.01));

				UnparentView(nativeView);
			}
		}

		[Test, Category("ScaleX"), TestCaseSource(nameof(ScaleXCases))]
		[Description("View X scale should match renderer X scale")]
		public async Task ScaleXConsistent(View view)
		{
			var expected = view.ScaleX;
			var actual = await GetRendererProperty(view, ver => ver.View.ScaleX, requiresParent: true);
			Assert.That((double)actual, Is.EqualTo(expected).Within(0.01d));
		}

		[Test, Category("ScaleY"), TestCaseSource(nameof(ScaleYCases))]
		[Description("View Y scale should match renderer Y scale")]
		public async Task ScaleYConsistent(View view)
		{
			var expected = view.ScaleY;
			var actual = await GetRendererProperty(view, ver => ver.View.ScaleY, requiresParent: true);
			Assert.That((double)actual, Is.EqualTo(expected).Within(0.01d));
		}
	}
}