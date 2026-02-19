#nullable disable
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Microsoft.Maui.Controls
{
	internal class AttachedCollection<T> : ObservableCollection<T>, ICollection<T>, IAttachedObject where T : IAttachedObject
	{
		readonly WeakList<BindableObject> _associatedObjects = new();

		public AttachedCollection()
		{
		}

		public AttachedCollection(IEnumerable<T> collection) : base(collection)
		{
		}

		public AttachedCollection(IList<T> list) : base(list)
		{
		}

		public void AttachTo(BindableObject bindable)
		{
			if (bindable == null)
				throw new ArgumentNullException(nameof(bindable));
			OnAttachedTo(bindable);
		}

		public void DetachFrom(BindableObject bindable)
		{
			OnDetachingFrom(bindable);
		}

		protected override void ClearItems()
		{
			foreach (var bindable in _associatedObjects)
			{
				foreach (T item in this)
				{
					item.DetachFrom(bindable);
				}
			}
			base.ClearItems();
		}

		protected override void InsertItem(int index, T item)
		{
			base.InsertItem(index, item);
			foreach (var bindable in _associatedObjects)
			{
				item.AttachTo(bindable);
			}
		}

		protected virtual void OnAttachedTo(BindableObject bindable)
		{
			lock (_associatedObjects)
			{
				_associatedObjects.Add(bindable);
			}
			foreach (T item in this)
				item.AttachTo(bindable);
		}

		protected virtual void OnDetachingFrom(BindableObject bindable)
		{
			foreach (T item in this)
				item.DetachFrom(bindable);
			lock (_associatedObjects)
			{
				_associatedObjects.Remove(bindable);
			}
		}

		protected override void RemoveItem(int index)
		{
			T item = this[index];
			foreach (var bindable in _associatedObjects)
			{
				item.DetachFrom(bindable);
			}

			base.RemoveItem(index);
		}

		protected override void SetItem(int index, T item)
		{
			T old = this[index];
			foreach (var bindable in _associatedObjects)
			{
				old.DetachFrom(bindable);
			}

			base.SetItem(index, item);

			foreach (var bindable in _associatedObjects)
			{
				item.AttachTo(bindable);
			}
		}
	}
}