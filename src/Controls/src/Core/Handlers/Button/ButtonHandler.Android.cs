//using System;
//using System.Collections.Generic;
//using System.Text;
//using AndroidX.AppCompat.Widget;
//using Microsoft.Maui.Controls.Platform;

//namespace Microsoft.Maui.Controls.Handlers
//{
//	public class ButtonHandler 
//		: Microsoft.Maui.Handlers.ButtonHandler
//	{
//		public new ControlsButton NativeView => (ControlsButton)base.NativeView;

//		protected override AppCompatButton CreateNativeView()
//		{
//			var nativeButton = new ControlsButton(Context)
//			{
//				SoundEffectsEnabled = false
//			};

//			return nativeButton;
//		}

//		protected override void ConnectHandler(AppCompatButton nativeView)
//		{
//			base.ConnectHandler(nativeView);
//		}

//		protected override void DisconnectHandler(AppCompatButton nativeView)
//		{
//			base.DisconnectHandler(nativeView);
//		}
//	}
//}
