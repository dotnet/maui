//using System;
//using System.Collections.Generic;
//using System.Text;
//using Android.Content;
//using Android.Views;
//using Microsoft.Maui.Controls.Platform;
//using Microsoft.Maui.Handlers;

//namespace Microsoft.Maui.Controls.Handlers
//{
//	public class LayoutHandler : Microsoft.Maui.Handlers.LayoutHandler
//	{
//		GestureManager gestureManager;
//		VisualElement _previousView;

//		protected override Maui.Handlers.LayoutViewGroup CreateNativeView()
//		{
//			var thing =  new ControlsLayoutViewGroup(Context!, this);

//			thing.Touch += Thing_Touch;
//			thing.SetOnTouchListener(new test());
//			thing.Touch += Thing_Touch1;
//			return thing;
//		}

//		private void Thing_Touch1(object sender, Android.Views.View.TouchEventArgs e)
//		{
//		}

//		class test : Java.Lang.Object, Android.Views.View.IOnTouchListener
//		{
//			public bool OnTouch(Android.Views.View v, MotionEvent e)
//			{
//				return false;
//			}
//		}

//		private void Thing_Touch(object sender, Android.Views.View.TouchEventArgs e)
//		{
//		}

//		public override void SetVirtualView(IView view)
//		{
//			base.SetVirtualView(view);
//			gestureManager?.OnElementChanged(new VisualElementChangedEventArgs(_previousView, (VisualElement)view));
//			_previousView = (VisualElement)view;
//		}

//		protected override void ConnectHandler(Maui.Handlers.LayoutViewGroup nativeView)
//		{
//			base.ConnectHandler(nativeView);
//			gestureManager = new GestureManager(this);
//		}

//		protected override void DisconnectHandler(LayoutViewGroup nativeView)
//		{
//			base.DisconnectHandler(nativeView);
//			gestureManager?.Dispose();
//			gestureManager = null;
//		}

//		public class ControlsLayoutViewGroup : Maui.Handlers.LayoutViewGroup
//		{
//			readonly LayoutHandler _layoutHandler;

//			public ControlsLayoutViewGroup(Context context, LayoutHandler layoutHandler) : base(context)
//			{
//				_layoutHandler = layoutHandler;
//			}

//			public override bool OnTouchEvent(MotionEvent e)
//			{
//				var result = _layoutHandler.gestureManager?.OnTouchEvent(e) ?? false;
//				return result || base.OnTouchEvent(e);
//			}
//		}
//	}
//}
