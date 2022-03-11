using System;
using System.ComponentModel;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../../docs/Microsoft.Maui.Controls/SwipeItem.xml" path="Type[@FullName='Microsoft.Maui.Controls.SwipeItem']/Docs" />
	public partial class SwipeItem : MenuItem, Maui.ISwipeItemMenuItem
	{
		Paint ISwipeItemMenuItem.Background => new SolidPaint(BackgroundColor);

		Visibility ISwipeItemMenuItem.Visibility => this.IsVisible ? Visibility.Visible : Visibility.Collapsed;

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