using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Xamarin.Forms
{
	internal class AttachedCollection<T> : ObservableCollection<T>, ICollection<T>, IAttachedObject where T : BindableObject, IAttachedObject
	{
		readonly List<WeakReference> _associatedObjects = new List<WeakReference>();

		const int CleanupTrigger = 128;
		int _cleanupThreshold = CleanupTrigger;

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
				throw new ArgumentNullException("bindable");
			OnAttachedTo(bindable);
		}

		public void DetachFrom(BindableObject bindable)
		{
			OnDetachingFrom(bindable);
		}

		protected override void ClearItems()
		{
			foreach (WeakReference weakbindable in _associatedObjects)
			{
				foreach (T item in this)
				{
					var bindable = weakbindable.Target as BindableObject;
					if (bindable == null)
						continue;
					item.DetachFrom(bindable);
				}
			}
			base.ClearItems();
		}

		protected override void InsertItem(int index, T item)
		{
			base.InsertItem(index, item);
			foreach (WeakReference weakbindable in _associatedObjects)
			{
				var bindable = weakbindable.Target as BindableObject;
				if (bindable == null)
					continue;
				item.AttachTo(bindable);
			}
		}

		protected virtual void OnAttachedTo(BindableObject bindable)
		{
			lock (_associatedObjects)
			{
				_associatedObjects.Add(new WeakReference(bindable));
				CleanUpWeakReferences();
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
				for (var i = 0; i < _associatedObjects.Count; i++)
				{
					object target = _associatedObjects[i].Target;

					if (target == null || target == bindable)
					{
						_associatedObjects.RemoveAt(i);
						i--;
					}
				}
			}
		}

		protected override void RemoveItem(int index)
		{
			T item = this[index];
			foreach (WeakReference weakbindable in _associatedObjects)
			{
				var bindable = weakbindable.Target as BindableObject;
				if (bindable == null)
					continue;
				item.DetachFrom(bindable);
			}

			base.RemoveItem(index);
		}

		protected override void SetItem(int index, T item)
		{
			T old = this[index];
			foreach (WeakReference weakbindable in _associatedObjects)
			{
				var bindable = weakbindable.Target as BindableObject;
				if (bindable == null)
					continue;
				old.DetachFrom(bindable);
			}

			base.SetItem(index, item);

			foreach (WeakReference weakbindable in _associatedObjects)
			{
				var bindable = weakbindable.Target as BindableObject;
				if (bindable == null)
					continue;
				item.AttachTo(bindable);
			}
		}

		void CleanUpWeakReferences()
		{
			if (_associatedObjects.Count < _cleanupThreshold)
			{
				return;
			}

			_associatedObjects.RemoveAll(t => t == null || !t.IsAlive);
			_cleanupThreshold = _associatedObjects.Count + CleanupTrigger;
		}
	}
}