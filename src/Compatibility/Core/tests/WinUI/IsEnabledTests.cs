using System.Collections;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UAP.UnitTests
{
	[TestFixture]
	public class IsEnabledTests : PlatformTestFixture
	{
		static IEnumerable TestCases
		{
			get
			{
				// Generate IsEnabled = true cases
				foreach (var element in BasicViews)
				{
					element.IsEnabled = true;
					yield return CreateTestCase(element)
						.SetName($"{element.GetType().Name}_IsEnabled_{element.IsEnabled}");
				}

				// Generate IsEnabled = false cases
				foreach (var element in BasicViews)
				{
					element.IsEnabled = false;
					yield return CreateTestCase(element)
						.SetName($"{element.GetType().Name}_IsEnabled_{element.IsEnabled}");
				}
			}
		}

		[Test, Category("IsEnabled"), TestCaseSource(nameof(TestCases))]
		[Description("View enabled should match renderer enabled")]
		public async Task EnabledConsistent(VisualElement element)
		{
			await Device.InvokeOnMainThreadAsync(() =>
			{
				using (var renderer = GetRenderer(element))
				{
					var expected = element.IsEnabled;
					var container = renderer.ContainerElement;

					// Check the container control
					Assert.That(container.IsHitTestVisible, Is.EqualTo(expected));

					// Check the actual control (if there is one; for some renderers, like Frame and BoxView, the 
					// native control isn't a Microsoft.UI.Xaml.Controls.Control)
					var control = GetNativeControl(element);

					if (control != null)
					{
						Assert.That(control.IsEnabled, Is.EqualTo(expected));
					}
				}
			});
		}
	}
}
