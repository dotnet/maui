using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls.Handlers
{
	public partial class ShellHandler : ViewHandler<Shell, ShellFlyoutView>
	{
		public new class Factory : ViewHandler.Factory
		{
			public virtual ShellFlyoutView CreateNativeView(ShellHandler ShellHandler, Shell Shell)
			{
				var drawerLayout = (ShellHandler._shellView as IShellContext)?.CurrentDrawerLayout;
				return (ShellFlyoutView)drawerLayout;
			}

			public virtual IShellObservableFragment CreateFragmentForPage(IShellContext shellContext, Page page)
			{
				return new ShellContentFragment(shellContext, page);
			}
		}

		public static Factory ShellFactory { get; set; } = new Factory();

		public static ShellFactoryMapper FactoryMapper = new()
		{
			[nameof(Factory.CreateNativeView)] = (h, v, _) => ShellFactory.CreateNativeView(h, v),
			[nameof(Factory.CreateFragmentForPage)] = (h, v, args) => ShellFactory.CreateFragmentForPage((IShellContext)v, (Page)args)
		};

		public class ShellFactoryMapper : FactoryMapper<Shell, ShellHandler>
		{
			// Provide simplified typed helpers for users to replace behavior
			public static void SetCreateFragmentForPage(Func<IShellContext, Page, IShellObservableFragment> func)
			{
				FactoryMapper[nameof(Factory.CreateFragmentForPage)] =
					(h, v, args) => func.Invoke((IShellContext)v, (Page)args);
			}
		}

		// Alternative Approach just add Func directly on Handler
		public static Func<IShellContext, Page, IShellObservableFragment> CreateFragmentForPage { get; set; } =
			(sc, page) => ShellFactory.CreateFragmentForPage(sc, page);


		ShellView _shellView;
		protected override ShellFlyoutView CreateNativeView() =>
			(ShellFlyoutView)FactoryMapper[nameof(ShellHandler.CreateNativeView)].Invoke(this, VirtualView, null);


		public override void SetVirtualView(IView view)
		{
			_shellView = new ShellView(Context);
			_shellView.SetVirtualView((Shell)view);
			base.SetVirtualView(view);
		}
	}
}
