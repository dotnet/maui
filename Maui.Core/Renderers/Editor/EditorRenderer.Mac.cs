using AppKit;
using Foundation;

namespace System.Maui.Platform
{
	public partial class EditorRenderer : AbstractViewRenderer<IEditor, NSTextField>
	{
		const string NewLineSelector = "insertNewline";

		protected override NSTextField CreateView()
		{
			var nsTextField = new NSTextField { UsesSingleLineMode = false };
			nsTextField.Cell.Scrollable = true;
			nsTextField.Cell.Wraps = true;
			nsTextField.DoCommandBySelector = (control, textView, commandSelector) =>
			{
				var result = false;
				if (commandSelector.Name.StartsWith(NewLineSelector, StringComparison.InvariantCultureIgnoreCase))
				{
					textView.InsertText(new NSString(Environment.NewLine));
					result = true;
				}
				return result;
			};

			nsTextField.EditingEnded += OnEditingEnded;
			nsTextField.Changed += OnChanged;

			return nsTextField;
		}

		protected override void DisposeView(NSTextField nsTextField)
		{
			nsTextField.EditingEnded -= OnEditingEnded;
			nsTextField.Changed -= OnChanged;

			base.DisposeView(nsTextField);
		}

		public static void MapPropertyColor(IViewRenderer renderer, IEditor editor)
		{
			if (!(renderer.NativeView is NSTextField nsTexField))
				return;

			var textColor = editor.Color;

			nsTexField.TextColor = textColor.IsDefault ? NSColor.Black : textColor.ToNativeColor();
		}

		public static void MapPropertyPlaceholder(IViewRenderer renderer, IEditor editor)
		{
			
		}

		public static void MapPropertyPlaceholderColor(IViewRenderer renderer, IEditor editor)
		{
		
		}

		public static void MapPropertyText(IViewRenderer renderer, IEditor editor)
		{
			if (!(renderer.NativeView is NSTextField nsTexField))
				return;

			if (nsTexField.StringValue != editor.Text)
				nsTexField.StringValue = editor.Text ?? string.Empty;
		}

		public static void MapPropertyAutoSize(IViewRenderer renderer, IEditor editor)
		{
		
		}

		void OnEditingEnded(object sender, EventArgs eventArgs)
		{
			VirtualView.Completed();
		}

		void OnChanged(object sender, EventArgs e) =>
			VirtualView.Text = TypedNativeView.StringValue ?? string.Empty;
	}
}