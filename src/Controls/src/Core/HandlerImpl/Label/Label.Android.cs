using System;
using Android.Text;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls
{
	public partial class Label
	{
		MauiTextView _mauiTextView;

		private protected override void OnHandlerChangedCore()
		{
			base.OnHandlerChangedCore();

			if (Handler != null)
			{
				if (Handler is LabelHandler labelHandler && labelHandler.PlatformView is MauiTextView mauiTextView)
				{
					_mauiTextView = mauiTextView;
					_mauiTextView.LayoutChanged += OnLayoutChanged;
				}
			}
			else
			{
				if (_mauiTextView != null)
				{
					_mauiTextView.LayoutChanged -= OnLayoutChanged;
					_mauiTextView = null;
				}
			}
		}

		public static void MapTextType(LabelHandler handler, Label label) => MapText((ILabelHandler)handler, label);
		public static void MapText(LabelHandler handler, Label label) => MapText((ILabelHandler)handler, label);
		public static void MapLineBreakMode(LabelHandler handler, Label label) => MapLineBreakMode((ILabelHandler)handler, label);


		public static void MapTextType(ILabelHandler handler, Label label)
		{
			Platform.TextViewExtensions.UpdateText(handler.PlatformView, label);
		}

		public static void MapText(ILabelHandler handler, Label label)
		{
			Platform.TextViewExtensions.UpdateText(handler.PlatformView, label);
		}

		// TODO: NET7 make this public
		internal static void MapTextColor(ILabelHandler handler, Label label)
		{
			handler.PlatformView?.UpdateTextColor(label);

			if (label?.HasFormattedTextSpans ?? false)
				return;

			Platform.TextViewExtensions.UpdateText(handler.PlatformView, label);
		}

		public static void MapLineBreakMode(ILabelHandler handler, Label label)
		{
			handler.PlatformView?.UpdateLineBreakMode(label);
		}

		public static void MapMaxLines(ILabelHandler handler, Label label)
		{
			handler.PlatformView?.UpdateMaxLines(label);
		}

		void OnLayoutChanged(object sender, LayoutChangedEventArgs args)
		{
			if (Handler is LabelHandler labelHandler)
			{
				var platformView = labelHandler.PlatformView;
				var virtualView = labelHandler.VirtualView as Label;

				if (platformView == null || virtualView == null)
					return;

				SpannableString spannableString = null;

				if (virtualView.FormattedText != null)
					spannableString = virtualView.ToSpannableString();

				platformView.RecalculateSpanPositions(virtualView, spannableString, new SizeRequest(new Size(args.Right - args.Left, args.Bottom - args.Top)));
			}
		}
	}
}
