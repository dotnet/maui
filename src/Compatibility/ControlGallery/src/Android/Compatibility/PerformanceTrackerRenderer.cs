//using System;
//using System.ComponentModel;
//using Android.Views;
//using Microsoft.Maui.Controls;
//using Microsoft.Maui.Controls.Compatibility;
//using Microsoft.Maui.Controls.ControlGallery;
//using Microsoft.Maui.Controls.ControlGallery.Android;
//using Microsoft.Maui.Controls.Compatibility.Platform.Android;
//using static Android.Views.ViewTreeObserver;
//using AView = Android.Views.View;

//[assembly: ExportRenderer(typeof(PerformanceTracker), typeof(PerformanceTrackerRenderer))]

//namespace Microsoft.Maui.Controls.ControlGallery.Android
//{
//	public class PerformanceTrackerRenderer : ViewRenderer, IOnDrawListener
//	{
//		public PerformanceTrackerRenderer(global::Android.Content.Context context) : base(context)
//		{
//			ViewTreeObserver.AddOnDrawListener(this);
//		}

//		PerformanceTracker PerformanceTracker => Element as PerformanceTracker;

//		void IOnDrawListener.OnDraw()
//		{
//			PerformanceTracker.Watcher.BeginTest(cleanup: () => UnsubscribeChildrenToDraw(this, this));
//		}

//		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
//		{
//			base.OnElementPropertyChanged(sender, e);

//			if (e.PropertyName == PerformanceTracker.ContentProperty.PropertyName)
//			{
//				PerformanceTracker.Watcher.ResetTest();

//				SubscribeChildrenToDraw(this, this);
//			}
//		}

//		static void SubscribeChildrenToDraw(AView view, IOnDrawListener observer)
//		{
//			if (view == null)
//				return;

//			view.ViewTreeObserver.AddOnDrawListener(observer);

//			var viewGroup = view as ViewGroup;

//			if (viewGroup == null)
//				return;

//			for (int i = 0; i < viewGroup.ChildCount; i++)
//			{
//				SubscribeChildrenToDraw(viewGroup.GetChildAt(i), observer);
//			}
//		}

//		static void UnsubscribeChildrenToDraw(AView view, IOnDrawListener observer)
//		{
//			if (view == null)
//				return;

//			view.ViewTreeObserver.RemoveOnDrawListener(observer);

//			var viewGroup = view as ViewGroup;

//			if (viewGroup == null)
//				return;

//			for (int i = 0; i < viewGroup.ChildCount; i++)
//			{
//				UnsubscribeChildrenToDraw(viewGroup.GetChildAt(i), observer);
//			}
//		}
//	}
//}