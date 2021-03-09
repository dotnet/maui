using System.Collections;
using NUnit.Framework;
using System.Threading.Tasks;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Android.UnitTests
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

		[Test, Category("TranslateX"), TestCaseSource(nameof(TranslationXCases))]
		[Description("View X translation should match renderer X translation")]
		public async Task TranslationXConsistent(View view)
		{
			var expected = Context.ToPixels(view.TranslationX);
			var actual = await GetRendererProperty(view, ver => ver.View.TranslationX, requiresParent: true);
			Assert.That((double)actual, Is.EqualTo(expected).Within(0.01d));
		}

		[Test, Category("TranslateY"), TestCaseSource(nameof(TranslationYCases))]
		[Description("View Y translation should match renderer Y translation")]
		public async Task TranslationYConsistent(View view)
		{
			var expected = Context.ToPixels(view.TranslationY);
			var actual = await GetRendererProperty(view, ver => ver.View.TranslationY, requiresParent: true);
			Assert.That((double)actual, Is.EqualTo(expected).Within(0.01d));
		}
	}
}