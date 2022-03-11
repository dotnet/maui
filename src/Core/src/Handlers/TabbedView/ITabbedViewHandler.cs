using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Maui.Handlers
{
	public partial interface ITabbedViewHandler : IViewHandler
	{
		new ITabbedView VirtualView { get; }
	}
}
