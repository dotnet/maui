using System.ComponentModel;

namespace Xamarin.Forms.DualScreen
{
	public sealed class WindowSpanModeStateTrigger : StateTriggerBase
	{
		public WindowSpanModeStateTrigger()
		{
			UpdateState();
		}

		public TwoPaneViewMode SpanMode
		{
			get => (TwoPaneViewMode)GetValue(SpanModeProperty);
			set => SetValue(SpanModeProperty, value);
		}

		public static readonly BindableProperty SpanModeProperty =
			BindableProperty.Create(nameof(SpanMode), typeof(TwoPaneViewMode), typeof(WindowSpanModeStateTrigger), default(TwoPaneViewMode),
				propertyChanged: OnSpanModeChanged);

		static void OnSpanModeChanged(BindableObject bindable, object oldvalue, object newvalue)
		{
			((WindowSpanModeStateTrigger)bindable).UpdateState();
		}

		internal override void OnAttached()
		{
			base.OnAttached();

			if (!DesignMode.IsDesignModeEnabled)
			{
				UpdateState();
				DualScreenInfo.Current.PropertyChanged += OnDualScreenInfoPropertyChanged;
			}
		}

		internal override void OnDetached()
		{
			base.OnDetached();

			DualScreenInfo.Current.PropertyChanged -= OnDualScreenInfoPropertyChanged;
		}

		void OnDualScreenInfoPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			UpdateState();
		}

		void UpdateState()
		{
			var spanMode = DualScreenInfo.Current.SpanMode;

			switch (SpanMode)
			{
				case TwoPaneViewMode.SinglePane:
					SetActive(spanMode == TwoPaneViewMode.SinglePane);
					break;
				case TwoPaneViewMode.Tall:
					SetActive(spanMode == TwoPaneViewMode.Tall);
					break;
				case TwoPaneViewMode.Wide:
					SetActive(spanMode == TwoPaneViewMode.Wide);
					break;
			}
		}
	}
}
