using Microsoft.Maui.Layouts;

namespace Microsoft.Maui.Controls
{
	public interface ILayoutManagerFactory
	{
		ILayoutManager CreateLayoutManager(Layout layout);
	}
}
