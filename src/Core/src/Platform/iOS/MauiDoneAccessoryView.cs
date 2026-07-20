#if IOS && !MACCATALYST
using System;
using CoreGraphics;
using Foundation;
using UIKit;

namespace Microsoft.Maui.Platform
{
	internal class MauiDoneAccessoryView : UIView
	{
		const double AccessoryHeight = 44;

		// UIKit's localized "Done" label, matching UIBarButtonSystemItem.Done so VoiceOver keeps reading
		// the affordance in the user's language on the iOS 26+ glass button path.
		static readonly string DoneAccessibilityLabel =
			NSBundle.FromIdentifier("com.apple.UIKit").GetLocalizedString("Done");

		// iOS 26 gives bars a translucent Liquid Glass background, so a full-width accessory looks empty
		// and appears to let taps through to the field behind it (dotnet/maui#36412). On those versions
		// we replace the bar with a floating, pass-through close button. Earlier iOS keeps the original
		// full-width touch-blocking Done toolbar, so the accessory already blocks taps behind it.
		static bool UseGlassButton => OperatingSystem.IsIOSVersionAtLeast(26);

		readonly BarButtonItemProxy _proxy;

		// Cache the discovered button so hit-testing (iOS 26+) doesn't walk the subview tree on every
		// touch. It's held weakly because the button is already retained by the native Subviews array,
		// so a strong managed field here would trip the MEM0002 memory-leak analyzer for NSObject
		// subclasses. The weak reference effectively always resolves while the button is a subview.
		WeakReference<UIButton>? _doneButton;

		public MauiDoneAccessoryView() : base(InitialFrame())
		{
			_proxy = new BarButtonItemProxy();
			Initialize(_proxy.OnDataClicked);
		}

		public MauiDoneAccessoryView(Action doneClicked) : base(InitialFrame())
		{
			_proxy = new BarButtonItemProxy(doneClicked);
			Initialize(_proxy.OnClicked);
		}

		internal UIButton? DoneButton
		{
			get
			{
				if (_doneButton is not null && _doneButton.TryGetTarget(out var cached))
					return cached;

				var button = FindDoneButton(this);
				_doneButton = button is null ? null : new WeakReference<UIButton>(button);
				return button;
			}
		}

		internal void SetDoneClicked(Action<object>? value) => _proxy.SetDoneClicked(value);

		internal void SetDataContext(object? dataContext) => _proxy.SetDataContext(dataContext);

		internal void SendDoneClicked()
		{
			if (UseGlassButton && DoneButton is UIButton button)
				button.SendActionForControlEvents(UIControlEvent.TouchUpInside);
			else
				_proxy.Invoke();
		}

		public override UIView? HitTest(CGPoint point, UIEvent? uievent)
		{
			var hitView = base.HitTest(point, uievent);

			// The classic toolbar (iOS < 26) keeps its original full-width touch-blocking behavior.
			if (!UseGlassButton)
				return hitView;

			// The floating glass button (iOS 26+) lets taps pass through everywhere except the button
			// itself, so the field showing behind the accessory stays tappable.
			if (hitView is null || Equals(hitView))
				return null;

			var doneButton = DoneButton;
			if (doneButton is null)
				return null;

			return doneButton.Equals(hitView) || hitView.IsDescendantOfView(doneButton) ? hitView : null;
		}

		static CGRect InitialFrame() => new(0, 0, UIScreen.MainScreen.Bounds.Width, AccessoryHeight);

		void Initialize(EventHandler doneClicked)
		{
			if (UseGlassButton)
				InitializeGlassButton(doneClicked);
			else
				InitializeToolbar(doneClicked);
		}

		// iOS < 26: the original translucent toolbar with the system Done item. It sizes itself and is
		// already localized, so there are no hard-coded metrics here.
		void InitializeToolbar(EventHandler doneClicked)
		{
			var toolbar = new UIToolbar(Bounds)
			{
				BarStyle = UIBarStyle.Default,
				Translucent = true,
				AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight,
			};

			var spacer = new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace);
			var doneButton = new UIBarButtonItem(UIBarButtonSystemItem.Done, doneClicked);
			toolbar.SetItems(new[] { spacer, doneButton }, false);

			AddSubview(toolbar);
		}

		// iOS 26+: a floating Liquid Glass close button pinned to the trailing layout margin. Keep the
		// original accessory height so UIKit continues scrolling focused input above the keyboard.
		void InitializeGlassButton(EventHandler doneClicked)
		{
			ClipsToBounds = false;

			var button = CreateGlassButton(doneClicked);
			_doneButton = new WeakReference<UIButton>(button);
			AddSubview(button);

			NSLayoutConstraint.ActivateConstraints(new[]
			{
				button.WidthAnchor.ConstraintEqualTo((nfloat)AccessoryHeight),
				button.HeightAnchor.ConstraintEqualTo((nfloat)AccessoryHeight),
				button.CenterYAnchor.ConstraintEqualTo(CenterYAnchor),
				button.TrailingAnchor.ConstraintEqualTo(LayoutMarginsGuide.TrailingAnchor),
			});
		}

		static UIButton CreateGlassButton(EventHandler action)
		{
			var button = UIButton.FromType(UIButtonType.System);
			button.ClipsToBounds = false;
			button.TranslatesAutoresizingMaskIntoConstraints = false;
			button.AccessibilityLabel = DoneAccessibilityLabel;
			button.TouchUpInside += action;

			// GlassButtonConfiguration is always available here because this path only runs on iOS 26+.
			var configuration = UIButtonConfiguration.GlassButtonConfiguration;
			configuration.Image = UIImage.GetSystemImage("xmark");
			configuration.CornerStyle = UIButtonConfigurationCornerStyle.Capsule;
			configuration.ButtonSize = UIButtonConfigurationSize.Medium;
			configuration.BaseForegroundColor = UIColor.Label;
			button.Configuration = configuration;

			return button;
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

			// Invokes whichever handler this proxy was configured with. Used by the < iOS 26 toolbar
			// path, where there is no UIButton to actuate directly.
			public void Invoke()
			{
				_doneClicked?.Invoke();

				if (_data is not null && _data.TryGetTarget(out var data))
					_doneWithDataClicked?.Invoke(data);
			}
		}
	}
}
#endif