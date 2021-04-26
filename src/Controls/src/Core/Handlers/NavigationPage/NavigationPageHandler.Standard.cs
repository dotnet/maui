using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls.Handlers
{
	public partial class NavigationPageHandler :
		ViewHandler<NavigationPage, object>
	{
		public NavigationPageHandler() : base(ViewHandler.ViewMapper)
		{

		}

		protected override object CreateNativeView()
		{
			throw new NotImplementedException();
		}
	}
}
