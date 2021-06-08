using System;
using Microsoft.Maui.Handlers;
using Tizen.UIExtensions.ElmSharp;

namespace Microsoft.Maui.Controls.Handlers
{
	public partial class ShellHandler : ViewHandler<Shell, NavigationDrawer>
	{
		protected override NavigationDrawer CreateNativeView() => new NavigationDrawer(NativeParent);
	}
}
