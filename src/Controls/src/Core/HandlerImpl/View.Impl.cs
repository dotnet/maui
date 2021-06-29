#nullable enable

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Controls.Platform;

namespace Microsoft.Maui.Controls
{
	public partial class View
	{
		GestureManager? _gestureManager;
		private protected override void OnAttachedHandlerCore()
		{
			base.OnAttachedHandlerCore();

			_gestureManager?.Dispose();
			_gestureManager = new GestureManager(Handler);
			_gestureRecognizers.CollectionChanged += OnGestureRecognizersCollectionChanged;

#if __ANDROID__
			var nativeview = (Handler as INativeViewHandler)?.NativeView;
			if (nativeview != null)
			{
				nativeview.Touch += Nativeview_Touch;
			}
#endif
		}

#if __ANDROID__
		private void Nativeview_Touch(object? sender, Android.Views.View.TouchEventArgs e)
		{
			if (e.Event != null)
				_gestureManager?.OnTouchEvent(e.Event);
		}
#endif

		private protected override void OnDetachingHandlerCore()
		{
			base.OnDetachingHandlerCore();
			_gestureManager?.Dispose();
			_gestureManager = null;
			_gestureRecognizers.CollectionChanged -= OnGestureRecognizersCollectionChanged;
		}


		void OnGestureRecognizersCollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
		}

		void SetupGestures()
		{

		}
	}
}
