using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace System.Maui.Core.Controls
{
	public partial class MauiProgressRing : UserControl
	{
		public static readonly DependencyProperty IsActiveProperty = DependencyProperty.Register("IsActive", typeof(bool), typeof(MauiProgressRing), new PropertyMetadata(false, new PropertyChangedCallback(IsActiveChanged)));

		Storyboard animation;

		public MauiProgressRing()
		{
			InitializeComponent();

			animation = (Storyboard)Resources["ProgressRingStoryboard"];
		}

		public bool IsActive
		{
			get
			{
				return (bool)GetValue(IsActiveProperty);
			}

			set
			{
				SetValue(IsActiveProperty, value);
			}
		}

		static void IsActiveChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
		{
			((MauiProgressRing)sender).OnIsActiveChanged(Convert.ToBoolean(e.NewValue));
		}

		void OnIsActiveChanged(bool newValue)
		{
			if (newValue)
			{
				animation.Begin();
			}
			else
			{
				animation.Stop();
			}
		}

		protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
		{
			// force the ring to the largest square which is fully visible in the control
			Ring.Width = Math.Min(ActualWidth, ActualHeight);
			Ring.Height = Math.Min(ActualWidth, ActualHeight);
			base.OnRenderSizeChanged(sizeInfo);
		}
	}
}
