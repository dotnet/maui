using System.Collections.Generic;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	public partial class Page : IPage
	{
		IView IPage.Content => null;

		// TODO ezhart super sus
		public Thickness Margin => Thickness.Zero;

		public IList<IGestureRecognizer> GestureRecognizers { get; set; }

		public IList<IGestureRecognizer> CompositeGestureRecognizers { get; set; }

		public IList<IGestureView> GetChildElements(Point point) => null;
	}
}