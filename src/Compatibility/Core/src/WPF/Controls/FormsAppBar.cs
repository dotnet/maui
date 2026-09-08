using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Microsoft.Maui.Controls.Compatibility.Platform.WPF.Controls
{
	[TemplatePart(Name = "PART_More", Type = typeof(ToggleButton))]
	[TemplatePart(Name = "PART_Command", Type = typeof(ItemsControl))]
	public class FormsAppBar : ContentControl
	{
		ToggleButton btnMore;

		public static readonly DependencyProperty PrimaryCommandsProperty = DependencyProperty.Register("PrimaryCommands", typeof(IEnumerable<FrameworkElement>), typeof(FormsAppBar), new PropertyMetadata(new List<FrameworkElement>()));
		public static readonly DependencyProperty SecondaryCommandsProperty = DependencyProperty.Register("SecondaryCommands", typeof(IEnumerable<FrameworkElement>), typeof(FormsAppBar), new PropertyMetadata(new List<FrameworkElement>()));

		public IEnumerable<FrameworkElement> PrimaryCommands
		{
			get { return (IEnumerable<FrameworkElement>)GetValue(PrimaryCommandsProperty); }
			set { SetValue(PrimaryCommandsProperty, value); }
		}

		public IEnumerable<FrameworkElement> SecondaryCommands
		{
			get { return (IEnumerable<FrameworkElement>)GetValue(SecondaryCommandsProperty); }
			set { SetValue(SecondaryCommandsProperty, value); }
		}

		public FormsAppBar()
		{
			this.DefaultStyleKey = typeof(FormsAppBar);
		}

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();
			btnMore = Template.FindName("PART_More", this) as ToggleButton;
		}

		public void Reset()
		{
			if (btnMore != null)
			{
				btnMore.IsChecked = false;
			}
		}
	}
}
