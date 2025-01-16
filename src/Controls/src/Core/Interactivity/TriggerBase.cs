#nullable disable
using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../../docs/Microsoft.Maui.Controls/TriggerBase.xml" path="Type[@FullName='Microsoft.Maui.Controls.TriggerBase']/Docs/*" />
	public abstract class TriggerBase : BindableObject, IAttachedObject
	{
		bool _isSealed;

		internal TriggerBase(Type targetType)
		{
			TargetType = targetType ?? throw new ArgumentNullException(nameof(targetType));

			EnterActions = new SealedList<TriggerAction>();
			ExitActions = new SealedList<TriggerAction>();
		}

		internal TriggerBase(Condition condition, Type targetType) : this(targetType)
		{
			Setters = new SealedList<Setter>();
			Condition = condition;
			Condition.ConditionChanged = OnConditionChanged;
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/TriggerBase.xml" path="//Member[@MemberName='EnterActions']/Docs/*" />
		public IList<TriggerAction> EnterActions { get; }

		/// <include file="../../../docs/Microsoft.Maui.Controls/TriggerBase.xml" path="//Member[@MemberName='ExitActions']/Docs/*" />
		public IList<TriggerAction> ExitActions { get; }

		/// <include file="../../../docs/Microsoft.Maui.Controls/TriggerBase.xml" path="//Member[@MemberName='IsSealed']/Docs/*" />
		public bool IsSealed
		{
			get { return _isSealed; }
			private set
			{
				if (_isSealed == value)
					return;
				if (!value)
					throw new InvalidOperationException("What is sealed cannot be unsealed.");
				_isSealed = value;
				OnSeal();
			}
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/TriggerBase.xml" path="//Member[@MemberName='TargetType']/Docs/*" />
		public Type TargetType { get; }

		internal Condition Condition { get; }

		//Setters and Condition are used by Trigger, DataTrigger and MultiTrigger
		internal IList<Setter> Setters { get; }

		void IAttachedObject.AttachTo(BindableObject bindable)
		{
			IsSealed = true;

			if (bindable == null)
				throw new ArgumentNullException(nameof(bindable));
			if (!TargetType.IsInstanceOfType(bindable))
				throw new InvalidOperationException("bindable not an instance of AssociatedType");
			OnAttachedTo(bindable);
		}

		void IAttachedObject.DetachFrom(BindableObject bindable)
		{
			if (bindable == null)
				throw new ArgumentNullException(nameof(bindable));
			OnDetachingFrom(bindable);
		}

		internal virtual void OnAttachedTo(BindableObject bindable)
		{
			if (Condition != null)
			{
				var triggerIndex = ++bindable._triggerCount;
				var manualSpecificity = (ushort)(SetterSpecificity.ManualTriggerBaseline + triggerIndex);
				var specificity = new SetterSpecificity(0, manualSpecificity, 0, 0, 0, 0, 0, 0);

				bindable._triggerSpecificity[this] = specificity;
				Condition.SetUp(bindable);
			}
		}

		internal virtual void OnDetachingFrom(BindableObject bindable)
		{
			if (Condition != null)
			{
				Condition.TearDown(bindable);
				bindable._triggerSpecificity.Remove(this);
			}
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
			if (!bindable._triggerSpecificity.TryGetValue(this, out var specificity))
			{
				// this should never happen
				return;
			}

			if (newValue)
			{
				foreach (TriggerAction action in EnterActions)
					action.DoInvoke(bindable);
				foreach (Setter setter in Setters)
					setter.Apply(bindable, specificity);
			}
			else
			{
				foreach (Setter setter in Setters)
					setter.UnApply(bindable, specificity);
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