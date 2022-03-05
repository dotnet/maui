using System;
using Microsoft.Maui.Essentials;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/AdaptiveTrigger.xml" path="Type[@FullName='Microsoft.Maui.Controls.AdaptiveTrigger']/Docs" />
	public sealed class AdaptiveTrigger : StateTriggerBase
	{
		/// <include file="../../docs/Microsoft.Maui.Controls/AdaptiveTrigger.xml" path="//Member[@MemberName='.ctor']/Docs" />
		public AdaptiveTrigger()
		{
			UpdateState();
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/AdaptiveTrigger.xml" path="//Member[@MemberName='MinWindowHeight']/Docs" />
		public double MinWindowHeight
		{
			get => (double)GetValue(MinWindowHeightProperty);
			set => SetValue(MinWindowHeightProperty, value);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/AdaptiveTrigger.xml" path="//Member[@MemberName='MinWindowHeightProperty']/Docs" />
		public static readonly BindableProperty MinWindowHeightProperty =
			BindableProperty.Create(nameof(MinWindowHeight), typeof(double), typeof(AdaptiveTrigger), -1d,
				propertyChanged: OnMinWindowHeightChanged);

		static void OnMinWindowHeightChanged(BindableObject bindable, object oldvalue, object newvalue)
		{
			((AdaptiveTrigger)bindable).UpdateState();
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/AdaptiveTrigger.xml" path="//Member[@MemberName='MinWindowWidth']/Docs" />
		public double MinWindowWidth
		{
			get => (double)GetValue(MinWindowWidthProperty);
			set => SetValue(MinWindowWidthProperty, value);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/AdaptiveTrigger.xml" path="//Member[@MemberName='MinWindowWidthProperty']/Docs" />
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
			var scaledScreenSize = DeviceDisplay.MainDisplayInfo.GetScaledScreenSize();

			var w = scaledScreenSize.Width;
			var h = scaledScreenSize.Height;
			var mw = MinWindowWidth;
			var mh = MinWindowHeight;

			SetActive(w >= mw && h >= mh);
		}
	}
}