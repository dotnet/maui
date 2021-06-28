//using System;
//using System.Collections.Generic;
//using System.Text;
//using Android.Content;
//using Android.Widget;
//using Microsoft.Maui.Controls.Platform;
//using Microsoft.Maui.Handlers;

//namespace Microsoft.Maui.Controls.Handlers
//{
//	public class LabelHandler
//		: Microsoft.Maui.Handlers.LabelHandler
//	{
//		GestureManager gestureManager;
//		VisualElement _previousView;
//		public override void SetVirtualView(IView view)
//		{
//			base.SetVirtualView(view);
//			gestureManager?.OnElementChanged(new VisualElementChangedEventArgs(_previousView, (VisualElement)view));
//			_previousView = (VisualElement)view;
//		}

//		protected override void ConnectHandler(TextView nativeView)
//		{
//			base.ConnectHandler(nativeView);
//			gestureManager = new GestureManager(this);
//		}

//		protected override void DisconnectHandler(TextView nativeView)
//		{
//			base.DisconnectHandler(nativeView);
//			gestureManager?.Dispose();
//			gestureManager = null;
//		}
//	}
//}
