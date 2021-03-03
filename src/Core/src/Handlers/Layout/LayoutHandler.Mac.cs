using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Maui.Handlers
{
	public partial class LayoutHandler : AbstractViewHandler<ILayout, NSView>
	{
		protected override NSView CreateView()
		{
			return new NSView();
		}
	}
}
