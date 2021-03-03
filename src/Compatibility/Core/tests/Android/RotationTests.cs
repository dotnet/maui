using System.Collections;
using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Android.UnitTests
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

		[Test, Category("RotationX"), TestCaseSource(nameof(RotationXCases))]
		[Description("VisualElement X rotation should match renderer X rotation")]
		public async Task RotationXConsistent(VisualElement element)
		{
			var expected = element.RotationX;
			var actual = await GetRendererProperty(element, ver => ver.View.RotationX, requiresParent: true);
			Assert.That((double)actual, Is.EqualTo(expected).Within(0.0001d));
		}

		[Test, Category("RotationY"), TestCaseSource(nameof(RotationYCases))]
		[Description("VisualElement Y rotation should match renderer Y rotation")]
		public async Task RotationYConsistent(VisualElement element)
		{
			var expected = element.RotationY;
			var actual = await GetRendererProperty(element, ver => ver.View.RotationY, requiresParent: true);
			Assert.That((double)actual, Is.EqualTo(expected).Within(0.0001d));
		}

		[Test, Category("Rotation"), TestCaseSource(nameof(RotationCases))]
		[Description("VisualElement rotation should match renderer rotation")]
		public async Task RotationConsistent(VisualElement element)
		{
			var expected = element.Rotation;
			var actual = await GetRendererProperty(element, ver => ver.View.Rotation, requiresParent: true);
			Assert.That((double)actual, Is.EqualTo(expected).Within(0.0001d));
		}
	}
}