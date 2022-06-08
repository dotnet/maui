using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Handlers
{
    public partial class MapHandler : ViewHandler<IMap, WebView2>
	{

		protected override WebView2 CreatePlatformView() => throw new NotImplementedException();
	}
}
