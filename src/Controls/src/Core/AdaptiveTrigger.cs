#nullable enable

using System;
using System.ComponentModel;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/AdaptiveTrigger.xml" path="Type[@FullName='Microsoft.Maui.Controls.AdaptiveTrigger']/Docs/*" />
	public sealed class AdaptiveTrigger : StateTriggerBase
	{
		VisualElement? _visualElement;
		Window? _window;

		/// <include file="../../docs/Microsoft.Maui.Controls/AdaptiveTrigger.xml" path="//Member[@MemberName='.ctor']/Docs/*" />
		public AdaptiveTrigger()
		{
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/AdaptiveTrigger.xml" path="//Member[@MemberName='MinWindowHeight']/Docs/*" />
		public double MinWindowHeight
		{
			get => (double)GetValue(MinWindowHeightProperty);
			set => SetValue(MinWindowHeightProperty, value);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/AdaptiveTrigger.xml" path="//Member[@MemberName='MinWindowHeightProperty']/Docs/*" />
		public static readonly BindableProperty MinWindowHeightProperty =
			BindableProperty.Create(nameof(MinWindowHeight), typeof(double), typeof(AdaptiveTrigger), -1d,
				propertyChanged: OnMinWindowDimensionChanged);

		/// <include file="../../docs/Microsoft.Maui.Controls/AdaptiveTrigger.xml" path="//Member[@MemberName='MinWindowWidth']/Docs/*" />
		public double MinWindowWidth
		{
			get => (double)GetValue(MinWindowWidthProperty);
			set => SetValue(MinWindowWidthProperty, value);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/AdaptiveTrigger.xml" path="//Member[@MemberName='MinWindowWidthProperty']/Docs/*" />
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