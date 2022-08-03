using System;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../../docs/Microsoft.Maui.Controls/Behavior.xml" path="Type[@FullName='Microsoft.Maui.Controls.Behavior' and position()=0]/Docs" />
	public abstract class Behavior : BindableObject, IAttachedObject
	{
		protected Behavior() : this(typeof(BindableObject))
		{
		}

		internal Behavior(Type associatedType) => AssociatedType = associatedType ?? throw new ArgumentNullException(nameof(associatedType));

		protected Type AssociatedType { get; }

		void IAttachedObject.AttachTo(BindableObject bindable)
		{
			if (bindable == null)
				throw new ArgumentNullException(nameof(bindable));
			if (!AssociatedType.IsInstanceOfType(bindable))
				throw new InvalidOperationException("bindable not an instance of AssociatedType");
			OnAttachedTo(bindable);
		}

		void IAttachedObject.DetachFrom(BindableObject bindable) => OnDetachingFrom(bindable);

		protected virtual void OnAttachedTo(BindableObject bindable)
		{
		}

		protected virtual void OnDetachingFrom(BindableObject bindable)
		{
		}
	}

	/// <include file="../../../docs/Microsoft.Maui.Controls/Behavior.xml" path="Type[@FullName='Microsoft.Maui.Controls.Behavior' and position()=1]/Docs" />
	public abstract class Behavior<T> : Behavior where T : BindableObject
	{
		protected Behavior() : base(typeof(T))
		{
		}

		protected override void OnAttachedTo(BindableObject bindable)
		{
			base.OnAttachedTo(bindable);
			OnAttachedTo((T)bindable);
		}

		protected virtual void OnAttachedTo(T bindable)
		{
		}

		protected override void OnDetachingFrom(BindableObject bindable)
		{
			OnDetachingFrom((T)bindable);
			base.OnDetachingFrom(bindable);
		}

		protected virtual void OnDetachingFrom(T bindable)
		{
		}
	}
}
