using AView = Android.Views.View;
using System;
using System.ComponentModel;
using Xamarin.Forms;
using Xamarin.Forms.ControlGallery.Android;
using Xamarin.Forms.Controls;
using Xamarin.Forms.Platform.Android;
using static Android.Views.ViewTreeObserver;
using Android.Views;

[assembly: ExportRenderer(typeof(PerformanceTracker), typeof(PerformanceTrackerRenderer))]

namespace Xamarin.Forms.ControlGallery.Android
{
	public class PerformanceTrackerRenderer : ViewRenderer, IOnDrawListener
	{
		public PerformanceTrackerRenderer(global::Android.Content.Context context): base(context)
		{
			ViewTreeObserver.AddOnDrawListener(this);
		}

		PerformanceTracker PerformanceTracker => Element as PerformanceTracker;

		void IOnDrawListener.OnDraw()
		{
			PerformanceTracker.Watcher.BeginTest(cleanup: () => UnsubscribeChildrenToDraw(this, this));
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == PerformanceTracker.ContentProperty.PropertyName)
			{
				PerformanceTracker.Watcher.ResetTest();

				SubscribeChildrenToDraw(this, this);
			}
		}

		static void SubscribeChildrenToDraw(AView view, IOnDrawListener observer)
		{
			if (view == null)
				return;

			view.ViewTreeObserver.AddOnDrawListener(observer);

			var viewGroup = view as ViewGroup;

			if (viewGroup == null)
				return;

			for (int i = 0; i < viewGroup.ChildCount; i++)
			{
				SubscribeChildrenToDraw(viewGroup.GetChildAt(i), observer);
			}
		}

		static void UnsubscribeChildrenToDraw(AView view, IOnDrawListener observer)
		{
			if (view == null)
				return;

			view.ViewTreeObserver.RemoveOnDrawListener(observer);

			var viewGroup = view as ViewGroup;

			if (viewGroup == null)
				return;

			for (int i = 0; i < viewGroup.ChildCount; i++)
			{
				UnsubscribeChildrenToDraw(viewGroup.GetChildAt(i), observer);
			}
		}
	}
}