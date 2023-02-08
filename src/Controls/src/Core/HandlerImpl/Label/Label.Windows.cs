#nullable disable
using System.Collections.Generic;
using Microsoft.Maui.Controls.Platform;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Controls
{
	public partial class Label
	{
		TextBlock _textBlock;

		private protected override void OnHandlerChangedCore()
		{
			base.OnHandlerChangedCore();

			if (Handler is not null)
			{
				if (Handler is LabelHandler labelHandler && labelHandler.PlatformView is TextBlock textBlock)
				{
					_textBlock = textBlock;
					_textBlock.SizeChanged += OnSizeChanged;
				}
			}
			else
			{
				if (_textBlock is not null)
				{
					_textBlock.SizeChanged -= OnSizeChanged;
					_textBlock = null;
				}
			}
		}

		public static void MapDetectReadingOrderFromContent(LabelHandler handler, Label label) => MapDetectReadingOrderFromContent((ILabelHandler)handler, label);
		public static void MapTextType(LabelHandler handler, Label label) => MapTextType((ILabelHandler)handler, label);
		public static void MapText(LabelHandler handler, Label label) => MapText((ILabelHandler)handler, label);

		public static void MapDetectReadingOrderFromContent(ILabelHandler handler, Label label) =>
			Platform.TextBlockExtensions.UpdateDetectReadingOrderFromContent(handler.PlatformView, label);

		public static void MapTextType(ILabelHandler handler, Label label) =>
			Platform.TextBlockExtensions.UpdateText(handler.PlatformView, label);

		public static void MapText(ILabelHandler handler, Label label) =>
			Platform.TextBlockExtensions.UpdateText(handler.PlatformView, label);

		public static void MapLineBreakMode(ILabelHandler handler, Label label) =>
			handler.PlatformView?.UpdateLineBreakMode(label.LineBreakMode);

		public static void MapMaxLines(ILabelHandler handler, Label label) =>
			handler.PlatformView?.UpdateMaxLines(label);

		void OnSizeChanged(object sender, UI.Xaml.SizeChangedEventArgs e)
		{
			if (Handler is LabelHandler labelHandler)
			{
				var platformView = labelHandler.PlatformView;
				var virtualView = labelHandler.VirtualView as Label;

				if (platformView is null || virtualView is null)
					return;

				IList<double> inlineHeights = GetInlineHeights();
				platformView.RecalculateSpanPositions(virtualView, inlineHeights);
			}
		}

		IList<double> GetInlineHeights()
		{
			IList<double> inlineHeights = new List<double>();

			if (Handler is LabelHandler labelHandler)
			{
				var platformView = labelHandler.PlatformView;
				var virtualView = labelHandler.VirtualView as Label;

				FormattedString formatted = virtualView.FormattedText;

				if (formatted is not null)
				{
					var fontManager = virtualView.RequireFontManager();
					platformView.Inlines.Clear();

					// Have to implement a measure here, otherwise inline.ContentStart and ContentEnd will be null, when used in RecalculatePositions
					platformView.Measure(new global::Windows.Foundation.Size(double.MaxValue, double.MaxValue));

					var heights = new List<double>();
					for (var i = 0; i < formatted.Spans.Count; i++)
					{
						var span = formatted.Spans[i];

						var run = span.ToRunAndColorsTuple(fontManager).Item1;
						heights.Add(platformView.FindDefaultLineHeight(run));
						platformView.Inlines.Add(run);
					}

					inlineHeights = heights;
				}
			}

			return inlineHeights;
		}
	}
}