#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Platform;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;

namespace Microsoft.Maui.Controls.Platform
{
	public partial class ShellToolbarItemView : Microsoft.UI.Xaml.Controls.Button
	{

		public static readonly DependencyProperty ToolbarItemProperty =
			DependencyProperty.Register("ToolbarItem", typeof(ToolbarItem), typeof(ShellToolbarItemView), new PropertyMetadata(null, OnToolbarItemChanged));

		static void OnToolbarItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((ShellToolbarItemView)d)
				.ToolbarItemChanged(e.OldValue as ToolbarItem, e.NewValue as ToolbarItem);
		}

		public ShellToolbarItemView()
		{
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
			if (oldItem != null)
				oldItem.PropertyChanged -= ToolbarItemPropertyChanged;

			// TODO MAUI
			this.SetAutomationProperties(newItem, null, defaultName: newItem?.Text);

			if (newItem != null)
				newItem.PropertyChanged += ToolbarItemPropertyChanged;

			void ToolbarItemPropertyChanged(object sender, PropertyChangedEventArgs e)
			{
				// TODO MAUI
				this.SetAutomationProperties(newItem, null, defaultName: newItem?.Text);
			}
		}
	}
}
