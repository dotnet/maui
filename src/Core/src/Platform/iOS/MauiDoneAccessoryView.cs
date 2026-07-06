#if IOS && !MACCATALYST
using System;
using CoreGraphics;
using UIKit;

namespace Microsoft.Maui.Platform
{
	internal class MauiDoneAccessoryView : UIView
	{
		const double DoneToolbarWidth = 120;
		readonly BarButtonItemProxy _proxy;

		public MauiDoneAccessoryView() : base(new CGRect(0, 0, UIScreen.MainScreen.Bounds.Width, 44))
		{
			_proxy = new BarButtonItemProxy();
			var toolbar = CreateToolbar();
			var spacer = new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace);
			var doneButton = new UIBarButtonItem(UIBarButtonSystemItem.Done, _proxy.OnDataClicked);

			toolbar.SetItems(new[] { spacer, doneButton }, false);
			AddToolbar(toolbar);
		}

		internal UIBarButtonItem[]? Items => Toolbar?.Items;

		UIToolbar? Toolbar => Subviews.Length > 0 ? Subviews[0] as UIToolbar : null;

		internal void SetDoneClicked(Action<object>? value) => _proxy.SetDoneClicked(value);

		internal void SetDataContext(object? dataContext) => _proxy.SetDataContext(dataContext);

		public MauiDoneAccessoryView(Action doneClicked) : base(new CGRect(0, 0, UIScreen.MainScreen.Bounds.Width, 44))
		{
			_proxy = new BarButtonItemProxy(doneClicked);
			var toolbar = CreateToolbar();

			var spacer = new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace);
			var doneButton = new UIBarButtonItem(UIBarButtonSystemItem.Done, _proxy.OnClicked);
			toolbar.SetItems(new[] { spacer, doneButton }, false);
			AddToolbar(toolbar);
		}

		public override UIView? HitTest(CGPoint point, UIEvent? uievent)
		{
			var hitView = base.HitTest(point, uievent);

			if (hitView is null || Equals(hitView))
				return null;

			return hitView;
		}

		static UIToolbar CreateToolbar()
		{
			return new UIToolbar
			{
				BarStyle = UIBarStyle.Default,
				Translucent = true,
				TranslatesAutoresizingMaskIntoConstraints = false,
			};
		}

		void AddToolbar(UIToolbar toolbar)
		{
			AddSubview(toolbar);

			NSLayoutConstraint.ActivateConstraints(new[]
			{
				toolbar.TopAnchor.ConstraintEqualTo(TopAnchor),
				toolbar.BottomAnchor.ConstraintEqualTo(BottomAnchor),
				toolbar.TrailingAnchor.ConstraintEqualTo(TrailingAnchor),
				toolbar.WidthAnchor.ConstraintEqualTo((nfloat)DoneToolbarWidth),
			});
		}

		class BarButtonItemProxy
		{
			readonly Action? _doneClicked;
			Action<object>? _doneWithDataClicked;
			WeakReference<object>? _data;

			public BarButtonItemProxy() { }

			public BarButtonItemProxy(Action doneClicked)
			{
				_doneClicked = doneClicked;
			}

			public void SetDoneClicked(Action<object>? value) => _doneWithDataClicked = value;

			public void SetDataContext(object? dataContext) => _data = dataContext is null ? null : new(dataContext);

			public void OnDataClicked(object? sender, EventArgs e)
			{
				if (_data is not null && _data.TryGetTarget(out var data))
					_doneWithDataClicked?.Invoke(data);
			}

			public void OnClicked(object? sender, EventArgs e) => _doneClicked?.Invoke();
		}
	}
}
#endif