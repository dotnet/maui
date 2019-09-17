using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;

namespace Xamarin.Forms.Platform.UWP
{
	public class ShellToolbarItemRenderer : Windows.UI.Xaml.Controls.Button
	{
		public ShellToolbarItemRenderer()
		{
			Xamarin.Forms.Shell.VerifyShellUWPFlagEnabled(nameof(ShellToolbarItemRenderer));
			Click += OnClick;
		}

		void OnClick(object sender, RoutedEventArgs e)
		{
			if (ToolbarItem is IMenuItemController controller)
				controller?.Activate();
		}

		public ToolbarItem ToolbarItem
		{
			get { return (ToolbarItem)GetValue(ToolbarItemProperty); }
			set { SetValue(ToolbarItemProperty, value); }
		}

		public static readonly DependencyProperty ToolbarItemProperty =
			DependencyProperty.Register("ToolbarItem", typeof(ToolbarItem), typeof(ShellToolbarItemRenderer), new PropertyMetadata(null));
	}
}
