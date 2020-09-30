using AppKit;
using CoreGraphics;

namespace Xamarin.Forms.Platform.MacOS
{
	internal static class NSTextFieldExtensions
	{
		public static NSTextField CreateLabel(string text)
		{
			var textField = new NSTextField();
			textField.StringValue = text;
			textField.DrawsBackground = false;
			textField.Editable = false;
			textField.Bezeled = false;
			textField.Selectable = false;
			textField.SizeToFit();
			textField.CenterTextVertically();
			return textField;
		}

		public static NSTextFieldCell CreateLabelCentered(string text)
		{
			var textField = new VerticallyCenteredTextFieldCell(0);
			textField.StringValue = text;
			textField.DrawsBackground = false;
			textField.Editable = false;
			textField.Bezeled = false;
			textField.Selectable = false;
			return textField;
		}

		public static void CenterTextVertically(this NSTextField self)
		{
			self.CenterTextVertically(self.Frame);
		}

		public static void CenterTextVertically(this NSTextField self, CGRect frame)
		{
			var stringHeight = self.Cell.AttributedStringValue.Size.Height;
			var titleRect = self.Cell.TitleRectForBounds(frame);
			var newTitleRect = new CGRect(titleRect.X, frame.Y + (frame.Height - stringHeight) / 2.0, titleRect.Width,
				stringHeight);
			self.Frame = newTitleRect;
		}
	}
}