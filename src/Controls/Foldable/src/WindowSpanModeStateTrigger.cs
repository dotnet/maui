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
		VisualElement _visualElement;
		DualScreenInfo _info;

		/// <summary>
		/// Initializes a new instance of the <see cref="WindowSpanModeStateTrigger"/> class.
		/// </summary>
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

		/// <summary>Bindable property for <see cref="SpanMode"/>.</summary>
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
				AttachToVisualElement();
				_info?.PropertyChanged -= OnDualScreenInfoPropertyChanged;
				_info?.PropertyChanged += OnDualScreenInfoPropertyChanged;
				UpdateState();
			}
		}

		protected override void OnDetached()
		{
			base.OnDetached();

			_info?.PropertyChanged -= OnDualScreenInfoPropertyChanged;
		}

		void OnDualScreenInfoPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			UpdateState();
		}

		void UpdateState()
		{
			AttachToVisualElement();
			var spanMode = _info?.SpanMode ?? DualScreenInfo.Current.SpanMode;

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

		void AttachToVisualElement()
		{
			var visualElement = VisualState?.VisualStateGroup?.VisualElement;
			if (visualElement == null || visualElement == _visualElement)
				return;

			_info?.PropertyChanged -= OnDualScreenInfoPropertyChanged;

			_visualElement = visualElement;
			_info = new DualScreenInfo(visualElement);

			if (IsAttached)
				_info.PropertyChanged += OnDualScreenInfoPropertyChanged;
		}
	}
}
