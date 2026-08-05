using System;
using CoreGraphics;
using Foundation;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Platform;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	public partial class EditorHandler : ViewHandler<IEditor, MauiTextView>
	{
		readonly MauiTextViewEventProxy _proxy = new();

		// Height from MAUI's last PlatformArrange call. Native layout containers can assign
		// a transient placeholder height directly to a view's Bounds via UIKit, bypassing
		// MAUI's arrange pipeline. This field is zero until MAUI explicitly arranges the view,
		// making it a reliable guard for the scrollability cap in GetDesiredSize.
		double _lastArrangedHeight;

		public override void PlatformArrange(Rect rect)
		{
			_lastArrangedHeight = rect.Height;
			base.PlatformArrange(rect);
		}

		protected override MauiTextView CreatePlatformView()
		{
			var platformEditor = new MauiTextView();
			platformEditor.AddMauiDoneAccessoryView(this);
			return platformEditor;
		}

		public override void SetVirtualView(IView view)
		{
			base.SetVirtualView(view);

			_proxy.SetVirtualView(PlatformView);
		}

		protected override void ConnectHandler(MauiTextView platformView)
		{
			_proxy.Connect(VirtualView, platformView);
		}

		protected override void DisconnectHandler(MauiTextView platformView)
		{
			_proxy.Disconnect(platformView);
		}

		public override bool NeedsContainer
		{
			get
			{
				// The layout of the Editor behaves differently on iOS 16 and earlier versions when the size or scale changes at runtime.
				// https://github.com/dotnet/maui/issues/25581 - The content height gradually increases when scaling down the Editor, indicating improper handling of sizing.
				// It appears that iOS 17.0 manages this correctly.
				// To ensure consistent behavior that matches iOS 17.0, we wrap the Editor in a container on iOS 16 and earlier versions.
				if (!OperatingSystem.IsIOSVersionAtLeast(17) && !OperatingSystem.IsMacCatalyst())
				{
					return true;
				}

				return base.NeedsContainer;
			}
		}

		public override Size GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			// Tracks whether the height constraint represents a content measurement rather
			// than a real upper bound. When true, the cap at the bottom is skipped — either
			// the base already returns the correct size (SizeThatFits substitution) or the
			// caller provided a finite constraint and the standard MAUI measure contract
			// applies (children may report DesiredSize larger than the constraint; the
			// parent decides how to arrange). When false, the substitute came from
			// Bounds.Height (a real frame bound) and the cap applies to preserve
			// scrollability after rotation (#35114).
			bool heightSubstitutedFromSizeThatFits = false;

			if (double.IsInfinity(widthConstraint) || double.IsInfinity(heightConstraint))
			{
				// If we drop an infinite value into base.GetDesiredSize for the Editor, we'll
				// get an exception; it doesn't know what do to with it. So instead we'll size
				// it to fit its current contents and use those values to replace infinite constraints

				var sizeThatFits = PlatformView.SizeThatFits(new CGSize(widthConstraint, heightConstraint));

				if (double.IsInfinity(widthConstraint))
				{
					widthConstraint = sizeThatFits.Width;
				}

				if (double.IsInfinity(heightConstraint))
				{
					// Prefer _lastArrangedHeight over PlatformView.Bounds.Height. Native containers
					// can set Bounds directly via UIKit outside MAUI's pipeline, making Bounds.Height
					// unreliable. _lastArrangedHeight is zero until MAUI explicitly arranges the view,
					// so the cap is skipped for transient placeholder frames and applied only once
					// MAUI has given the view a real frame.
					var currentHeight = _lastArrangedHeight;

					if (!PlatformView.AllowAutoGrowth
						&& currentHeight > 0
						&& PlatformView.ContentSize.Height > currentHeight)
					{
						heightConstraint = currentHeight; // real MAUI-arranged bound — cap will apply
					}
					else
					{
						heightConstraint = sizeThatFits.Height;
						heightSubstitutedFromSizeThatFits = true; // not a real bound — cap will be skipped
					}
				}
			}
			else
			{
				// Caller-provided finite constraint — not a substituted bound. Skip the cap.
				heightSubstitutedFromSizeThatFits = true;
			}

			var result = base.GetDesiredSize(widthConstraint, heightConstraint);

			// Clamp the result to the constraint only when it represents a real upper bound.
			// UITextView (a UIScrollView subclass) ignores the height in SizeThatFits and always
			// returns full content height, so clamping is needed for caller- or frame-derived bounds.
			if (!heightSubstitutedFromSizeThatFits && result.Height > heightConstraint)
			{
				return new Size(result.Width, heightConstraint);
			}

			return result;
		}

		public static void MapText(IEditorHandler handler, IEditor editor)
		{
			handler.PlatformView?.UpdateText(editor);

			// Any text update requires that we update any attributed string formatting
			MapFormatting(handler, editor);
		}

		public static void MapBackground(IEditorHandler handler, IEditor editor)
		{
			if (handler.PlatformView is not MauiTextView platformView)
				return;

			if (editor.Background is ImageSourcePaint image)
			{
				var provider = handler.GetRequiredService<IImageSourceServiceProvider>();
				platformView.UpdateBackgroundImageSourceAsync(image.ImageSource, provider)
					.FireAndForget(handler);
			}
			else if (editor.Background.IsNullOrEmpty())
			{
				platformView.RemoveBackgroundLayer();
				platformView.BackgroundColor = ColorExtensions.BackgroundColor;
			}
			else
			{
				platformView.UpdateBackground(editor);
			}
		}

		public static void MapTextColor(IEditorHandler handler, IEditor editor) =>
			handler.PlatformView?.UpdateTextColor(editor);

		public static void MapPlaceholder(IEditorHandler handler, IEditor editor)
		{
			handler.PlatformView?.UpdatePlaceholder(editor);
			handler.UpdateValue(nameof(IEditor.CharacterSpacing));
		}

		public static void MapPlaceholderColor(IEditorHandler handler, IEditor editor) =>
			handler.PlatformView?.UpdatePlaceholderColor(editor);

		public static void MapCharacterSpacing(IEditorHandler handler, IEditor editor) =>
			handler.PlatformView?.UpdateCharacterSpacing(editor);

		public static void MapMaxLength(IEditorHandler handler, IEditor editor) =>
			handler.PlatformView?.UpdateMaxLength(editor);

		public static void MapIsReadOnly(IEditorHandler handler, IEditor editor) =>
			handler.PlatformView?.UpdateIsReadOnly(editor);

		public static void MapIsTextPredictionEnabled(IEditorHandler handler, IEditor editor) =>
			handler.PlatformView?.UpdateIsTextPredictionEnabled(editor);

		public static void MapIsSpellCheckEnabled(IEditorHandler handler, IEditor editor) =>
			handler.PlatformView?.UpdateIsSpellCheckEnabled(editor);

		public static void MapFont(IEditorHandler handler, IEditor editor) =>
			handler.PlatformView?.UpdateFont(editor, handler.GetRequiredService<IFontManager>());

		public static void MapHorizontalTextAlignment(IEditorHandler handler, IEditor editor) =>
			handler.PlatformView?.UpdateHorizontalTextAlignment(editor);

		public static void MapVerticalTextAlignment(IEditorHandler handler, IEditor editor) =>
			handler.PlatformView?.UpdateVerticalTextAlignment(editor);

		public static void MapCursorPosition(IEditorHandler handler, IEditor editor) =>
			handler.PlatformView?.UpdateCursorPosition(editor);

		public static void MapSelectionLength(IEditorHandler handler, IEditor editor) =>
			handler.PlatformView?.UpdateSelectionLength(editor);

		public static void MapKeyboard(IEditorHandler handler, IEditor editor) =>
			handler.PlatformView?.UpdateKeyboard(editor);

		public static void MapFormatting(IEditorHandler handler, IEditor editor)
		{
			handler.PlatformView?.UpdateMaxLength(editor);

			// Update all of the attributed text formatting properties
			handler.PlatformView?.UpdateCharacterSpacing(editor);
		}

		public static void MapIsEnabled(IEditorHandler handler, IEditor editor) =>
			handler.PlatformView?.UpdateIsEnabled(editor);

		class MauiTextViewEventProxy
		{
			bool _set;
			WeakReference<IEditor>? _virtualView;

			IEditor? VirtualView => _virtualView is not null && _virtualView.TryGetTarget(out var v) ? v : null;

			public void Connect(IEditor virtualView, MauiTextView platformView)
			{
				_virtualView = new(virtualView);

				platformView.ShouldChangeText += OnShouldChangeText;
				platformView.Started += OnStarted;
				platformView.Ended += OnEnded;
				platformView.TextSetOrChanged += OnTextPropertySet;
			}

			public void Disconnect(MauiTextView platformView)
			{
				_virtualView = null;

				platformView.ShouldChangeText -= OnShouldChangeText;
				platformView.Started -= OnStarted;
				platformView.Ended -= OnEnded;
				platformView.TextSetOrChanged -= OnTextPropertySet;
				if (_set)
					platformView.SelectionChanged -= OnSelectionChanged;

				_set = false;
			}

			public void SetVirtualView(MauiTextView platformView)
			{
				if (!_set)
					platformView.SelectionChanged += OnSelectionChanged;
				_set = true;
			}

			void OnSelectionChanged(object? sender, EventArgs e)
			{
				if (sender is MauiTextView platformView && VirtualView is IEditor virtualView)
				{
					var cursorPosition = platformView.GetCursorPosition();
					var selectedTextLength = platformView.GetSelectedTextLength();

					if (virtualView.CursorPosition != cursorPosition)
						virtualView.CursorPosition = cursorPosition;

					if (virtualView.SelectionLength != selectedTextLength)
						virtualView.SelectionLength = selectedTextLength;
				}
			}

			bool OnShouldChangeText(UITextView textView, NSRange range, string replacementString) =>
				VirtualView?.TextWithinMaxLength(textView.Text, range, replacementString) ?? false;

			void OnStarted(object? sender, EventArgs eventArgs)
			{
				if (VirtualView is IEditor virtualView)
					virtualView.IsFocused = true;
			}

			void OnEnded(object? sender, EventArgs eventArgs)
			{
				if (VirtualView is IEditor virtualView)
				{
					virtualView.IsFocused = false;
					virtualView.Completed();
				}
			}

			void OnTextPropertySet(object? sender, EventArgs e)
			{
				if (sender is MauiTextView platformView)
				{
					VirtualView?.UpdateText(platformView.Text);
				}
			}
		}
	}
}
