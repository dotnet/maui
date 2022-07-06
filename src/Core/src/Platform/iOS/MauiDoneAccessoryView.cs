#if IOS && !MACCATALYST
using System;
using CoreGraphics;
using UIKit;

namespace Microsoft.Maui.Platform
{
	internal class MauiDoneAccessoryView : UIToolbar
	{
		WeakReference<object>? _data;
		Action<object>? _doneClicked;

		public MauiDoneAccessoryView(Action<object> doneClicked, object data) : base(new CGRect(0, 0, UIScreen.MainScreen.Bounds.Width, 44))
		{
			BarStyle = UIBarStyle.Default;
			Translucent = true;
			_data = new WeakReference<object>(data);
			_doneClicked = doneClicked;
			var spacer = new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace);
			var doneButton = new UIBarButtonItem(UIBarButtonSystemItem.Done, OnClicked);

			SetItems(new[] { spacer, doneButton }, false);
		}

		void OnClicked(object? sender, EventArgs e)
		{
			if (_data?.TryGetTarget(out object? target) == true)
				_doneClicked?.Invoke(target);
		}

		public MauiDoneAccessoryView(Action doneClicked) : base(new CGRect(0, 0, UIScreen.MainScreen.Bounds.Width, 44))
		{
			BarStyle = UIBarStyle.Default;
			Translucent = true;

			var spacer = new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace);
			var doneButton = new UIBarButtonItem(UIBarButtonSystemItem.Done, (o, a) => doneClicked?.Invoke());
			SetItems(new[] { spacer, doneButton }, false);
		}
	}
}
#endif