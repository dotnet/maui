using Gtk;

namespace Microsoft.Maui.Platform
{
	public class MauiToolbar : Box
	{
		internal Button BackButton { get; }

		public delegate void BackButtonClickedEventHandler(object? sender);
		public event BackButtonClickedEventHandler? BackButtonClicked;

		public MauiToolbar() : base(Orientation.Horizontal, 0)
		{
			BackButton = new Button();
			BackButton.Image = new Image(Stock.GoBack, IconSize.Button);
			// Remove button border
			BackButton.Relief = ReliefStyle.None;

			BackButton.Clicked += (sender, args) => BackButtonClicked?.Invoke(sender);

			PackStart(BackButton, false, false, 0);
			NoShowAll = true;
		}
	}
}