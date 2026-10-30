#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Maui.Hosting.Internal
{
	class MauiServiceCollection : IMauiServiceCollection
	{
		readonly List<ServiceDescriptor> _descriptors = new List<ServiceDescriptor>();
		readonly Dictionary<Type, ServiceDescriptor> _descriptorDictionary = new Dictionary<Type, ServiceDescriptor>();

		public int Count => _descriptors.Count;

		public bool IsReadOnly => false;

		public ServiceDescriptor this[int index]
		{
			get => _descriptors[index];
			set
			{
				var previous = _descriptors[index];
				_descriptors[index] = value;

				// Keep the lookup used by TryGetService equal to "the last descriptor of that service type in the list":
				// recompute it for the type that left the slot and for the type that entered it.
				RefreshLookup(previous.ServiceType);
				if (value.ServiceType != previous.ServiceType)
					RefreshLookup(value.ServiceType);
			}
		}

		void RefreshLookup(Type serviceType)
		{
			for (var i = _descriptors.Count - 1; i >= 0; i--)
			{
				if (_descriptors[i].ServiceType == serviceType)
				{
					_descriptorDictionary[serviceType] = _descriptors[i];
					return;
				}
			}

			_descriptorDictionary.Remove(serviceType);
		}

		public void Add(ServiceDescriptor item)
		{
			if (_descriptors.Contains(item))
				return;

			_descriptors.Add(item);
			_descriptorDictionary[item.ServiceType] = item;
		}

		public void Clear()
		{
			_descriptors.Clear();
			_descriptorDictionary.Clear();
		}

		public bool Contains(ServiceDescriptor item) =>
			_descriptors.Contains(item);

		public IEnumerator<ServiceDescriptor> GetEnumerator() =>
			_descriptors.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() =>
			_descriptors.GetEnumerator();

		public bool Remove(ServiceDescriptor item)
		{
			var success = _descriptors.Remove(item);
			if (success)
				_descriptorDictionary.Remove(item.ServiceType);
			return success;
		}

		public void CopyTo(ServiceDescriptor[] array, int arrayIndex) =>
			_descriptors.CopyTo(array, arrayIndex);

		public int IndexOf(ServiceDescriptor item) => _descriptors.IndexOf(item);

		public void Insert(int index, ServiceDescriptor item)
		{
			_descriptors.Insert(index, item);
			_descriptorDictionary[item.ServiceType] = item;
		}

		public void RemoveAt(int index)
		{
			var descriptor = _descriptors[index];
			_descriptors.RemoveAt(index);
			_descriptorDictionary.Remove(descriptor.ServiceType);
		}

		public bool TryGetService(Type serviceType, out ServiceDescriptor? descriptor) =>
			_descriptorDictionary.TryGetValue(serviceType, out descriptor);
	}
}