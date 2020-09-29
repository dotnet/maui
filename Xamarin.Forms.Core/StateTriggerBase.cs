using System;

namespace Xamarin.Forms
{
	public abstract class StateTriggerBase : BindableObject
	{
		bool _isActive;
		public event EventHandler IsActiveChanged;

		public StateTriggerBase()
		{

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

		public bool IsAttached { get; private set; }

		protected void SetActive(bool active)
		{
			IsActive = active;

			VisualState?.VisualStateGroup?.UpdateStateTriggers();
		}

		protected virtual void OnAttached()
		{

		}

		protected virtual void OnDetached()
		{

		}

		internal void SendAttached()
		{
			if (IsAttached)
				return;
			OnAttached();
			IsAttached = true;
		}

		internal void SendDetached()
		{
			if (!IsAttached)
				return;
			OnDetached();
			IsAttached = false;
		}
	}
}