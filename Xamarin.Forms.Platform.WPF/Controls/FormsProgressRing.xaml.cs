using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace Xamarin.Forms.Platform.WPF.Controls
{
	public partial class FormsProgressRing : UserControl
	{
		public static readonly DependencyProperty IsActiveProperty = DependencyProperty.Register("IsActive", typeof(bool), typeof(FormsProgressRing), new PropertyMetadata(false, new PropertyChangedCallback(IsActiveChanged)));

		Storyboard animation;

		public FormsProgressRing()
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
			((FormsProgressRing)sender).OnIsActiveChanged(Convert.ToBoolean(e.NewValue));
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
