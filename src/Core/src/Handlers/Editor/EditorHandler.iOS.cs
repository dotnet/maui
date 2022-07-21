using System;
using CoreGraphics;
using Foundation;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	public partial class EditorHandler : ViewHandler<IEditor, MauiTextView>
	{
		bool _set;

		protected override MauiTextView CreatePlatformView()
		{
			var platformEditor = new MauiTextView();

#if !MACCATALYST
			var accessoryView = new MauiDoneAccessoryView();
			accessoryView.SetDoneClicked(OnDoneClicked);
			platformEditor.InputAccessoryView = accessoryView;
#endif

			return platformEditor;
		}

#if !MACCATALYST
		static void OnDoneClicked(object sender)
		{
			if (sender is IEditorHandler handler)
			{
				handler.PlatformView.ResignFirstResponder();
				handler.VirtualView.Completed();
			}
		}
#endif

		public override void SetVirtualView(IView view)
		{
			base.SetVirtualView(view);

			if (!_set)
				PlatformView.SelectionChanged += OnSelectionChanged;

			_set = true;
		}

		protected override void ConnectHandler(MauiTextView platformView)
		{
			platformView.ShouldChangeText += OnShouldChangeText;
			platformView.Started += OnStarted;
			platformView.Ended += OnEnded;
			platformView.TextSetOrChanged += OnTextPropertySet;
		}

		protected override void DisconnectHandler(MauiTextView platformView)
		{
			platformView.ShouldChangeText -= OnShouldChangeText;
			platformView.Started -= OnStarted;
			platformView.Ended -= OnEnded;
			platformView.TextSetOrChanged -= OnTextPropertySet;

			if (_set)
				platformView.SelectionChanged -= OnSelectionChanged;

			_set = false;
		}

		public override Size GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			if (double.IsInfinity(widthConstraint) || double.IsInfinity(heightConstraint))
			{
				// If we drop an infinite value into base.GetDesiredSize for the Editor, we'll
				// get an exception; it doesn't know what do to with it. So instead we'll size
				// it to fit its current contents and use those values to replace infinite constraints

				PlatformView.SizeToFit();

				if (double.IsInfinity(widthConstraint))
				{
					widthConstraint = PlatformView.Frame.Size.Width;
				}

				if (double.IsInfinity(heightConstraint))
				{
					heightConstraint = PlatformView.Frame.Size.Height;
				}
			}

			return base.GetDesiredSize(widthConstraint, heightConstraint);
		}

		public static void MapText(IEditorHandler handler, IEditor editor)
		{
			handler.PlatformView?.UpdateText(editor);

			// Any text update requires that we update any attributed string formatting
			MapFormatting(handler, editor);
		}

		public static void MapTextColor(IEditorHandler handler, IEditor editor) =>
			handler.PlatformView?.UpdateTextColor(editor);

		public static void MapPlaceholder(IEditorHandler handler, IEditor editor) =>
			handler.PlatformView?.UpdatePlaceholder(editor);

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

		bool OnShouldChangeText(UITextView textView, NSRange range, string replacementString) =>
			VirtualView.TextWithinMaxLength(textView.Text, range, replacementString);

		void OnStarted(object? sender, EventArgs eventArgs)
		{
			if (VirtualView != null)
				VirtualView.IsFocused = true;
		}

		void OnEnded(object? sender, EventArgs eventArgs)
		{
			if (VirtualView != null)
			{
				VirtualView.IsFocused = false;

				VirtualView.Completed();
			}
		}

		void OnTextPropertySet(object? sender, EventArgs e) =>
			VirtualView.UpdateText(PlatformView.Text);

		private void OnSelectionChanged(object? sender, EventArgs e)
		{
			var cursorPostion = PlatformView.GetCursorPosition();
			var selectedTextLength = PlatformView.GetSelectedTextLength();

			if (VirtualView.CursorPosition != cursorPostion)
				VirtualView.CursorPosition = cursorPostion;

			if (VirtualView.SelectionLength != selectedTextLength)
				VirtualView.SelectionLength = selectedTextLength;
		}
	}
}
