using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
{
	public class ShellToolbarItemRenderer : Microsoft.UI.Xaml.Controls.Button
	{

		public static readonly DependencyProperty ToolbarItemProperty =
			DependencyProperty.Register("ToolbarItem", typeof(ToolbarItem), typeof(ShellToolbarItemRenderer), new PropertyMetadata(null, OnToolbarItemChanged));

		static void OnToolbarItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((ShellToolbarItemRenderer)d)
				.ToolbarItemChanged(e.OldValue as ToolbarItem, e.NewValue as ToolbarItem);
		}

		public ShellToolbarItemRenderer()
		{
			Microsoft.Maui.Controls.Shell.VerifyShellUWPFlagEnabled(nameof(ShellToolbarItemRenderer));
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

		void ToolbarItemChanged(ToolbarItem oldItem, ToolbarItem newItem)
		{
			if(oldItem != null)
				oldItem.PropertyChanged -= ToolbarItemPropertyChanged;

			this.SetAutomationProperties(newItem, defaultName: newItem?.Text);

			if (newItem != null)
				newItem.PropertyChanged += ToolbarItemPropertyChanged;

			void ToolbarItemPropertyChanged(object sender, PropertyChangedEventArgs e)
			{
				this.SetAutomationProperties(newItem, defaultName: newItem?.Text);
			}
		}
	}
}
