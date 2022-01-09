using System;
using System.ComponentModel;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	public partial class SwipeItem : MenuItem, Maui.ISwipeItemMenuItem
	{
		Paint IMenuElement.Background => new SolidPaint(BackgroundColor);

		IImageSource IImageSourcePart.Source => this.IconImageSource;

		bool IImageSourcePart.IsAnimationPlaying => false;

		Visibility IMenuElement.Visibility => this.IsVisible ? Visibility.Visible : Visibility.Collapsed;

		void Maui.ISwipeItem.OnInvoked()
		{
			if (Command != null && Command.CanExecute(CommandParameter))
				Command.Execute(CommandParameter);

			OnClicked();
			Invoked?.Invoke(this, EventArgs.Empty);
		}

		void IImageSourcePart.UpdateIsLoading(bool isLoading)
		{
		}
	}
}