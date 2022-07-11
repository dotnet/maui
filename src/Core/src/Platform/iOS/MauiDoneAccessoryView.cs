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

		public MauiDoneAccessoryView() : base(new CGRect(0, 0, UIScreen.MainScreen.Bounds.Width, 44))
		{
			BarStyle = UIBarStyle.Default;
			Translucent = true;
			var spacer = new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace);
			var doneButton = new UIBarButtonItem(UIBarButtonSystemItem.Done, OnClicked);

			SetItems(new[] { spacer, doneButton }, false);
		}

		void OnClicked(object? sender, EventArgs e)
		{
			if (_data?.TryGetTarget(out object? target) == true)
				_doneClicked?.Invoke(target);
		}

		internal void SetDoneClicked(Action<object>? value) => _doneClicked = value;
		internal void SetDataContext(object? dataContext)
		{
			_data = null;
			if (dataContext == null)
				return;

			_data = new WeakReference<object>(dataContext);
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