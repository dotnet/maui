#nullable disable
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	public partial class RefreshView : IRefreshView
	{
		Paint IRefreshView.RefreshColor => RefreshColor?.AsPaint();

		IView IRefreshView.Content => base.Content;
	}
}