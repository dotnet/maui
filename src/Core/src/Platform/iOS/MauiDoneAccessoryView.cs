#if IOS && !MACCATALYST
using System;
using CoreGraphics;
using UIKit;

namespace Microsoft.Maui.Platform
{
	internal class MauiDoneAccessoryView : UIView
	{
		const double AccessoryHeight = 64;
		const double ButtonSize = 52;
		const double ButtonPadding = 8;

		readonly BarButtonItemProxy _proxy;

		public MauiDoneAccessoryView() : base(new CGRect(0, 0, UIScreen.MainScreen.Bounds.Width, AccessoryHeight))
		{
			_proxy = new BarButtonItemProxy();
			ClipsToBounds = false;

			AddDoneButton(CreateDoneButton(_proxy.OnDataClicked));
		}

		UIButton? DoneButton => FindDoneButton(this);

		internal void SetDoneClicked(Action<object>? value) => _proxy.SetDoneClicked(value);

		internal void SetDataContext(object? dataContext) => _proxy.SetDataContext(dataContext);

		internal void SendDoneClicked()
		{
			var doneButton = DoneButton ?? throw new InvalidOperationException("The Done button was not found.");
			doneButton.SendActionForControlEvents(UIControlEvent.TouchUpInside);
		}

		public MauiDoneAccessoryView(Action doneClicked) : base(new CGRect(0, 0, UIScreen.MainScreen.Bounds.Width, AccessoryHeight))
		{
			_proxy = new BarButtonItemProxy(doneClicked);
			ClipsToBounds = false;

			AddDoneButton(CreateDoneButton(_proxy.OnClicked));
		}

		public override UIView? HitTest(CGPoint point, UIEvent? uievent)
		{
			var hitView = base.HitTest(point, uievent);

			if (hitView is null || Equals(hitView))
				return null;

			var doneButton = DoneButton;
			if (doneButton is null)
				return null;

			return doneButton.Equals(hitView) || hitView.IsDescendantOfView(doneButton) ? hitView : null;
		}

		static UIButton CreateDoneButton(EventHandler action)
		{
			var button = UIButton.FromType(UIButtonType.System);
			button.ClipsToBounds = false;
			button.TranslatesAutoresizingMaskIntoConstraints = false;
			button.AccessibilityLabel = "Done";
			button.TouchUpInside += action;

			var image = UIImage.GetSystemImage("xmark");
			if (OperatingSystem.IsIOSVersionAtLeast(15))
			{
				var configuration = OperatingSystem.IsIOSVersionAtLeast(26)
					? UIButtonConfiguration.GlassButtonConfiguration
					: UIButtonConfiguration.GrayButtonConfiguration;
				configuration.Image = image;
				configuration.CornerStyle = UIButtonConfigurationCornerStyle.Capsule;
				configuration.ButtonSize = UIButtonConfigurationSize.Large;
				configuration.BaseForegroundColor = UIColor.Label;
				button.Configuration = configuration;
			}
			else
			{
				button.SetImage(image, UIControlState.Normal);
				button.TintColor = UIColor.Label;
				button.BackgroundColor = UIColor.SecondarySystemBackground.ColorWithAlpha(0.9f);
				button.Layer.CornerRadius = (nfloat)(ButtonSize / 2);
			}

			return button;
		}

		void AddDoneButton(UIButton button)
		{
			AddSubview(button);

			NSLayoutConstraint.ActivateConstraints(new[]
			{
				button.CenterYAnchor.ConstraintEqualTo(CenterYAnchor),
				button.WidthAnchor.ConstraintEqualTo((nfloat)ButtonSize),
				button.HeightAnchor.ConstraintEqualTo((nfloat)ButtonSize),
				button.TrailingAnchor.ConstraintEqualTo(TrailingAnchor, -(nfloat)ButtonPadding),
			});
		}

		static UIButton? FindDoneButton(UIView view)
		{
			foreach (var subview in view.Subviews)
			{
				if (subview is UIButton button)
					return button;

				if (FindDoneButton(subview) is UIButton descendant)
					return descendant;
			}

			return null;
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