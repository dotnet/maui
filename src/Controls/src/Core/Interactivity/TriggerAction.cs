using System;

namespace Microsoft.Maui.Controls
{
	public abstract class TriggerAction
	{
		internal TriggerAction(Type associatedType)
		{
			if (associatedType == null)
				throw new ArgumentNullException("associatedType");
			AssociatedType = associatedType;
		}

		protected Type AssociatedType { get; private set; }

		protected abstract void Invoke(object sender);

		internal virtual void DoInvoke(object sender)
		{
			Invoke(sender);
		}
	}

	/// <include file="../../../docs/Microsoft.Maui.Controls/TriggerAction.xml" path="Type[@FullName='Microsoft.Maui.Controls.TriggerAction' and position()=1]/Docs/*" />
	public abstract class TriggerAction<T> : TriggerAction where T : BindableObject
	{
		protected TriggerAction() : base(typeof(T))
		{
		}

		protected override void Invoke(object sender)
		{
			Invoke((T)sender);
		}

		protected abstract void Invoke(T sender);
	}
}
