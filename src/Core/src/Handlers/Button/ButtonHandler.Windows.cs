using System;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Handlers
{
	public partial class ButtonHandler : AbstractViewHandler<IButton, Button>
	{
		protected override Button CreateNativeView() => new Button();

		public static void MapText(ButtonHandler handler, IButton button) 
		{ 
			if(handler.TypedNativeView != null)
				handler.TypedNativeView.Content = button.Text; 
		}

		public static void MapTextColor(ButtonHandler handler, IButton button) { }
		public static void MapFont(ButtonHandler handler, IButton button) { }
		public static void MapPadding(ButtonHandler handler, IButton button) { }
	}
}