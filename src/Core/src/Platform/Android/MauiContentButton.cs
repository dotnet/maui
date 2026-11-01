using System;
using Android.Content;
using Android.Content.Res;
using Android.Graphics.Drawables;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Views.Accessibility;
using Google.Android.Material.Card;
using Google.Android.Material.Shape;
using Microsoft.Maui.Graphics;
using AColor = Android.Graphics.Color;
using AView = Android.Views.View;

namespace Microsoft.Maui.Platform
{
	internal sealed class MauiContentButton : MaterialCardView
	{
		readonly Context _context;
		Drawable.ConstantState? _defaultBackgroundState;
		Drawable? _defaultBackground;
		ColorStateList? _defaultBackgroundTint;
		ColorStateList? _defaultCardBackgroundColor;
		ColorStateList? _defaultRippleColor;
		ColorStateList? _defaultStrokeColor;
		ShapeAppearanceModel? _defaultShapeAppearanceModel;
		int _defaultStrokeWidth;
		float _defaultRadius;
		ImportantForAccessibility _contentImportantForAccessibility;
		AView? _content;
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
		}

		internal void CaptureNativeDefaults()
		{
			CaptureNativeDefaults(this);
		}

		internal void RefreshNativeDefaults()
		{
			using var materialCard = new MaterialCardView(_context);
			CaptureNativeDefaults(materialCard);
		}

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
			if (ReferenceEquals(_content, content) && content?.Parent == this)
				return;

			RestoreContentAccessibility();
			RemoveAllViews();

			_content = content;
			if (content is null)
				return;

			content.RemoveFromParent();
			_contentImportantForAccessibility = content.ImportantForAccessibility;
			content.ImportantForAccessibility = ImportantForAccessibility.NoHideDescendants;
			AddView(content);
		}

		void RestoreContentAccessibility()
		{
			_content?.ImportantForAccessibility = _contentImportantForAccessibility;

			_content = null;
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
			_nightMode = Resources?.Configuration?.UiMode & UiMode.NightMask ?? UiMode.NightUndefined;
			ThemeChanged?.Invoke(this, EventArgs.Empty);
		}

		protected override void OnDetachedFromWindow()
		{
			DetachedFromWindow?.Invoke(this, EventArgs.Empty);
			base.OnDetachedFromWindow();
		}

		protected override void OnConfigurationChanged(Configuration? newConfig)
		{
			base.OnConfigurationChanged(newConfig);
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
			var measured = contentView.CrossPlatformMeasure(widthConstraint, heightConstraint);
			var widthMode = MeasureSpec.GetMode(widthMeasureSpec);
			var heightMode = MeasureSpec.GetMode(heightMeasureSpec);

			var width = widthMode switch
			{
				MeasureSpecMode.Exactly => widthConstraint,
				MeasureSpecMode.AtMost => Math.Min(widthConstraint, measured.Width),
				_ => measured.Width,
			};
			var height = heightMode switch
			{
				MeasureSpecMode.Exactly => heightConstraint,
				MeasureSpecMode.AtMost => Math.Min(heightConstraint, measured.Height),
				_ => measured.Height,
			};

			SetMeasuredDimension(
				Math.Max(MinimumWidth, (int)_context.ToPixels(width)),
				Math.Max(MinimumHeight, (int)_context.ToPixels(height)));
		}

		protected override void OnLayout(bool changed, int left, int top, int right, int bottom)
		{
			if (CrossPlatformLayout is not IContentView contentView)
				return;

			var destination = _context.ToCrossPlatformRectInReferenceFrame(left, top, right, bottom);
			contentView.CrossPlatformArrange(destination);
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
				RestoreContentAccessibility();
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
	}
}
