#nullable disable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/ResourceDictionary.xml" path="Type[@FullName='Microsoft.Maui.Controls.ResourceDictionary']/Docs/*" />
	public class ResourceDictionary : IResourceDictionary, IDictionary<string, object>
	{
		const string GetResourcePathUriScheme = "maui://";
		static ConditionalWeakTable<Type, ResourceDictionary> s_instances = new ConditionalWeakTable<Type, ResourceDictionary>();
		readonly Dictionary<string, object> _innerDictionary = new(StringComparer.Ordinal);
		ResourceDictionary _mergedInstance;
		Uri _source;

		/// <include file="../../docs/Microsoft.Maui.Controls/ResourceDictionary.xml" path="//Member[@MemberName='Source']/Docs/*" />
		[System.ComponentModel.TypeConverter(typeof(RDSourceTypeConverter))]
		public Uri Source
		{
			get { return _source; }
			set
			{
				if (_source == value)
					return;
				throw new InvalidOperationException("Source can only be set from XAML."); //through the RDSourceTypeConverter
			}
		}

		//Used by the XamlC compiled converter
		/// <include file="../../docs/Microsoft.Maui.Controls/ResourceDictionary.xml" path="//Member[@MemberName='SetAndLoadSource']/Docs/*" />
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void SetAndLoadSource(Uri value, string resourcePath, Assembly assembly, global::System.Xml.IXmlLineInfo lineInfo)
		{
			_source = value;

			//this will return a type if the RD as an x:Class element, and codebehind
			var type = XamlResourceIdAttribute.GetTypeForPath(assembly, resourcePath);
			if (type != null)
				_mergedInstance = s_instances.GetValue(type, _ => (ResourceDictionary)Activator.CreateInstance(type));
			else
				_mergedInstance = DependencyService.Get<IResourcesLoader>().CreateFromResource<ResourceDictionary>(resourcePath, assembly, lineInfo);
			OnValuesChanged(_mergedInstance.ToArray());
		}

		ObservableCollection<ResourceDictionary> _mergedDictionaries;
		/// <include file="../../docs/Microsoft.Maui.Controls/ResourceDictionary.xml" path="//Member[@MemberName='MergedDictionaries']/Docs/*" />
		public ICollection<ResourceDictionary> MergedDictionaries
		{
			get
			{
				if (_mergedDictionaries == null)
				{
					var col = new ObservableCollection<ResourceDictionary>();
					col.CollectionChanged += MergedDictionaries_CollectionChanged;
					_mergedDictionaries = col;
				}
				return _mergedDictionaries;
			}
		}

		internal IList<StyleSheets.StyleSheet> StyleSheets { get; set; }

		void StyleSheetsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					ValuesChanged?.Invoke(this, ResourcesChangedEventArgs.StyleSheets);
					break;
			}
		}
		IList<ResourceDictionary> _collectionTrack;

		void MergedDictionaries_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			// Move() isn't exposed by ICollection
			if (e.Action == NotifyCollectionChangedAction.Move)
				return;

			_collectionTrack = _collectionTrack ?? new List<ResourceDictionary>();
			// Collection has been cleared
			if (e.Action == NotifyCollectionChangedAction.Reset)
			{
				foreach (var dictionary in _collectionTrack)
					dictionary.ValuesChanged -= Item_ValuesChanged;

				_collectionTrack.Clear();
				return;
			}

			// New Items
			if (e.NewItems != null)
			{
				foreach (var item in e.NewItems)
				{
					var rd = (ResourceDictionary)item;
					_collectionTrack.Add(rd);
					rd.ValuesChanged += Item_ValuesChanged;
					OnValuesChanged(rd.ToArray());
				}
			}

			// Old Items
			if (e.OldItems != null)
			{
				foreach (var item in e.OldItems)
				{
					var rd = (ResourceDictionary)item;
					rd.ValuesChanged -= Item_ValuesChanged;
					_collectionTrack.Remove(rd);
				}
			}
		}

		void Item_ValuesChanged(object sender, ResourcesChangedEventArgs e)
		{
			OnValuesChanged(e.Values.ToArray());
		}

		void ICollection<KeyValuePair<string, object>>.Add(KeyValuePair<string, object> item)
		{
			((ICollection<KeyValuePair<string, object>>)_innerDictionary).Add(item);
			OnValuesChanged(item);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/ResourceDictionary.xml" path="//Member[@MemberName='Clear']/Docs/*" />
		public void Clear()
		{
			_innerDictionary.Clear();
		}

		bool ICollection<KeyValuePair<string, object>>.Contains(KeyValuePair<string, object> item)
		{
			return ((ICollection<KeyValuePair<string, object>>)_innerDictionary).Contains(item)
				|| (_mergedInstance != null && _mergedInstance.Contains(item));
		}

		void ICollection<KeyValuePair<string, object>>.CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
		{
			((ICollection<KeyValuePair<string, object>>)_innerDictionary).CopyTo(array, arrayIndex);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/ResourceDictionary.xml" path="//Member[@MemberName='Count']/Docs/*" />
		public int Count
		{
			get { return _innerDictionary.Count + (_mergedInstance?.Count ?? 0); }
		}

		bool ICollection<KeyValuePair<string, object>>.IsReadOnly
		{
			get { return ((ICollection<KeyValuePair<string, object>>)_innerDictionary).IsReadOnly; }
		}

		bool ICollection<KeyValuePair<string, object>>.Remove(KeyValuePair<string, object> item)
		{
			return ((ICollection<KeyValuePair<string, object>>)_innerDictionary).Remove(item);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/ResourceDictionary.xml" path="//Member[@MemberName='Add'][4]/Docs/*" />
		public void Add(string key, object value)
		{
			if (ContainsKey(key))
				throw new ArgumentException($"A resource with the key '{key}' is already present in the ResourceDictionary.");
			_innerDictionary.Add(key, value);
			OnValueChanged(key, value);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/ResourceDictionary.xml" path="//Member[@MemberName='ContainsKey']/Docs/*" />
		public bool ContainsKey(string key)
		{
			// Note that this only checks the inner dictionary and ignores the merged dictionaries. This is apparently an intended 
			// behavior to support Hot Reload. 

			return _innerDictionary.ContainsKey(key);
		}

		[IndexerName("Item")]
		public object this[string index]
		{
			get
			{
				if (_innerDictionary.ContainsKey(index))
					return _innerDictionary[index];
				if (_mergedInstance != null && _mergedInstance.ContainsKey(index))
					return _mergedInstance[index];
				if (_mergedDictionaries != null)
				{
					var dictionaries = (ObservableCollection<ResourceDictionary>)MergedDictionaries;
					for (int i = dictionaries.Count - 1; i >= 0; i--)
					{
						if (dictionaries[i].TryGetValue(index, out var value))
						{
							return value;
						}
					}
				}

				throw new KeyNotFoundException($"The resource '{index}' is not present in the dictionary.");
			}
			set
			{
				_innerDictionary[index] = value;
				OnValueChanged(index, value);
			}
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/ResourceDictionary.xml" path="//Member[@MemberName='Keys']/Docs/*" />
		public ICollection<string> Keys
		{
			get
			{
				if (_mergedInstance is null)
					return _innerDictionary.Keys;
				if (_innerDictionary.Count == 0)
					return _mergedInstance.Keys;
				return new ReadOnlyCollection<string>(_innerDictionary.Keys.Concat(_mergedInstance.Keys).ToList());
			}
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/ResourceDictionary.xml" path="//Member[@MemberName='Remove']/Docs/*" />
		public bool Remove(string key)
		{
			return _innerDictionary.Remove(key) || (_mergedInstance?.Remove(key) ?? false);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/ResourceDictionary.xml" path="//Member[@MemberName='Values']/Docs/*" />
		public ICollection<object> Values
		{
			get
			{
				if (_mergedInstance is null)
					return _innerDictionary.Values;
				if (_innerDictionary.Count == 0)
					return _mergedInstance.Values;
				return new ReadOnlyCollection<object>(_innerDictionary.Values.Concat(_mergedInstance.Values).ToList());
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/ResourceDictionary.xml" path="//Member[@MemberName='GetEnumerator']/Docs/*" />
		public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
		{
			return _innerDictionary.GetEnumerator();
		}

		internal IEnumerable<KeyValuePair<string, object>> MergedResources
		{
			get
			{
				if (_mergedDictionaries != null)
				{
					for (int i = _mergedDictionaries.Count - 1; i >= 0; i--)
					{
						ResourceDictionary r = _mergedDictionaries[i];
						foreach (var x in r.MergedResources)
							yield return x;
					}
				}

				if (_mergedInstance != null)
					foreach (var r in _mergedInstance.MergedResources)
						yield return r;
				foreach (var r in _innerDictionary)
					yield return r;
			}
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/ResourceDictionary.xml" path="//Member[@MemberName='TryGetValue']/Docs/*" />
		public bool TryGetValue(string key, out object value)
			=> TryGetValueAndSource(key, out value, out _);

		internal bool TryGetValueAndSource(string key, out object value, out ResourceDictionary source)
		{
			source = this;
			return _innerDictionary.TryGetValue(key, out value)
				|| (_mergedInstance != null && _mergedInstance.TryGetValueAndSource(key, out value, out source))
				|| (_mergedDictionaries != null && TryGetMergedDictionaryValue(key, out value, out source));
		}

		bool TryGetMergedDictionaryValue(string key, out object value, out ResourceDictionary source)
		{
			var dictionaries = (ObservableCollection<ResourceDictionary>)MergedDictionaries;
			for (int i = dictionaries.Count - 1; i >= 0; i--)
			{
				var dictionary = dictionaries[i];
				if (dictionary.TryGetValue(key, out value))
				{
					source = dictionary;
					return true;
				}
			}

			value = null;
			source = null;
			return false;
		}

		event EventHandler<ResourcesChangedEventArgs> IResourceDictionary.ValuesChanged
		{
			add { ValuesChanged += value; }
			remove { ValuesChanged -= value; }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/ResourceDictionary.xml" path="//Member[@MemberName='Add'][2]/Docs/*" />
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

		/// <include file="../../docs/Microsoft.Maui.Controls/ResourceDictionary.xml" path="//Member[@MemberName='Add'][1]/Docs/*" />
		public void Add(ResourceDictionary mergedResourceDictionary)
		{
			MergedDictionaries.Add(mergedResourceDictionary);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/ResourceDictionary.xml" path="//Member[@MemberName='Add'][3]/Docs/*" />
		public void Add(StyleSheets.StyleSheet styleSheet)
		{
			StyleSheets = StyleSheets ?? new List<StyleSheets.StyleSheet>(2);
			StyleSheets.Add(styleSheet);
			ValuesChanged?.Invoke(this, ResourcesChangedEventArgs.StyleSheets);
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

		internal void Reload()
		{
			foreach (var mr in MergedResources)
				OnValuesChanged(mr);
		}

		event EventHandler<ResourcesChangedEventArgs> ValuesChanged;

		//only used for unit testing
		internal static void ClearCache() => s_instances = new ConditionalWeakTable<Type, ResourceDictionary>();

		[Xaml.ProvideCompiled("Microsoft.Maui.Controls.XamlC.RDSourceTypeConverter")]
		public class RDSourceTypeConverter : TypeConverter, IExtendedTypeConverter
		{
			public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
				=> sourceType == typeof(string);

			public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
				=> true;

			object IExtendedTypeConverter.ConvertFromInvariantString(string value, IServiceProvider serviceProvider)
			{
				if (serviceProvider == null)
					throw new ArgumentNullException(nameof(serviceProvider));

				if (!((serviceProvider.GetService(typeof(Xaml.IProvideValueTarget)) as Xaml.IProvideValueTarget)?.TargetObject is ResourceDictionary targetRD))
					return null;

				var rootObjectType = (serviceProvider.GetService(typeof(Xaml.IRootObjectProvider)) as Xaml.IRootObjectProvider)?.RootObject.GetType();
				if (rootObjectType == null)
					return null;

				var lineInfo = (serviceProvider.GetService(typeof(Xaml.IXmlLineInfoProvider)) as Xaml.IXmlLineInfoProvider)?.XmlLineInfo;
				var rootTargetPath = XamlResourceIdAttribute.GetPathForType(rootObjectType);
				var assembly = rootObjectType.Assembly;

				if (value.IndexOf(";assembly=", StringComparison.Ordinal) != -1)
				{
					var parts = value.Split(new[] { ";assembly=" }, StringSplitOptions.RemoveEmptyEntries);
					value = parts[0];
					var asmName = parts[1];
					assembly = Assembly.Load(asmName);
				}

				var uri = new Uri(value, UriKind.Relative); //we don't want file:// uris, even if they start with '/'
				var resourcePath = GetResourcePath(uri, rootTargetPath);

				//Re-add the assembly= in all cases, so HotReload doesn't have to make assumptions
				uri = new Uri($"{value};assembly={assembly.GetName().Name}", UriKind.Relative);
				targetRD.SetAndLoadSource(uri, resourcePath, assembly, lineInfo);

				return uri;
			}

			internal static string GetResourcePath(Uri uri, string rootTargetPath)
			{
				// GetResourcePathUriScheme is a fake scheme so it's not seen as file:// uri,
				// and the forward slashes are valid on all plats
				var resourceUri = uri.OriginalString.StartsWith("/", StringComparison.Ordinal)
									 ? new Uri($"{GetResourcePathUriScheme}{uri.OriginalString}", UriKind.Absolute)
									 : new Uri($"{GetResourcePathUriScheme}/{rootTargetPath}/../{uri.OriginalString}", UriKind.Absolute);

				//drop the leading '/'
				return resourceUri.AbsolutePath.Substring(1);
			}

			public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
				=> throw new NotImplementedException();

			public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
			{
				if (value is not Uri uri)
					throw new NotSupportedException();
				return uri.ToString();
			}
		}
	}
}
