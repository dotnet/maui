using System.Maui.Core.Controls;
using CoreGraphics;
using Foundation;
using UIKit;

namespace System.Maui.Platform
{
	public partial class EditorRenderer : AbstractViewRenderer<IEditor, MauiEditor>
	{
		// Using same placeholder color as for the Entry
		readonly UIColor _defaultPlaceholderColor = ColorExtensions.SeventyPercentGrey;

		UILabel _placeholderLabel;

		protected override MauiEditor CreateView()
		{
			var mauiEditor = new MauiEditor(CGRect.Empty);

			CreatePlaceholderLabel(mauiEditor);

			mauiEditor.Ended += OnEnded;
			mauiEditor.Changed += OnChanged;

			return mauiEditor;
		}

		protected override void DisposeView(MauiEditor mauiEditor)
		{
			mauiEditor.FrameChanged -= OnFrameChanged;
			mauiEditor.Ended -= OnEnded;
			mauiEditor.Changed -= OnChanged;

			base.DisposeView(mauiEditor);
		}

		public static void MapPropertyColor(IViewRenderer renderer, IEditor editor)
		{
			if (!(renderer.NativeView is MauiEditor mauiEditor))
				return;

			var textColor = editor.Color;

			if (textColor.IsDefault)
				mauiEditor.TextColor = UIColor.Black;
			else
				mauiEditor.TextColor = textColor.ToNativeColor();
		}

		public static void MapPropertyPlaceholder(IViewRenderer renderer, IEditor editor)
		{
			if (!(renderer is EditorRenderer editorRenderer))
				return;

			editorRenderer._placeholderLabel.Text = editor.Placeholder;
		}

		public static void MapPropertyPlaceholderColor(IViewRenderer renderer, IEditor editor)
		{
			if (!(renderer is EditorRenderer editorRenderer))
				return;

			Color placeholderColor = editor.PlaceholderColor;

			if (placeholderColor.IsDefault)
				editorRenderer._placeholderLabel.TextColor = editorRenderer._defaultPlaceholderColor;
			else
				editorRenderer._placeholderLabel.TextColor = placeholderColor.ToNativeColor();
		}

		public static void MapPropertyText(IViewRenderer renderer, IEditor editor)
		{
			if (!(renderer is EditorRenderer editorRenderer) || !(renderer.NativeView is MauiEditor mauiEditor))
				return;

			if (mauiEditor.Text != editor.Text)
				mauiEditor.Text = editor.Text;

			editorRenderer._placeholderLabel.Hidden = !string.IsNullOrEmpty(editor.Text);
		}

		public static void MapPropertyMaxLenght(IViewRenderer renderer, IEditor editor)
		{
			if (!(renderer.NativeView is MauiEditor mauiEditor))
				return;

			var currentControlText = editor.Text;

			if (currentControlText.Length > editor.MaxLength)
				mauiEditor.Text = currentControlText.Substring(0, editor.MaxLength);
		}

		public static void MapPropertyAutoSize(IViewRenderer renderer, IEditor editor)
		{
			if (!(renderer is EditorRenderer editorRenderer) || !(renderer.NativeView is MauiEditor mauiEditor))
				return;

			mauiEditor.FrameChanged -= editorRenderer.OnFrameChanged;
			if (editor.AutoSize == EditorAutoSizeOption.TextChanges)
				mauiEditor.FrameChanged += editorRenderer.OnFrameChanged;
		}

		void CreatePlaceholderLabel(MauiEditor control)
		{
			if (control == null)
			{
				return;
			}

			_placeholderLabel = new UILabel
			{
				BackgroundColor = UIColor.Clear
			};

			control.AddSubview(_placeholderLabel);

			var edgeInsets = control.TextContainerInset;
			var lineFragmentPadding = control.TextContainer.LineFragmentPadding;

			var vConstraints = NSLayoutConstraint.FromVisualFormat(
				"V:|-" + edgeInsets.Top + "-[_placeholderLabel]-" + edgeInsets.Bottom + "-|", 0, new NSDictionary(),
				NSDictionary.FromObjectsAndKeys(
					new NSObject[] { _placeholderLabel }, new NSObject[] { new NSString("_placeholderLabel") })
			);

			var hConstraints = NSLayoutConstraint.FromVisualFormat(
				"H:|-" + lineFragmentPadding + "-[_placeholderLabel]-" + lineFragmentPadding + "-|",
				0, new NSDictionary(),
				NSDictionary.FromObjectsAndKeys(
					new NSObject[] { _placeholderLabel }, new NSObject[] { new NSString("_placeholderLabel") })
			);

			_placeholderLabel.TranslatesAutoresizingMaskIntoConstraints = false;

			control.AddConstraints(hConstraints);
			control.AddConstraints(vConstraints);
		}

		void OnFrameChanged(object sender, EventArgs e)
		{
			// When a new line is added to the UITextView the resize happens after the view has already scrolled
			// This causes the view to reposition without the scroll. If TextChanges is enabled then the Frame
			// will resize until it can't anymore and thus it should never be scrolled until the Frame can't increase in size
			if (VirtualView.AutoSize == EditorAutoSizeOption.TextChanges)
			{
				TypedNativeView.ScrollRangeToVisible(new NSRange(0, 0));
			}
		}

		void OnEnded(object sender, EventArgs eventArgs)
		{
			VirtualView.Completed();
		}

		void OnChanged(object sender, EventArgs e) =>
			VirtualView.Text = TypedNativeView.Text ?? string.Empty;
	}
}