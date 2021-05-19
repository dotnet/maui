using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	public partial class Page : IPage
	{
		IView IPage.Content => null;
	}
}
