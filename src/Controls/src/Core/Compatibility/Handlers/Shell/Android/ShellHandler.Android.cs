//using System;
//using System.Collections.Generic;
//using System.Text;
//using Microsoft.Maui.Controls.Platform;
//using Microsoft.Maui.Graphics;
//using Microsoft.Maui.Handlers;
//using AView = Android.Views.View;
//using Microsoft.Maui.Controls.Platform.Compatibility;

//namespace Microsoft.Maui.Controls.Handlers.Compatibility
//{
//	public partial class ShellHandler : ViewHandler<Shell, ShellFlyoutRenderer>
//	{
//		public static PropertyMapper<Shell, ShellHandler> Mapper =
//				   new PropertyMapper<Shell, ShellHandler>(ElementMapper);

//		public static CommandMapper<Shell, ShellHandler> CommandMapper =
//			new CommandMapper<Shell, ShellHandler>(ElementCommandMapper);

//		public ShellHandler() : base(Mapper, CommandMapper)
//		{
//		}

//		ShellRenderer _ShellRenderer;
//		protected override ShellFlyoutRenderer CreatePlatformView()
//		{
//			var drawerLayout = (_ShellRenderer as IShellContext)?.CurrentDrawerLayout;
//			return (ShellFlyoutRenderer)drawerLayout;
//		}


//		public override void SetVirtualView(IView view)
//		{
//			_ShellRenderer = new ShellRenderer(Context);
//			_ShellRenderer.SetVirtualView((Shell)view);
//			base.SetVirtualView(view);
//		}
//	}
//}
