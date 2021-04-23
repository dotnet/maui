using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Maui.Controls
{
	public partial class Page : IPage
	{
		IView IPage.Content => throw new NotImplementedException();

		Thickness IView.Margin => Thickness.Zero;
	}
}
