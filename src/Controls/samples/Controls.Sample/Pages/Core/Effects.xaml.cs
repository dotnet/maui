using System;
using System.Diagnostics;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Platform;

namespace Maui.Controls.Sample.Pages
{
	public partial class Effects
	{
		public Effects()
		{
			InitializeComponent();
		}
	}

	public class FocusRoutingEffect : RoutingEffect
	{
		protected override void OnAttached()
		{
			base.OnAttached();
		}

		protected override void OnDetached()
		{
			base.OnDetached();
		}
	}

	public class FocusPlatformEffect : PlatformEffect
	{
		public FocusPlatformEffect() : base()
		{
		}

#if WINDOWS
		protected override void OnAttached()
		{
			try
			{
				(Control as Microsoft.UI.Xaml.Controls.Control).Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Cyan);
				(Control as MauiTextBox).BackgroundFocusBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.White);
			}
			catch (Exception ex)
			{
				Debug.WriteLine("Cannot set property on attached control. Error: ", ex.Message);
			}
		}
#endif

		protected override void OnDetached()
		{
		}
	}
}