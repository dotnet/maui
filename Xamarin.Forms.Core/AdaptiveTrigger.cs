using System;

namespace Xamarin.Forms
{
	public sealed class AdaptiveTrigger : StateTriggerBase
	{
		public AdaptiveTrigger()
		{
			UpdateState();
		}

		public double MinWindowHeight
		{
			get => (double)GetValue(MinWindowHeightProperty);
			set => SetValue(MinWindowHeightProperty, value);
		}

		public static readonly BindableProperty MinWindowHeightProperty =
			BindableProperty.Create(nameof(MinWindowHeight), typeof(double), typeof(AdaptiveTrigger), -1d,
				propertyChanged: OnMinWindowHeightChanged);

		static void OnMinWindowHeightChanged(BindableObject bindable, object oldvalue, object newvalue)
		{
			((AdaptiveTrigger)bindable).UpdateState();
		}

		public double MinWindowWidth
		{
			get => (double)GetValue(MinWindowWidthProperty);
			set => SetValue(MinWindowWidthProperty, value);
		}

		public static readonly BindableProperty MinWindowWidthProperty =
			BindableProperty.Create(nameof(MinWindowWidthProperty), typeof(double), typeof(AdaptiveTrigger), -1d,
				propertyChanged: OnMinWindowWidthChanged);

		static void OnMinWindowWidthChanged(BindableObject bindable, object oldvalue, object newvalue)
		{
			((AdaptiveTrigger)bindable).UpdateState();
		}

		protected override void OnAttached()
		{
			base.OnAttached();

			if (!DesignMode.IsDesignModeEnabled)
			{
				UpdateState();
				Application.Current.MainPage.SizeChanged += OnSizeChanged;
			}
		}

		protected override void OnDetached()
		{
			base.OnDetached();

			Application.Current.MainPage.SizeChanged -= OnSizeChanged;
		}

		void OnSizeChanged(object sender, EventArgs e)
		{
			UpdateState();
		}

		void UpdateState()
		{
			var scaledScreenSize = Device.Info.ScaledScreenSize;

			var w = scaledScreenSize.Width;
			var h = scaledScreenSize.Height;
			var mw = MinWindowWidth;
			var mh = MinWindowHeight;

			SetActive(w >= mw && h >= mh);
		}
	}
}