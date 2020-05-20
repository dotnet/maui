using System;
using Android.Content;

using System.ComponentModel;
using Android.Content.Res;
using Android.Graphics;
#if __ANDROID_29__
using AndroidX.Core.View;
#else
using Android.Support.V4.View;
#endif
using Android.Text;
using Android.Views;
using Android.Widget;

namespace System.Maui.Platform
{
	public partial class LabelRenderer : AbstractViewRenderer<ILabel, TextView>
	{
		ColorStateList _labelTextColorDefault;
		Color _lastUpdateColor = Color.Default;
		float _lineSpacingExtraDefault = -1.0f;
		float _lineSpacingMultiplierDefault = -1.0f;

		int _lastConstraintHeight;
		int _lastConstraintWidth;
		SizeRequest? _lastSizeRequest;

		protected override TextView CreateView()
		{
			var text = new TextView(Context);
			
			// Doing this temporarily to make the Label sensible; letting it do multiple lines
			// is causing it to explode all over the page. We'll figure that out later.
			text.SetMaxLines(1);
			
			_labelTextColorDefault = text.TextColors;
			return text;
		}

		public static void MapPropertyText(IViewRenderer renderer, IText view)
		{
			var textView = renderer.NativeView as TextView;
			if (textView == null)
				return;
			//TODO: lets make a way for any renderer to tell it to reset last cached size request
			if(renderer is LabelRenderer lr)
				lr._lastSizeRequest = null;
			textView.Text = view.Text;

			view.InvalidateMeasure();
			textView.RequestLayout();
		}

		public static void MapPropertyColor(IViewRenderer renderer, IText view)
		{
			var labelRenderer = renderer as LabelRenderer;
			if (labelRenderer == null)
				return;
			var c = view.Color;
			if (c == labelRenderer._lastUpdateColor)
				return;


			if (c.IsDefault)
				labelRenderer.TypedNativeView.SetTextColor(labelRenderer._labelTextColorDefault);
			else
				labelRenderer.TypedNativeView.SetTextColor(c.ToNative());
		}

		public static void MapPropertyLineHeight(IViewRenderer renderer, ILabel view)
		{
			var nativeLabel = renderer.NativeView as TextView;
			var labelRenderer = renderer as LabelRenderer;
			if (labelRenderer._lineSpacingExtraDefault < 0)
				labelRenderer._lineSpacingExtraDefault = nativeLabel.LineSpacingExtra;
			if (labelRenderer._lineSpacingMultiplierDefault < 0)
				labelRenderer._lineSpacingMultiplierDefault = nativeLabel.LineSpacingMultiplier;

			if (view.LineHeight == -1)
				nativeLabel.SetLineSpacing(labelRenderer._lineSpacingExtraDefault, labelRenderer._lineSpacingMultiplierDefault);
			else if (nativeLabel.LineHeight >= 0)
				nativeLabel.SetLineSpacing(0, (float)nativeLabel.LineHeight);

			labelRenderer._lastSizeRequest = null;
		}

		public override SizeRequest GetDesiredSize(double wConstraint, double hConstraint)
		{
			int widthConstraint = wConstraint == double.MaxValue ? int.MaxValue : (int)wConstraint;
			int heightConstraint = hConstraint == double.MaxValue ? int.MaxValue : (int)hConstraint;
			var hint = TypedNativeView.Hint;
			if (!string.IsNullOrEmpty(hint))
				TypedNativeView.Hint = string.Empty;

			var deviceWidthConstraint = Context.ToPixels(widthConstraint);
			var deviceHeightConstraint = Context.ToPixels(heightConstraint);

			var widthSpec = MeasureSpecMode.AtMost.MakeMeasureSpec((int)deviceWidthConstraint);
			var heightSpec = MeasureSpecMode.AtMost.MakeMeasureSpec((int)deviceHeightConstraint);

			TypedNativeView.Measure(widthSpec, heightSpec);

			var deviceIndependentSize = Context.FromPixels(TypedNativeView.MeasuredWidth, TypedNativeView.MeasuredHeight);

			var result = new SizeRequest(deviceIndependentSize, new Size());

			//Set Hint back after sizing
			TypedNativeView.Hint = hint;

			result.Minimum = new Size(Math.Min(Context.ToPixels(10), result.Request.Width), result.Request.Height);

			// if the measure of the view has changed then trigger a request for layout
			// if the measure hasn't changed then force a layout of the label
			var measureIsChanged = !_lastSizeRequest.HasValue ||
				_lastSizeRequest.HasValue && (_lastSizeRequest.Value.Request.Height != TypedNativeView.MeasuredHeight || _lastSizeRequest.Value.Request.Width != TypedNativeView.MeasuredWidth);
			if (measureIsChanged)
				TypedNativeView.RequestLayout();
			else
				TypedNativeView.ForceLayout();

			_lastConstraintWidth = (int)widthConstraint;
			_lastConstraintHeight = (int)heightConstraint;
			_lastSizeRequest = result;

			return result;
		}
	}
}
