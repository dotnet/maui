using System;
using System.Collections;
using System.Collections.Generic;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms
{
	public abstract class TriggerBase : BindableObject, IAttachedObject
	{
		bool _isSealed;

		internal TriggerBase(Type targetType)
		{
			if (targetType == null)
				throw new ArgumentNullException("targetType");
			TargetType = targetType;

			EnterActions = new SealedList<TriggerAction>();
			ExitActions = new SealedList<TriggerAction>();
		}

		internal TriggerBase(Condition condition, Type targetType) : this(targetType)
		{
			Setters = new SealedList<Setter>();
			Condition = condition;
			Condition.ConditionChanged = OnConditionChanged;
		}

		public IList<TriggerAction> EnterActions { get; }

		public IList<TriggerAction> ExitActions { get; }

		public bool IsSealed
		{
			get { return _isSealed; }
			private set
			{
				if (_isSealed == value)
					return;
				if (!value)
					throw new InvalidOperationException("What is sealed can not be unsealed.");
				_isSealed = value;
				OnSeal();
			}
		}

		public Type TargetType { get; }

		internal Condition Condition { get; }

		//Setters and Condition are used by Trigger, DataTrigger and MultiTrigger
		internal IList<Setter> Setters { get; }

		void IAttachedObject.AttachTo(BindableObject bindable)
		{
			IsSealed = true;

			if (bindable == null)
				throw new ArgumentNullException("bindable");
			if (!TargetType.IsInstanceOfType(bindable))
				throw new InvalidOperationException("bindable not an instance of AssociatedType");
			OnAttachedTo(bindable);
		}

		void IAttachedObject.DetachFrom(BindableObject bindable)
		{
			if (bindable == null)
				throw new ArgumentNullException("bindable");
			OnDetachingFrom(bindable);
		}

		internal virtual void OnAttachedTo(BindableObject bindable)
		{
			if (Condition != null)
				Condition.SetUp(bindable);
		}

		internal virtual void OnDetachingFrom(BindableObject bindable)
		{
			if (Condition != null)
				Condition.TearDown(bindable);
		}

		internal virtual void OnSeal()
		{
			((SealedList<TriggerAction>)EnterActions).IsReadOnly = true;
			((SealedList<TriggerAction>)ExitActions).IsReadOnly = true;
			if (Setters != null)
				((SealedList<Setter>)Setters).IsReadOnly = true;
			if (Condition != null)
				Condition.IsSealed = true;
		}

		void OnConditionChanged(BindableObject bindable, bool oldValue, bool newValue)
		{
			if (newValue)
			{
				foreach (TriggerAction action in EnterActions)
					action.DoInvoke(bindable);
				foreach (Setter setter in Setters)
					setter.Apply(bindable);
			}
			else
			{
				foreach (Setter setter in Setters)
					setter.UnApply(bindable);
				foreach (TriggerAction action in ExitActions)
					action.DoInvoke(bindable);
			}
		}

		internal class SealedList<T> : IList<T>
		{
			readonly IList<T> _actual;

			bool _isReadOnly;

			public SealedList()
			{
				_actual = new List<T>();
			}

			public void Add(T item)
			{
				if (IsReadOnly)
					throw new InvalidOperationException("This list is ReadOnly");
				_actual.Add(item);
			}

			public void Clear()
			{
				if (IsReadOnly)
					throw new InvalidOperationException("This list is ReadOnly");
				_actual.Clear();
			}

			public bool Contains(T item)
			{
				return _actual.Contains(item);
			}

			public void CopyTo(T[] array, int arrayIndex)
			{
				_actual.CopyTo(array, arrayIndex);
			}

			public int Count
			{
				get { return _actual.Count; }
			}

			public bool IsReadOnly
			{
				get { return _isReadOnly; }
				set
				{
					if (_isReadOnly == value)
						return;
					if (!value)
						throw new InvalidOperationException("Can't change this back to non readonly");
					_isReadOnly = value;
				}
			}

			public bool Remove(T item)
			{
				if (IsReadOnly)
					throw new InvalidOperationException("This list is ReadOnly");
				return _actual.Remove(item);
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return ((IEnumerable)_actual).GetEnumerator();
			}

			public IEnumerator<T> GetEnumerator()
			{
				return _actual.GetEnumerator();
			}

			public int IndexOf(T item)
			{
				return _actual.IndexOf(item);
			}

			public void Insert(int index, T item)
			{
				if (IsReadOnly)
					throw new InvalidOperationException("This list is ReadOnly");
				_actual.Insert(index, item);
			}

			public T this[int index]
			{
				get { return _actual[index]; }
				set
				{
					if (IsReadOnly)
						throw new InvalidOperationException("This list is ReadOnly");
					_actual[index] = value;
				}
			}

			public void RemoveAt(int index)
			{
				if (IsReadOnly)
					throw new InvalidOperationException("This list is ReadOnly");
				_actual.RemoveAt(index);
			}
		}
	}
}