#nullable disable
using System;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// Base class for state triggers that activate visual states based on conditions.
	/// </summary>
	public abstract class StateTriggerBase : BindableObject
	{
		bool _isActive;
		public event EventHandler IsActiveChanged;

		/// <summary>
		/// Initializes a new instance of the <see cref="StateTriggerBase"/> class.
		/// </summary>
		public StateTriggerBase()
		{

		}

		/// <summary>
		/// Gets a value indicating whether this trigger is currently active.
		/// </summary>
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

		/// <summary>
		/// Gets a value indicating whether this trigger is attached to a visual state.
		/// </summary>
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