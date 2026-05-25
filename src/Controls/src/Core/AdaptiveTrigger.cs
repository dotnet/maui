using System;
using System.ComponentModel;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// A state trigger that activates when the window meets a minimum width and/or height threshold.
	/// </summary>
	public sealed class AdaptiveTrigger : StateTriggerBase
	{
		VisualElement? _visualElement;
		Window? _window;

		/// <summary>
		/// Initializes a new instance of the <see cref="AdaptiveTrigger"/> class.
		/// </summary>
		public AdaptiveTrigger()
		{
		}

		/// <summary>
		/// Gets or sets the minimum window height required for this trigger to activate. This is a bindable property.
		/// </summary>
		public double MinWindowHeight
		{
			get => (double)GetValue(MinWindowHeightProperty);
			set => SetValue(MinWindowHeightProperty, value);
		}

		/// <summary>Bindable property for <see cref="MinWindowHeight"/>.</summary>
		public static readonly BindableProperty MinWindowHeightProperty =
			BindableProperty.Create(nameof(MinWindowHeight), typeof(double), typeof(AdaptiveTrigger), -1d,
				propertyChanged: OnMinWindowDimensionChanged);

		/// <summary>
		/// Gets or sets the minimum window width required for this trigger to activate. This is a bindable property.
		/// </summary>
		public double MinWindowWidth
		{
			get => (double)GetValue(MinWindowWidthProperty);
			set => SetValue(MinWindowWidthProperty, value);
		}

		/// <summary>Bindable property for <see cref="MinWindowWidth"/>.</summary>
		public static readonly BindableProperty MinWindowWidthProperty =
			BindableProperty.Create(nameof(MinWindowWidth), typeof(double), typeof(AdaptiveTrigger), -1d,
				propertyChanged: OnMinWindowDimensionChanged);

		static void OnMinWindowDimensionChanged(BindableObject bindable, object oldvalue, object newvalue) =>
			((AdaptiveTrigger)bindable).UpdateState();

		protected override void OnAttached()
		{
			base.OnAttached();

			AttachEvents();

			UpdateState(true);
		}

		protected override void OnDetached()
		{
			base.OnDetached();

			DetachEvents();
		}

		void AttachEvents()
		{
			DetachEvents();

			_visualElement = VisualState?.VisualStateGroup?.VisualElement;

			_window = _visualElement?.Window;
			if (_window is not null)
			{
				_window.SizeChanged += OnWindowSizeChanged;
			}
		}

		void DetachEvents()
		{
			_visualElement = null;

			if (_window is not null)
			{
				_window.SizeChanged -= OnWindowSizeChanged;
			}
			_window = null;
		}

		void OnWindowSizeChanged(object? sender, EventArgs e)
		{
			UpdateState();
		}

		void UpdateState(bool knownAttached = false)
		{
			if (!knownAttached && !IsAttached)
				return;

			var w = _window?.Width ?? -1;
			var h = _window?.Height ?? -1;

			if (w == -1 || h == -1)
				return;

			var mw = MinWindowWidth;
			var mh = MinWindowHeight;

			SetActive(w >= mw && h >= mh);
		}
	}
}