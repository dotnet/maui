using System;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/StateTriggerBase.xml" path="Type[@FullName='Microsoft.Maui.Controls.StateTriggerBase']/Docs" />
	public abstract class StateTriggerBase : BindableObject
	{
		bool _isActive;
		public event EventHandler IsActiveChanged;

		/// <include file="../../docs/Microsoft.Maui.Controls/StateTriggerBase.xml" path="//Member[@MemberName='.ctor']/Docs" />
		public StateTriggerBase()
		{

		}

		/// <include file="../../docs/Microsoft.Maui.Controls/StateTriggerBase.xml" path="//Member[@MemberName='IsActive']/Docs" />
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

		/// <include file="../../docs/Microsoft.Maui.Controls/StateTriggerBase.xml" path="//Member[@MemberName='IsAttached']/Docs" />
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