using AndroidX.AppCompat.Widget;

namespace Microsoft.Maui
{
	public static class ButtonExtensions
	{
		public static void UpdateText(this AppCompatButton appCompatButton, IButton button) =>
			appCompatButton.Text = button.Text;

		public static void UpdatePadding(this AppCompatButton appCompatButton, IButton button, Thickness? defaultPadding = null)
		{
			var context = appCompatButton.Context;
			if (context == null)
				return;

			// TODO: have a way to use default padding
			//       Windows keeps the default as a base but this is also wrong.
			// var padding = defaultPadding ?? new Thickness();
			var padding = new Thickness();
			padding.Left += context.ToPixels(button.Padding.Left);
			padding.Top += context.ToPixels(button.Padding.Top);
			padding.Right += context.ToPixels(button.Padding.Right);
			padding.Bottom += context.ToPixels(button.Padding.Bottom);

			appCompatButton.SetPadding(
				(int)padding.Left,
				(int)padding.Top,
				(int)padding.Right,
				(int)padding.Bottom);
		}
	}
}