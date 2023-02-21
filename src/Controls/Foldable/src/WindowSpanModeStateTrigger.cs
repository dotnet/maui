using System.ComponentModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Foldable;

namespace Microsoft.Maui.Controls.Foldable
{
	/// <summary>
	/// Triggers a state change when the <see cref="Microsoft.Maui.Controls.Foldable.TwoPaneViewMode"/>
	/// of the window changes.
	/// </summary>
	public sealed class WindowSpanModeStateTrigger : StateTriggerBase
	{
		public WindowSpanModeStateTrigger()
		{
			UpdateState();
		}

		/// <summary>
		/// <see cref="Microsoft.Maui.Controls.Foldable.TwoPaneViewMode"/>
		/// which indicates the span mode to which the visual state should be applied.
		/// </summary>
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

		protected override void OnAttached()
		{
			base.OnAttached();

			if (!DesignMode.IsDesignModeEnabled)
			{
				UpdateState();
				DualScreenInfo.Current.PropertyChanged += OnDualScreenInfoPropertyChanged;
			}
		}

		protected override void OnDetached()
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
