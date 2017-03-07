using System;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms
{
	public abstract class Behavior : BindableObject, IAttachedObject
	{
		internal Behavior(Type associatedType)
		{
			if (associatedType == null)
				throw new ArgumentNullException("associatedType");
			AssociatedType = associatedType;
		}

		protected Type AssociatedType { get; }

		void IAttachedObject.AttachTo(BindableObject bindable)
		{
			if (bindable == null)
				throw new ArgumentNullException("bindable");
			if (!AssociatedType.IsInstanceOfType(bindable))
				throw new InvalidOperationException("bindable not an instance of AssociatedType");
			OnAttachedTo(bindable);
		}

		void IAttachedObject.DetachFrom(BindableObject bindable)
		{
			OnDetachingFrom(bindable);
		}

		protected virtual void OnAttachedTo(BindableObject bindable)
		{
		}

		protected virtual void OnDetachingFrom(BindableObject bindable)
		{
		}
	}

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