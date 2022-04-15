using System.Collections;
using ElmSharp;
using Microsoft.Maui.Graphics;
using Tizen.UIExtensions.ElmSharp;
using ITNavigationView = Tizen.UIExtensions.ElmSharp.INavigationView;

namespace Microsoft.Maui.Controls.Platform
{
	public class TVShellView : ShellView
	{
		public TVShellView(EvasObject parent) : base(parent)
		{
		}

		public override void SetElement(Shell shell, IMauiContext context)
		{
			base.SetElement(shell, context);

			// Workaround to set to use a default color for TV different from the mobile
			shell.SetAppThemeColor(Shell.FlyoutBackgroundColorProperty, Colors.Black, Colors.Black);
		}

		protected override INavigationDrawer CreateNavigationDrawer()
		{
			return new TVNavigationDrawer(NativeParent);
		}

		protected override ITNavigationView CreateNavigationView()
		{
			return new TVNavigationView(NativeParent);
		}

		protected override ShellItemView CreateShellItemView(ShellItem item)
		{
			return new TVShellItemView(item, MauiContext);
		}

		protected override ItemAdaptor GetItemAdaptor(IEnumerable items)
		{
			return new TVShellItemAdaptor(Element, NavigationView, MauiContext, items, !Element.FlyoutIsPresented);
		}
	}
}
