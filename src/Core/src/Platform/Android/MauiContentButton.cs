using System;
using Android.Content;
using Android.Content.Res;
using Android.Graphics.Drawables;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Views.Accessibility;
using AndroidX.Core.View;
using Google.Android.Material.Card;
using Google.Android.Material.Shape;
using Microsoft.Maui.Graphics;
using AColor = Android.Graphics.Color;
using AView = Android.Views.View;

namespace Microsoft.Maui.Platform
{
	internal sealed class MauiContentButton : MaterialCardView, ICrossPlatformLayoutBacking, IHandleWindowInsets
	{
		readonly Context _context;
		bool _didSafeAreaEdgeConfigurationChange = true;
		Drawable.ConstantState? _defaultBackgroundState;
		Drawable? _defaultBackground;
		ColorStateList? _defaultBackgroundTint;
		ColorStateList? _defaultCardBackgroundColor;
		ColorStateList? _defaultRippleColor;
		ColorStateList? _defaultStrokeColor;
		ShapeAppearanceModel? _defaultShapeAppearanceModel;
		int _defaultStrokeWidth;
		float _defaultRadius;
		bool _isInsetListenerSet;
		bool _preserveDefaultBackground;
		bool _preserveDefaultBackgroundTint;
		bool _preserveDefaultCardBackgroundColor;
		bool _preserveDefaultRippleColor;
		bool _preserveDefaultStrokeColor;
		bool _preserveDefaultStrokeWidth;
		bool _preserveDefaultShapeAppearanceModel;
		bool _preserveDefaultRadius;
		(int left, int top, int right, int bottom) _safeAreaPadding;
		bool _isApplyingSafeAreaInsets;
		AView? _content;
		ContentContainer? _contentContainer;
		UiMode _nightMode;

		internal event EventHandler? DetachedFromWindow;
		internal event EventHandler? ThemeChanged;

		internal IContentButton? CrossPlatformLayout { get; set; }

		public MauiContentButton(Context context) : base(context)
		{
			_context = context;
			Initialize();
		}

		public MauiContentButton(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
		{
			_context = Context ?? throw new InvalidOperationException("The native ContentButton requires an Android context.");
			Initialize();
		}

		public MauiContentButton(Context context, IAttributeSet attrs) : base(context, attrs)
		{
			_context = context;
			Initialize();
		}

		public MauiContentButton(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
		{
			_context = context;
			Initialize();
		}

		void Initialize()
		{
			Clickable = true;
			Focusable = true;
			ImportantForAccessibility = ImportantForAccessibility.Yes;
			SetClipChildren(true);
			_nightMode = Resources?.Configuration?.UiMode & UiMode.NightMask ?? UiMode.NightUndefined;
		}

		internal void CaptureNativeDefaults()
		{
			using var themedCard = new MaterialCardView(_context);
			_preserveDefaultBackground = !HaveEquivalentBackground(Background, themedCard.Background);
			_preserveDefaultBackgroundTint = !HaveEquivalentColors(BackgroundTintList, themedCard.BackgroundTintList);
			_preserveDefaultCardBackgroundColor = !HaveEquivalentColors(CardBackgroundColor, themedCard.CardBackgroundColor);
			_preserveDefaultRippleColor = !HaveEquivalentColors(RippleColor, themedCard.RippleColor);
			_preserveDefaultStrokeColor = !HaveEquivalentColors(StrokeColorStateList, themedCard.StrokeColorStateList);
			_preserveDefaultStrokeWidth = StrokeWidth != themedCard.StrokeWidth;
			_preserveDefaultRadius = Radius != themedCard.Radius;
			_preserveDefaultShapeAppearanceModel = _preserveDefaultRadius;
			CaptureNativeDefaults(this);
		}

		internal void RefreshNativeDefaults()
		{
			using var materialCard = new MaterialCardView(_context);
			if (!_preserveDefaultBackground)
			{
				_defaultBackgroundState = materialCard.Background?.GetConstantState();
				_defaultBackground = _defaultBackgroundState?.NewDrawable()?.Mutate() ?? materialCard.Background;
			}

			if (!_preserveDefaultBackgroundTint)
				_defaultBackgroundTint = materialCard.BackgroundTintList;
			if (!_preserveDefaultCardBackgroundColor)
				_defaultCardBackgroundColor = materialCard.CardBackgroundColor;
			if (!_preserveDefaultRippleColor)
				_defaultRippleColor = materialCard.RippleColor;
			if (!_preserveDefaultStrokeColor)
				_defaultStrokeColor = materialCard.StrokeColorStateList;
			if (!_preserveDefaultStrokeWidth)
				_defaultStrokeWidth = materialCard.StrokeWidth;
			if (!_preserveDefaultShapeAppearanceModel)
				_defaultShapeAppearanceModel = materialCard.ShapeAppearanceModel;
			if (!_preserveDefaultRadius)
				_defaultRadius = materialCard.Radius;
		}

		static bool HaveEquivalentBackground(Drawable? first, Drawable? second) =>
			first?.GetType() == second?.GetType() &&
			first?.GetConstantState()?.GetType() == second?.GetConstantState()?.GetType();

		static bool HaveEquivalentColors(ColorStateList? first, ColorStateList? second) =>
			ReferenceEquals(first, second) || first?.ToString() == second?.ToString();

		void CaptureNativeDefaults(MaterialCardView materialCard)
		{
			_defaultBackgroundState = materialCard.Background?.GetConstantState();
			_defaultBackground = _defaultBackgroundState?.NewDrawable()?.Mutate() ?? materialCard.Background;
			_defaultBackgroundTint = materialCard.BackgroundTintList;
			_defaultCardBackgroundColor = materialCard.CardBackgroundColor;
			_defaultRippleColor = materialCard.RippleColor;
			_defaultStrokeColor = materialCard.StrokeColorStateList;
			_defaultStrokeWidth = materialCard.StrokeWidth;
			_defaultShapeAppearanceModel = materialCard.ShapeAppearanceModel;
			_defaultRadius = materialCard.Radius;
		}

		internal void SetContent(AView? content)
		{
			if (ReferenceEquals(_content, content) && content?.Parent == _contentContainer)
				return;

			_contentContainer?.RemoveAllViews();
			RemoveAllViews();

			_content = content;
			if (content is null)
				return;

			content.RemoveFromParent();
			_contentContainer ??= new ContentContainer(_context);
			_contentContainer.AddView(content);
			AddView(_contentContainer);
		}

		internal void UpdateButtonBackground(IContentButton button)
		{
			if (button.Background.IsNullOrEmpty())
			{
				Background = _defaultBackgroundState?.NewDrawable()?.Mutate() ?? _defaultBackground;
				BackgroundTintList = _defaultBackgroundTint;
				if (_defaultCardBackgroundColor is not null)
					SetCardBackgroundColor(_defaultCardBackgroundColor.DefaultColor);

				UpdateButtonStroke(button);
				return;
			}

			this.UpdateMauiRippleDrawableBackground(
				button.Background,
				button,
				getDefaultRippleColor: () => RippleColor ?? ResolveRippleColor(),
				beforeSet: () =>
				{
					BackgroundTintList = null;
					StrokeWidth = 0;
					SetStrokeColor(ColorStateList.ValueOf(AColor.Transparent));
				});

			UpdateButtonStroke(button);
		}

		internal void UpdateButtonStroke(IContentButton button)
		{
			if (this.UpdateMauiRippleDrawableStroke(button))
			{
				if (this.TryGetMauiBackground(out _, out _, out var background, out var stroke, out var mask))
				{
					if (button.StrokeColor is null && button.StrokeThickness < 0)
					{
						stroke.SetStroke(_defaultStrokeWidth, _defaultStrokeColor);
						background.SetStroke(_defaultStrokeWidth, AColor.Transparent);
					}

					var radius = button.CornerRadius < 0
						? _defaultRadius
						: button.GetStrokeProperties(_context, false).Radius;

					background.SetCornerRadius(radius);
					stroke.SetCornerRadius(radius);
					mask.SetCornerRadius(radius);
					UpdateShape(radius, button.CornerRadius < 0);
				}

				StrokeWidth = 0;
				SetStrokeColor(ColorStateList.ValueOf(AColor.Transparent));
				return;
			}

			if (button.StrokeColor is null && button.StrokeThickness < 0)
			{
				StrokeWidth = _defaultStrokeWidth;
				SetStrokeColor(_defaultStrokeColor);
			}
			else
			{
				var (width, color, _) = button.GetStrokeProperties(_context, true);
				StrokeWidth = width;
				SetStrokeColor(color);
			}

			if (button.CornerRadius < 0)
			{
				if (_defaultShapeAppearanceModel is not null)
					ShapeAppearanceModel = _defaultShapeAppearanceModel;
				Radius = _defaultRadius;
			}
			else
			{
				var (_, _, radius) = button.GetStrokeProperties(_context, true);
				UpdateShape(radius, false);
			}
		}

		void UpdateShape(float radius, bool useDefaultShape)
		{
			if (useDefaultShape && _defaultShapeAppearanceModel is not null)
			{
				ShapeAppearanceModel = _defaultShapeAppearanceModel;
				Radius = _defaultRadius;
				return;
			}

			ShapeAppearanceModel = ShapeAppearanceModel
				.ToBuilder()
				.SetAllCorners(CornerFamily.Rounded, radius)
				.Build();
			Radius = radius;
		}

		internal void UpdateRippleColor(Color? rippleColor)
		{
			var colors = rippleColor is null
				? ResolveRippleColor()
				: ColorStateList.ValueOf(rippleColor.ToPlatform());

			RippleColor = colors;

			if (Background is RippleDrawable ripple)
				ripple.SetColor(colors);
		}

		ColorStateList ResolveRippleColor() =>
			_defaultRippleColor ?? ColorStateList.ValueOf(AColor.Transparent);

		protected override void OnAttachedToWindow()
		{
			base.OnAttachedToWindow();
			_isInsetListenerSet = MauiWindowInsetListenerExtensions.TrySetMauiWindowInsetListener(this, _context);

			var nightMode = Resources?.Configuration?.UiMode & UiMode.NightMask ?? UiMode.NightUndefined;
			if (_nightMode != nightMode)
			{
				_nightMode = nightMode;
				ThemeChanged?.Invoke(this, EventArgs.Empty);
			}
		}

		protected override void OnDetachedFromWindow()
		{
			DetachedFromWindow?.Invoke(this, EventArgs.Empty);
			base.OnDetachedFromWindow();
			if (_isInsetListenerSet)
				MauiWindowInsetListenerExtensions.RemoveMauiWindowInsetListener(this, _context);
			_didSafeAreaEdgeConfigurationChange = true;
			_isInsetListenerSet = false;
		}

		protected override void OnConfigurationChanged(Configuration? newConfig)
		{
			base.OnConfigurationChanged(newConfig);
			MauiWindowInsetListener.FindListenerForView(this)?.ResetView(this);
			_didSafeAreaEdgeConfigurationChange = true;

			if (newConfig is null)
				return;

			var nightMode = newConfig.UiMode & UiMode.NightMask;
			if (_nightMode == nightMode)
				return;

			_nightMode = nightMode;
			ThemeChanged?.Invoke(this, EventArgs.Empty);
		}

		protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
		{
			if (CrossPlatformLayout is not IContentView contentView)
			{
				base.OnMeasure(widthMeasureSpec, heightMeasureSpec);
				return;
			}

			var widthConstraint = widthMeasureSpec.ToDouble(_context);
			var heightConstraint = heightMeasureSpec.ToDouble(_context);
			var paddingLeft = _context.FromPixels(_safeAreaPadding.left);
			var paddingTop = _context.FromPixels(_safeAreaPadding.top);
			var paddingRight = _context.FromPixels(_safeAreaPadding.right);
			var paddingBottom = _context.FromPixels(_safeAreaPadding.bottom);
			var availableWidth = Math.Max(0, widthConstraint - paddingLeft - paddingRight);
			var availableHeight = Math.Max(0, heightConstraint - paddingTop - paddingBottom);
			var measured = contentView.CrossPlatformMeasure(availableWidth, availableHeight);
			var widthMode = MeasureSpec.GetMode(widthMeasureSpec);
			var heightMode = MeasureSpec.GetMode(heightMeasureSpec);

			var width = widthMode switch
			{
				MeasureSpecMode.Exactly => widthConstraint,
				MeasureSpecMode.AtMost => Math.Min(widthConstraint, measured.Width + paddingLeft + paddingRight),
				_ => measured.Width + paddingLeft + paddingRight,
			};
			var height = heightMode switch
			{
				MeasureSpecMode.Exactly => heightConstraint,
				MeasureSpecMode.AtMost => Math.Min(heightConstraint, measured.Height + paddingTop + paddingBottom),
				_ => measured.Height + paddingTop + paddingBottom,
			};

			SetMeasuredDimension(
				Math.Max(MinimumWidth, (int)_context.ToPixels(width)),
				Math.Max(MinimumHeight, (int)_context.ToPixels(height)));

			_contentContainer?.Measure(
				MeasureSpec.MakeMeasureSpec(MeasuredWidth, MeasureSpecMode.Exactly),
				MeasureSpec.MakeMeasureSpec(MeasuredHeight, MeasureSpecMode.Exactly));
		}

		protected override void OnLayout(bool changed, int left, int top, int right, int bottom)
		{
			if (CrossPlatformLayout is not IContentView contentView)
				return;

			_contentContainer?.Layout(0, 0, right - left, bottom - top);

			var destination = _context.ToCrossPlatformRectInReferenceFrame(left, top, right, bottom);
			var paddingLeft = _context.FromPixels(_safeAreaPadding.left);
			var paddingTop = _context.FromPixels(_safeAreaPadding.top);
			var paddingRight = _context.FromPixels(_safeAreaPadding.right);
			var paddingBottom = _context.FromPixels(_safeAreaPadding.bottom);
			destination = new Rect(
				destination.X + paddingLeft,
				destination.Y + paddingTop,
				Math.Max(0, destination.Width - paddingLeft - paddingRight),
				Math.Max(0, destination.Height - paddingTop - paddingBottom));
			contentView.CrossPlatformArrange(destination);

			if (_isInsetListenerSet &&
				(_didSafeAreaEdgeConfigurationChange ||
					(changed && SafeAreaExtensions.ShouldReapplyInsetsForResize(contentView, this))))
			{
				ViewCompat.RequestApplyInsets(this);
				_didSafeAreaEdgeConfigurationChange = false;
			}
		}

		internal void MarkSafeAreaEdgeConfigurationChanged()
		{
			_isInsetListenerSet = MauiWindowInsetListenerExtensions.RefreshMauiWindowInsetListener(this, _context);
			_didSafeAreaEdgeConfigurationChange = true;
			RequestLayout();
		}

		ICrossPlatformLayout? ICrossPlatformLayoutBacking.CrossPlatformLayout
		{
			get => CrossPlatformLayout;
			set => CrossPlatformLayout = value as IContentButton;
		}

		WindowInsetsCompat? IHandleWindowInsets.HandleWindowInsets(AView view, WindowInsetsCompat insets)
		{
			if (CrossPlatformLayout is null)
				return insets;

			_isApplyingSafeAreaInsets = true;
			try
			{
				return SafeAreaExtensions.ApplyAdjustedSafeAreaInsetsPx(insets, CrossPlatformLayout, _context, view);
			}
			finally
			{
				_isApplyingSafeAreaInsets = false;
			}
		}

		void IHandleWindowInsets.ResetWindowInsets(AView view)
		{
			if (_safeAreaPadding != default)
			{
				_safeAreaPadding = default;
				RequestLayout();
			}
		}

		public override void SetPadding(int left, int top, int right, int bottom)
		{
			if (!_isApplyingSafeAreaInsets)
			{
				base.SetPadding(left, top, right, bottom);
				return;
			}

			var safeAreaPadding = (left, top, right, bottom);
			if (_safeAreaPadding != safeAreaPadding)
			{
				_safeAreaPadding = safeAreaPadding;
				RequestLayout();
			}
		}

		public override bool OnInterceptTouchEvent(MotionEvent? ev) => true;

		public override void OnInitializeAccessibilityNodeInfo(AccessibilityNodeInfo? info)
		{
			base.OnInitializeAccessibilityNodeInfo(info);
			info?.ClassName = "android.widget.Button";
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				SetContent(null);
				_contentContainer?.Dispose();
				_contentContainer = null;
				CrossPlatformLayout = null;
				_defaultBackground = null;
				_defaultBackgroundState = null;
				_defaultBackgroundTint = null;
				_defaultCardBackgroundColor = null;
				_defaultRippleColor = null;
				_defaultStrokeColor = null;
				_defaultShapeAppearanceModel = null;
			}

			base.Dispose(disposing);
		}

		sealed class ContentContainer : ViewGroup
		{
			public ContentContainer(Context context) : base(context)
			{
				// Keep presentation hidden even when its own semantics change.
				ImportantForAccessibility = ImportantForAccessibility.NoHideDescendants;
			}

			protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec) =>
				SetMeasuredDimension(MeasureSpec.GetSize(widthMeasureSpec), MeasureSpec.GetSize(heightMeasureSpec));

			protected override void OnLayout(bool changed, int left, int top, int right, int bottom)
			{
				// The ContentButton's cross-platform layout arranges the presentation.
			}
		}
	}
}
