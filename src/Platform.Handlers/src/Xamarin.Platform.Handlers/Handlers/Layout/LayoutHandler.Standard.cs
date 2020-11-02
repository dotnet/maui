using System;
using System.Collections.Generic;
using System.Text;

namespace Xamarin.Platform.Handlers
{
	public partial class LayoutHandler : AbstractViewHandler<ILayout, object>
	{
		protected override object CreateNativeView() => throw new NotImplementedException();
	}
}
