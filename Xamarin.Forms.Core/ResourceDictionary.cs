using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Linq;
using System.Reflection;

namespace Xamarin.Forms
{
	public class ResourceDictionary : IResourceDictionary, IDictionary<string, object>
	{
		static ConditionalWeakTable<Type, ResourceDictionary> s_instances = new ConditionalWeakTable<Type, ResourceDictionary>();
		readonly Dictionary<string, object> _innerDictionary = new Dictionary<string, object>();

		Type _mergedWith;
		[TypeConverter (typeof(TypeTypeConverter))]
		public Type MergedWith {
			get { return _mergedWith; }
			set { 
				if (_mergedWith == value)
					return;

				if (!typeof(ResourceDictionary).GetTypeInfo().IsAssignableFrom(value.GetTypeInfo()))
					throw new ArgumentException("MergedWith should inherit from ResourceDictionary");

				_mergedWith = value;
				if (_mergedWith == null)
					return;

				_mergedInstance = s_instances.GetValue(_mergedWith,(key) => (ResourceDictionary)Activator.CreateInstance(key));
				OnValuesChanged (_mergedInstance.ToArray());
			}
		}

		ResourceDictionary _mergedInstance;

		void ICollection<KeyValuePair<string, object>>.Add(KeyValuePair<string, object> item)
		{
			((ICollection<KeyValuePair<string, object>>)_innerDictionary).Add(item);
			OnValuesChanged(item);
		}

		public void Clear()
		{
			_innerDictionary.Clear();
		}

		bool ICollection<KeyValuePair<string, object>>.Contains(KeyValuePair<string, object> item)
		{
			return ((ICollection<KeyValuePair<string, object>>)_innerDictionary).Contains(item);
		}

		void ICollection<KeyValuePair<string, object>>.CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
		{
			((ICollection<KeyValuePair<string, object>>)_innerDictionary).CopyTo(array, arrayIndex);
		}

		public int Count
		{
			get { return _innerDictionary.Count; }
		}

		bool ICollection<KeyValuePair<string, object>>.IsReadOnly
		{
			get { return ((ICollection<KeyValuePair<string, object>>)_innerDictionary).IsReadOnly; }
		}

		bool ICollection<KeyValuePair<string, object>>.Remove(KeyValuePair<string, object> item)
		{
			return ((ICollection<KeyValuePair<string, object>>)_innerDictionary).Remove(item);
		}

		public void Add(string key, object value)
		{
			if (ContainsKey(key))
				throw new ArgumentException($"A resource with the key '{key}' is already present in the ResourceDictionary.");
			_innerDictionary.Add(key, value);
			OnValueChanged(key, value);
		}

		public bool ContainsKey(string key)
		{
			return _innerDictionary.ContainsKey(key);
		}

		[IndexerName("Item")]
		public object this[string index]
		{
			get
			{
				if (_innerDictionary.ContainsKey(index))
					return _innerDictionary[index];
				throw new KeyNotFoundException($"The resource '{index}' is not present in the dictionary.");
			}
			set
			{
				_innerDictionary[index] = value;
				OnValueChanged(index, value);
			}
		}

		public ICollection<string> Keys
		{
			get { return _innerDictionary.Keys; }
		}

		public bool Remove(string key)
		{
			return _innerDictionary.Remove(key);
		}

		public ICollection<object> Values
		{
			get { return _innerDictionary.Values; }
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
		{
			return _innerDictionary.GetEnumerator();
		}

		internal IEnumerable<KeyValuePair<string, object>> MergedResources {
			get {
				if (_mergedInstance != null)
					foreach (var r in _mergedInstance.MergedResources)
						yield return r;
				foreach (var r in _innerDictionary)
					yield return r;
			}
		}

		public bool TryGetValue(string key, out object value)
		{
			return _innerDictionary.TryGetValue(key, out value);
		}

		internal bool TryGetMergedValue(string key, out object value)
		{
			return _innerDictionary.TryGetValue(key, out value) || (_mergedInstance != null && _mergedInstance.TryGetValue(key, out value));
		}

		event EventHandler<ResourcesChangedEventArgs> IResourceDictionary.ValuesChanged
		{
			add { ValuesChanged += value; }
			remove { ValuesChanged -= value; }
		}

		public void Add(Style style)
		{
			if (string.IsNullOrEmpty(style.Class))
				Add(style.TargetType.FullName, style);
			else
			{
				IList<Style> classes;
				object outclasses;
				if (!TryGetValue(Style.StyleClassPrefix + style.Class, out outclasses) || (classes = outclasses as IList<Style>) == null)
					classes = new List<Style>();
				classes.Add(style);
				this[Style.StyleClassPrefix + style.Class] = classes;
			}
		}

		void OnValueChanged(string key, object value)
		{
			OnValuesChanged(new KeyValuePair<string, object>(key, value));
		}

		void OnValuesChanged(params KeyValuePair<string, object>[] values)
		{
			if (values == null || values.Length == 0)
				return;
			ValuesChanged?.Invoke(this, new ResourcesChangedEventArgs(values));
		}

		event EventHandler<ResourcesChangedEventArgs> ValuesChanged;
	}
}