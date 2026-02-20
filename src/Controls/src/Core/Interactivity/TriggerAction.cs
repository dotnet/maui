#nullable disable
using System;

namespace Microsoft.Maui.Controls
{
	public abstract class TriggerAction
	{
		internal TriggerAction(Type associatedType)
		{
			if (associatedType == null)
				throw new ArgumentNullException(nameof(associatedType));
			AssociatedType = associatedType;
		}

		protected Type AssociatedType { get; private set; }

		protected abstract void Invoke(object sender);

		internal virtual void DoInvoke(object sender)
		{
			Invoke(sender);
		}
	}

	/// <summary>
	/// A base class for user-defined actions that respond to a trigger condition with a type-safe sender parameter.
	/// </summary>
	/// <typeparam name="T">The type of <see cref="BindableObject" /> to which this action can be attached.</typeparam>
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
