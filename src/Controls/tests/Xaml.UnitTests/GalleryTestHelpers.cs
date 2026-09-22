using System.Linq;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

static class GalleryTestHelpers
{
	public static T Find<T>(Element page, string automationId) where T : Element =>
		page.Descendants().OfType<T>().Single(element => element.AutomationId == automationId);

	public static void Click(Element page, string automationId) =>
		Find<Button>(page, automationId).SendClicked();
}
