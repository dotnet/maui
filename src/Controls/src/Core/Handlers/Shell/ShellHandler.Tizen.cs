using System;
using Microsoft.Maui.Handlers;
using Tizen.UIExtensions.ElmSharp;

namespace Microsoft.Maui.Controls.Handlers
{
	public partial class ShellHandler : EViewHandler<Shell, NavigationDrawer>
	{
		protected override NavigationDrawer CreateNativeView() => new NavigationDrawer(NativeParent);
	}
}
