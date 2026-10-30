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

				// Keep the lookup used by TryGetService in step with the list (the last registration of a service type wins):
				// it must resolve to the replacement, never to the descriptor that was just replaced.
				bool wasCurrent = _descriptorDictionary.TryGetValue(previous.ServiceType, out var current) && ReferenceEquals(current, previous);
				if (wasCurrent)
					_descriptorDictionary.Remove(previous.ServiceType);

				if (wasCurrent || !_descriptorDictionary.ContainsKey(value.ServiceType))
					_descriptorDictionary[value.ServiceType] = value;
			}
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