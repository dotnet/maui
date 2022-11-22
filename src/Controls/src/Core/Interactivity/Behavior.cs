using System;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// Base class for generalized user-defined behaviors that can respond to arbitrary conditions and events.
	/// </summary>
	/// <remarks>Application developers should specialize the <see cref="Behavior{T}" /> generic class, instead of directly using <see cref="Behavior" />.</remarks>
	public abstract class Behavior : BindableObject, IAttachedObject
	{
		/// <summary>
		/// Creates a new <see cref="Behavior" /> with default values.
		/// </summary>
		protected Behavior() : this(typeof(BindableObject))
		{
		}

		internal Behavior(Type associatedType) => AssociatedType = associatedType ?? throw new ArgumentNullException(nameof(associatedType));

		/// <summary>
		/// Gets the type of the objects with which this <see cref="Behavior" /> can be associated.
		/// </summary>
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

		/// <summary>
		/// Application developers override this method to implement the behaviors that will be associated with <paramref name="bindable" />.
		/// </summary>
		/// <param name="bindable">The bindable object to which the behavior was attached.</param>
		protected virtual void OnAttachedTo(BindableObject bindable)
		{
		}

		/// <summary>
		/// Application developers override this method to remove the behaviors from <paramref name="bindable" />
		/// that were implemented in a previous call to the <see cref="OnAttachedTo(BindableObject)"/> method.
		/// </summary>
		/// <param name="bindable">The bindable object from which the behavior was detached.</param>
		protected virtual void OnDetachingFrom(BindableObject bindable)
		{
		}
	}

	/// <inheritdoc/>
	/// <typeparam name="T">The type of object this behavior will be applied to.</typeparam>
	public abstract class Behavior<T> : Behavior where T : BindableObject
	{
		/// <inheritdoc/>
		protected Behavior() : base(typeof(T))
		{
		}

		/// <inheritdoc/>
		protected override void OnAttachedTo(BindableObject bindable)
		{
			base.OnAttachedTo(bindable);
			OnAttachedTo((T)bindable);
		}

		/// <summary>
		/// Application developers override this method to implement the behaviors that will be associated with <paramref name="bindable" />.
		/// </summary>
		/// <param name="bindable">The bindable object to which the behavior was attached.</param>
		protected virtual void OnAttachedTo(T bindable)
		{
		}

		/// <inheritdoc/>
		protected override void OnDetachingFrom(BindableObject bindable)
		{
			OnDetachingFrom((T)bindable);
			base.OnDetachingFrom(bindable);
		}

		/// <summary>
		/// Application developers override this method to remove the behaviors from <paramref name="bindable" />
		/// that were implemented in a previous call to the <see cref="OnAttachedTo(T)"/> method.
		/// </summary>
		/// <param name="bindable">The bindable object from which the behavior was detached.</param>
		protected virtual void OnDetachingFrom(T bindable)
		{
		}
	}
}
