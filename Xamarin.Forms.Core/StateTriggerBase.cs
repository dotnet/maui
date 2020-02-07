using System;

namespace Xamarin.Forms
{
	public abstract class StateTriggerBase : BindableObject
	{
		bool _isActive;
		public event EventHandler IsActiveChanged;

		public StateTriggerBase()
		{
			ExperimentalFlags.VerifyFlagEnabled(nameof(IndicatorView), ExperimentalFlags.StateTriggersExperimental);
		}

		public bool IsActive
		{
			get => _isActive;
			private set
			{
				if (_isActive == value)
					return;

				_isActive = value;
				IsActiveChanged?.Invoke(this, EventArgs.Empty);
			}
		}

		internal VisualState VisualState { get; set; }

		protected void SetActive(bool active)
		{
			IsActive = active;

			VisualState?.VisualStateGroup?.UpdateStateTriggers();
		}

		internal virtual void OnAttached()
		{

		}

		internal virtual void OnDetached()
		{

		}
	}
}