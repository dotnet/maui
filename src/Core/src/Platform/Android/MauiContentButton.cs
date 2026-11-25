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
		ColorStateList? _defaultBackgroundShapeTint;
		ColorStateList? _defaultCardBackgroundColor;
		ColorStateList? _defaultRippleColor;
		ColorStateList? _defaultStrokeColor;
		ColorStateList? _mappedStrokeColor;
		ColorStateList? _transparentStrokeColor;
		ShapeAppearanceModel? _initialShapeAppearanceModel;
		ShapeAppearanceModel? _defaultShapeAppearanceModel;
		ShapeAppearanceModel? _mappedShapeAppearanceModel;
		float _mappedShapeRadius;
		bool _mappedShapeUsesDefault;
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
			// Handler subclasses can change the shape before native defaults are captured.
			_initialShapeAppearanceModel = ShapeAppearanceModel;
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
			_preserveDefaultShapeAppearanceModel = !Equals(ShapeAppearanceModel, _initialShapeAppearanceModel);
			_preserveDefaultRadius = _preserveDefaultShapeAppearanceModel || Radius != themedCard.Radius;
			CaptureNativeDefaults(this);
		}

		internal void RefreshNativeDefaults()
		{
			using var materialCard = new MaterialCardView(_context);
			if (!_preserveDefaultBackground)
			{
				_defaultBackgroundState = materialCard.Background?.GetConstantState();
				_defaultBackground = _defaultBackgroundState?.NewDrawable()?.Mutate() ?? materialCard.Background;
				_defaultBackgroundShapeTint = GetBackgroundShapeTint(materialCard.Background);
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
			first?.GetConstantState()?.GetType() == second?.GetConstantState()?.GetType() &&
			first?.Alpha == second?.Alpha &&
			HaveEquivalentColors(GetBackgroundShapeTint(first), GetBackgroundShapeTint(second));

		static ColorStateList? GetBackgroundShapeTint(Drawable? background) =>
			background is InsetDrawable { Drawable: MaterialShapeDrawable shape } ? shape.TintList : null;

		static bool HaveEquivalentColors(ColorStateList? first, ColorStateList? second) =>
			ReferenceEquals(first, second) || first?.ToString() == second?.ToString();

		void CaptureNativeDefaults(MaterialCardView materialCard)
		{
			_defaultBackgroundState = materialCard.Background?.GetConstantState();
			_defaultBackground = _defaultBackgroundState?.NewDrawable()?.Mutate() ?? materialCard.Background;
			_defaultBackgroundTint = materialCard.BackgroundTintList;
			_defaultBackgroundShapeTint = GetBackgroundShapeTint(materialCard.Background);
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

			// Removing or adding views runs native attach/detach callbacks, which can map newer
			// content before this call finishes. Stop as soon as this content is superseded.
			_content = content;
			_contentContainer?.RemoveAllViews();
			if (!ReferenceEquals(_content, content))
				return;

			RemoveAllViews();
			if (!ReferenceEquals(_content, content) || content is null)
				return;

			content.RemoveFromParent();
			if (!ReferenceEquals(_content, content))
				return;

			_contentContainer ??= new ContentContainer(_context);
			_contentContainer.AddView(content);
			if (ReferenceEquals(_content, content) && _contentContainer.Parent is null)
				AddView(_contentContainer);
		}

		internal void UpdateButtonBackground(IContentButton button)
		{
			if (button.Background.IsNullOrEmpty())
			{
				Background = _defaultBackgroundState?.NewDrawable()?.Mutate() ?? _defaultBackground;
				BackgroundTintList = _defaultBackgroundTint;
				if (_defaultCardBackgroundColor is not null)
					CardBackgroundColor = _defaultCardBackgroundColor;

				UpdateButtonStroke(button, updateShape: false);
				// Assigning BackgroundTintList can clear a tint applied directly to the drawable.
				if (_defaultBackgroundShapeTint is not null &&
					Background is InsetDrawable { Drawable: MaterialShapeDrawable shape })
				{
					shape.SetTintList(_defaultBackgroundShapeTint);
				}
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
					SetMappedStrokeColor(TransparentStrokeColor);
				});

			// The background refresh keeps the existing stroke layer, or recreates it from
			// the same stroke inputs, so only native-default adjustments need reapplying.
			UpdateButtonStroke(button, updateShape: false, refreshRippleStroke: false);
		}

		internal void UpdateButtonStroke(IContentButton button, bool updateShape = true, bool updateColor = false, bool refreshRippleStroke = true)
		{
			// Update framework-owned shapes and colors, but leave mapper customizations intact.
			updateShape |= _mappedShapeAppearanceModel is not null &&
				Equals(ShapeAppearanceModel, _mappedShapeAppearanceModel);
			updateColor |= _mappedStrokeColor is null || _mappedStrokeColor.Equals(StrokeColorStateList);

			// Unset stroke values use the native defaults. A handler-customized native width or
			// color is kept even when only the other stroke property is set explicitly.
			var useDefaultWidth = button.StrokeThickness < 0 &&
				(button.StrokeColor is null || _preserveDefaultStrokeWidth);
			var useDefaultColor = button.StrokeColor is null &&
				(button.StrokeThickness < 0 || _preserveDefaultStrokeColor);

			if (this.TryGetMauiBackground(out _, out _, out var background, out var stroke, out var mask))
			{
				if (refreshRippleStroke)
					this.UpdateMauiRippleDrawableStroke(button);

				if (useDefaultWidth || useDefaultColor)
				{
					var strokeWidth = useDefaultWidth ? _defaultStrokeWidth : (int)_context.ToPixels(button.StrokeThickness);
					var strokeColor = useDefaultColor
						? _defaultStrokeColor
						: ColorStateListExtensions.CreateButton(button.StrokeColor!.ToPlatform());
					stroke.SetStroke(strokeWidth, strokeColor);
					background.SetStroke(strokeWidth, AColor.Transparent);

					if (button.CornerRadius >= 0)
					{
						// Match GetStrokeProperties, which insets the radius by half the stroke width.
						var radius = (int)_context.ToPixels(button.CornerRadius) - strokeWidth / 2;
						background.SetCornerRadius(radius);
						stroke.SetCornerRadius(radius);
						mask.SetCornerRadius(radius);
					}
				}

				if (button.CornerRadius < 0)
				{
					background.SetCornerRadius(_defaultRadius);
					stroke.SetCornerRadius(_defaultRadius);
					mask.SetCornerRadius(_defaultRadius);
					if (updateShape)
						UpdateShape(_defaultRadius, true);
				}
				else if (updateShape)
				{
					UpdateShape(background.CornerRadius, false);
				}

				StrokeWidth = 0;
				// A MAUI background draws the stroke; hide the card stroke without reallocating per refresh.
				if (updateColor && !TransparentStrokeColor.Equals(StrokeColorStateList))
					SetMappedStrokeColor(TransparentStrokeColor);
				return;
			}

			if (useDefaultWidth && useDefaultColor)
			{
				StrokeWidth = _defaultStrokeWidth;
				if (updateColor)
					SetMappedStrokeColor(_defaultStrokeColor);
			}
			else if (useDefaultColor)
			{
				StrokeWidth = (int)_context.ToPixels(button.StrokeThickness);
				if (updateColor)
					SetMappedStrokeColor(_defaultStrokeColor);
			}
			else
			{
				var (width, color, _) = button.GetStrokeProperties(_context, true);
				StrokeWidth = useDefaultWidth ? _defaultStrokeWidth : width;
				if (updateColor)
					SetMappedStrokeColor(color);
			}

			if (!updateShape)
				return;

			if (button.CornerRadius < 0)
				UpdateShape(_defaultRadius, true);
			else
				UpdateShape((int)_context.ToPixels(button.CornerRadius), false);
		}

		void SetMappedStrokeColor(ColorStateList? color)
		{
			SetStrokeColor(color);
			_mappedStrokeColor = StrokeColorStateList;
		}

		ColorStateList TransparentStrokeColor =>
			_transparentStrokeColor ??= ColorStateList.ValueOf(AColor.Transparent);

		internal bool HasMauiBackground =>
			this.TryGetMauiBackground(out _, out _, out _, out _, out _);

		void UpdateShape(float radius, bool useDefaultShape)
		{
			if (_mappedShapeAppearanceModel is not null &&
				Equals(ShapeAppearanceModel, _mappedShapeAppearanceModel) &&
				_mappedShapeRadius == radius && _mappedShapeUsesDefault == useDefaultShape &&
				(!useDefaultShape || Equals(_mappedShapeAppearanceModel, _defaultShapeAppearanceModel)))
			{
				return;
			}

			if (useDefaultShape && _defaultShapeAppearanceModel is not null)
			{
				ShapeAppearanceModel = _defaultShapeAppearanceModel;
			}
			else if (useDefaultShape)
				Radius = radius;
			else
			{
				ShapeAppearanceModel = ShapeAppearanceModel
					.ToBuilder()
					.SetAllCorners(CornerFamily.Rounded, radius)
					.Build();
				Radius = radius;
			}

			_mappedShapeAppearanceModel = ShapeAppearanceModel;
			_mappedShapeRadius = radius;
			_mappedShapeUsesDefault = useDefaultShape;
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
			var availableWidth = Math.Max(0, Math.Max(widthConstraint, _context.FromPixels(MinimumWidth)) - paddingLeft - paddingRight);
			var availableHeight = Math.Max(0, Math.Max(heightConstraint, _context.FromPixels(MinimumHeight)) - paddingTop - paddingBottom);
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
			// Child arrangement is mirrored in RTL, but window insets are physical.
			var contentOffset = _contentContainer?.LayoutDirection == global::Android.Views.LayoutDirection.Rtl
				? paddingRight
				: paddingLeft;
			destination = new Rect(
				destination.X + contentOffset,
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

			SetSafeAreaPadding(left, top, right, bottom);
		}

		internal void SetAnimatedSafeAreaBottom(int bottom) =>
			SetSafeAreaPadding(_safeAreaPadding.left, _safeAreaPadding.top, _safeAreaPadding.right, bottom);

		void SetSafeAreaPadding(int left, int top, int right, int bottom)
		{
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
				_defaultBackgroundShapeTint = null;
				_defaultCardBackgroundColor = null;
				_defaultRippleColor = null;
				_defaultStrokeColor = null;
				_initialShapeAppearanceModel = null;
				_defaultShapeAppearanceModel = null;
				_mappedShapeAppearanceModel = null;
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
