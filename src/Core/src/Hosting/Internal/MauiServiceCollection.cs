#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Maui.Hosting.Internal
{
	class MauiServiceCollection : IMauiServiceCollection
	{
		readonly List<ServiceDescriptor> _descriptors = new List<ServiceDescriptor>();
		readonly Dictionary<Type, ServiceDescriptor> _descriptorDictionary = new Dictionary<Type, ServiceDescriptor>();
		internal long Version => Interlocked.Read(ref _version);
		long _version;

		public int Count => _descriptors.Count;

		public bool IsReadOnly => false;

		public ServiceDescriptor this[int index]
		{
			get => _descriptors[index];
			set
			{
				var oldServiceType = _descriptors[index].ServiceType;
				_descriptors[index] = value;
				RefreshDescriptor(oldServiceType);
				RefreshDescriptor(value.ServiceType);
				Interlocked.Increment(ref _version);
			}
		}

		public void Add(ServiceDescriptor item)
		{
			if (_descriptors.Contains(item))
				return;

			_descriptors.Add(item);
			_descriptorDictionary[item.ServiceType] = item;
			Interlocked.Increment(ref _version);
		}

		public void Clear()
		{
			_descriptors.Clear();
			_descriptorDictionary.Clear();
			Interlocked.Increment(ref _version);
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
			{
				RefreshDescriptor(item.ServiceType);
				Interlocked.Increment(ref _version);
			}
			return success;
		}

		public void CopyTo(ServiceDescriptor[] array, int arrayIndex) =>
			_descriptors.CopyTo(array, arrayIndex);

		public int IndexOf(ServiceDescriptor item) => _descriptors.IndexOf(item);

		public void Insert(int index, ServiceDescriptor item)
		{
			_descriptors.Insert(index, item);
			RefreshDescriptor(item.ServiceType);
			Interlocked.Increment(ref _version);
		}

		public void RemoveAt(int index)
		{
			var descriptor = _descriptors[index];
			_descriptors.RemoveAt(index);
			RefreshDescriptor(descriptor.ServiceType);
			Interlocked.Increment(ref _version);
		}

		void RefreshDescriptor(Type serviceType)
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

		public bool TryGetService(Type serviceType, out ServiceDescriptor? descriptor) =>
			_descriptorDictionary.TryGetValue(serviceType, out descriptor);
	}
}