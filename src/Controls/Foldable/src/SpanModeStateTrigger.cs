using System.ComponentModel;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Foldable;

namespace Microsoft.Maui.Controls.Foldable
{
	/// <summary>
	/// Triggers a state change when the <see cref="Microsoft.Maui.Controls.Foldable.TwoPaneViewMode"/>
	/// of the attached layout changes.
	/// </summary>
	public sealed class SpanModeStateTrigger : StateTriggerBase
	{
		VisualElement _visualElement;
		DualScreenInfo _info;
		public SpanModeStateTrigger()
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
			BindableProperty.Create(nameof(SpanMode), typeof(TwoPaneViewMode), typeof(SpanModeStateTrigger), default(TwoPaneViewMode),
				propertyChanged: OnSpanModeChanged);

		static void OnSpanModeChanged(BindableObject bindable, object oldvalue, object newvalue)
		{
			((SpanModeStateTrigger)bindable).UpdateState();
		}

		void AttachToVisualElement()
		{
			var visualElement = VisualState?.VisualStateGroup?.VisualElement;
			if (visualElement == null || visualElement == _visualElement)
			{
				return;
			}

			if (_info != null)
				_info.PropertyChanged -= OnDualScreenInfoPropertyChanged;

			_visualElement = visualElement;
			_info = new DualScreenInfo(_visualElement);
		}

		protected override void OnAttached()
		{
			base.OnAttached();

			if (!DesignMode.IsDesignModeEnabled)
			{
				AttachToVisualElement();
				UpdateState();

				if (_info != null)
					_info.PropertyChanged += OnDualScreenInfoPropertyChanged;
			}
		}

		protected override void OnDetached()
		{
			base.OnDetached();

			if (_info != null)
				_info.PropertyChanged -= OnDualScreenInfoPropertyChanged;
		}

		void OnDualScreenInfoPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			UpdateState();
		}

		void UpdateState()
		{
			if (_info == null)
				return;

			var spanMode = _info.SpanMode;

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
