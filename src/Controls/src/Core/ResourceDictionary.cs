#nullable disable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls
{
	/// <summary>A dictionary that maps identifier strings to arbitrary resource objects.</summary>
	public class ResourceDictionary : IResourceDictionary, IDictionary<string, object>
	{
		const string GetResourcePathUriScheme = "maui://";
		static ConditionalWeakTable<Type, ResourceDictionary> s_instances = new ConditionalWeakTable<Type, ResourceDictionary>();
		readonly Dictionary<string, object> _innerDictionary = new(StringComparer.Ordinal);
		ResourceDictionary _mergedInstance;
		Uri _source;

		// This action is instantiated in a module initializer in ResourceDictionaryHotReloadHelper
		internal static Action<ResourceDictionary, Uri, string, Assembly, System.Xml.IXmlLineInfo> s_setAndLoadSource;

		/// <summary>Gets or sets the URI of the merged resource dictionary.</summary>
		[TypeConverter(typeof(RDSourceTypeConverter))]
		public Uri Source
		{
			get { return _source; }
			set
			{
				if (_source == value)
					return;
				throw new InvalidOperationException("Source can only be set from XAML."); // through SetSource
			}
		}

		//Used by the XamlC compiled converter
		/// <summary>For internal use by the MAUI platform.</summary>
		/// <typeparam name="T">The type of resource dictionary to create.</typeparam>
		/// <param name="value">The source URI.</param>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void SetAndCreateSource<T>(Uri value)
			where T : ResourceDictionary, new()
		{
			var instance = s_instances.GetValue(typeof(T), static _ =>
			{
				try
				{
					return new T();
				}
				catch (TargetInvocationException tie) when (tie.InnerException is not null)
				{
					ExceptionDispatchInfo.Capture(tie.InnerException).Throw();
					throw;
				}
			});

			SetSource(value, instance);
		}

		// Used by hot reload
		/// <summary>For internal use by the MAUI platform.</summary>
		/// <param name="value">The source URI.</param>
		/// <param name="resourcePath">The resource path.</param>
		/// <param name="assembly">The assembly containing the resource.</param>
		/// <param name="lineInfo">The XML line info for error reporting.</param>
		[EditorBrowsable(EditorBrowsableState.Never)]
		[RequiresUnreferencedCode(TrimmerConstants.XamlRuntimeParsingNotSupportedWarning)]
		public void SetAndLoadSource(Uri value, string resourcePath, Assembly assembly, global::System.Xml.IXmlLineInfo lineInfo)
		{
			if (s_setAndLoadSource is null)
			{
				throw new InvalidOperationException("ResourceDictionary.SetAndLoadSource was not initialized");
			}

			s_setAndLoadSource(this, value, resourcePath, assembly, lineInfo);
		}

		internal static ResourceDictionary GetOrCreateInstance([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] Type type)
		{
			return s_instances.GetValue(type, _ =>
			{
				try
				{
					return (ResourceDictionary)Activator.CreateInstance(type);
				}
				catch (TargetInvocationException tie) when (tie.InnerException is not null)
				{
					ExceptionDispatchInfo.Capture(tie.InnerException).Throw();
					throw;
				}
			});
		}

		internal void SetSource(Uri source, ResourceDictionary sourceInstance)
		{
			_source = source;
			_mergedInstance = sourceInstance;
			OnKeysChanged(_mergedInstance.MergedResourcesKeys);
		}

		ObservableCollection<ResourceDictionary> _mergedDictionaries;
		/// <summary>Gets the collection of dictionaries that were merged into this dictionary.</summary>
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
					OnKeysChanged(rd.MergedResourcesKeys);
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
			OnKeysChanged(e.Keys);
		}

		void ICollection<KeyValuePair<string, object>>.Add(KeyValuePair<string, object> item)
		{
			((ICollection<KeyValuePair<string, object>>)_innerDictionary).Add(item);
			OnKeysChanged(new[] { item.Key });
		}

		/// <summary>Removes all items from the dictionary.</summary>
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

		/// <summary>Gets the number of entries in the dictionary.</summary>
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

		/// <summary>Adds a key-value pair to the dictionary.</summary>
		/// <param name="key">The identifier for the resource.</param>
		/// <param name="value">The resource object to store.</param>
		public void Add(string key, object value)
		{
			if (ContainsKey(key))
				throw new ArgumentException($"A resource with the key '{key}' is already present in the ResourceDictionary.");
			_innerDictionary.Add(key, value);
			OnValueChanged(key, value);
		}

		/// <summary>Returns whether the dictionary contains a key-value pair identified by the specified key.</summary>
		/// <param name="key">The identifier to search for.</param>
		/// <returns><see langword="true"/> if the key exists in the immediate dictionary; otherwise, <see langword="false"/>.</returns>
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
				if (_innerDictionary.TryGetValue(index, out var value))
					return ResolveValue(value);
				if (_mergedInstance != null && _mergedInstance.ContainsKey(index))
					return _mergedInstance[index];
				if (_mergedDictionaries != null)
				{
					var dictionaries = (ObservableCollection<ResourceDictionary>)MergedDictionaries;
					for (int i = dictionaries.Count - 1; i >= 0; i--)
					{
						if (dictionaries[i].TryGetValue(index, out value))
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

		/// <summary>Gets the collection of identifier strings that are keys in the dictionary.</summary>
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

		/// <summary>Removes the key-value pair identified by the specified key.</summary>
		/// <param name="key">The identifier of the item to remove.</param>
		/// <returns><see langword="true"/> if the key existed and was removed; otherwise, <see langword="false"/>.</returns>
		public bool Remove(string key)
		{
			return _innerDictionary.Remove(key) || (_mergedInstance?.Remove(key) ?? false);
		}

		/// <summary>Gets the collection of values stored in the dictionary.</summary>
		public ICollection<object> Values
		{
			get
			{
				// Resolve lazy values when enumerating
				var resolvedValues = _innerDictionary.Values.Select(ResolveValue);
				if (_mergedInstance is null)
					return new ReadOnlyCollection<object>(resolvedValues.ToList());
				if (_innerDictionary.Count == 0)
					return _mergedInstance.Values;
				return new ReadOnlyCollection<object>(resolvedValues.Concat(_mergedInstance.Values).ToList());
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		/// <summary>Returns an enumerator that iterates through the dictionary's key-value pairs.</summary>
		/// <returns>An enumerator for the dictionary.</returns>
		public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
		{
			// Resolve lazy values when enumerating
			foreach (var kvp in _innerDictionary)
				yield return new KeyValuePair<string, object>(kvp.Key, ResolveValue(kvp.Value));
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
				// Resolve lazy values when enumerating
				foreach (var r in _innerDictionary)
					yield return new KeyValuePair<string, object>(r.Key, ResolveValue(r.Value));
			}
		}

		/// <summary>
		/// Enumerates all resource keys without resolving lazy values.
		/// Used for resource change propagation where values are looked up on-demand.
		/// </summary>
		internal IEnumerable<string> MergedResourcesKeys
		{
			get
			{
				if (_mergedDictionaries != null)
				{
					for (int i = _mergedDictionaries.Count - 1; i >= 0; i--)
					{
						ResourceDictionary r = _mergedDictionaries[i];
						foreach (var key in r.MergedResourcesKeys)
							yield return key;
					}
				}

				if (_mergedInstance != null)
					foreach (var key in _mergedInstance.MergedResourcesKeys)
						yield return key;
				
				foreach (var key in _innerDictionary.Keys)
					yield return key;
			}
		}

		/// <summary>Attempts to get the value associated with the specified key.</summary>
		/// <param name="key">The key to search for.</param>
		/// <param name="value">When this method returns, contains the value if found; otherwise, <see langword="null"/>.</param>
		/// <returns><see langword="true"/> if the key was found; otherwise, <see langword="false"/>.</returns>
		public bool TryGetValue(string key, out object value)
			=> TryGetValueAndSource(key, out value, out _);

		internal bool TryGetValueAndSource(string key, out object value, out ResourceDictionary source)
		{
			source = this;
			if (_innerDictionary.TryGetValue(key, out value))
			{
				value = ResolveValue(value);
				return true;
			}
			if (_mergedInstance != null && _mergedInstance.TryGetValueAndSource(key, out value, out source))
				return true;
			if (_mergedDictionaries != null && TryGetMergedDictionaryValue(key, out value, out source))
				return true;
			return false;
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

		/// <summary>Adds an implicit style to the dictionary.</summary>
		/// <param name="style">The style to add.</param>
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

		/// <summary>Adds a resource dictionary to the merged dictionaries collection.</summary>
		/// <param name="mergedResourceDictionary">The dictionary to merge.</param>
		public void Add(ResourceDictionary mergedResourceDictionary)
		{
			MergedDictionaries.Add(mergedResourceDictionary);
		}

		/// <summary>Adds a style sheet to the dictionary.</summary>
		/// <param name="styleSheet">The style sheet to add.</param>
		public void Add(StyleSheets.StyleSheet styleSheet)
		{
			StyleSheets = StyleSheets ?? new List<StyleSheets.StyleSheet>(2);
			StyleSheets.Add(styleSheet);
			ValuesChanged?.Invoke(this, ResourcesChangedEventArgs.StyleSheets);
		}

		void OnValueChanged(string key, object value)
		{
			OnKeysChanged(new[] { key });
		}

		void OnKeysChanged(IEnumerable<string> keys)
		{
			if (keys == null)
				return;
			ValuesChanged?.Invoke(this, new ResourcesChangedEventArgs(keys, ResolveKey));
		}

		object ResolveKey(string key) => TryGetValue(key, out var value) ? value : null;

		internal void Reload()
		{
			OnKeysChanged(MergedResourcesKeys);
		}

		event EventHandler<ResourcesChangedEventArgs> ValuesChanged;

		//only used for unit testing
		internal static void ClearCache() => s_instances = new ConditionalWeakTable<Type, ResourceDictionary>();

		/// <summary>
		/// Adds a resource factory to the dictionary. The factory is invoked lazily when the resource is first accessed.
		/// </summary>
		/// <param name="key">The resource key.</param>
		/// <param name="factory">A factory function that creates the resource.</param>
		/// <param name="shared">If true (default), the factory result is cached. If false, the factory is invoked on each access.</param>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void AddFactory(string key, Func<object> factory, bool shared = true)
		{
			if (ContainsKey(key))
				throw new ArgumentException($"A resource with the key '{key}' is already present in the ResourceDictionary.");
			var lazyResource = new LazyResource(factory, shared);
			_innerDictionary.Add(key, lazyResource);
			// Note: We don't fire OnValueChanged here since the value hasn't been resolved yet.
			// The event will contain the LazyResource wrapper, which is internal behavior.
			OnValueChanged(key, lazyResource);
		}

		/// <summary>
		/// Adds an implicit style factory to the dictionary. The factory is invoked lazily when the style is first accessed.
		/// </summary>
		/// <param name="targetType">The target type for the implicit style. The type's full name is used as the key.</param>
		/// <param name="factory">A factory function that creates the style.</param>
		/// <param name="shared">If true (default), the factory result is cached. If false, the factory is invoked on each access.</param>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void AddFactory(Type targetType, Func<Style> factory, bool shared = true)
		{
			AddFactory(targetType.FullName, factory, shared);
		}

		/// <summary>
		/// Internal wrapper for lazy resource factories.
		/// </summary>
		internal sealed class LazyResource
		{
			private readonly Func<object> _factory;
			private readonly bool _shared;
			private object _cachedValue;
			private bool _hasValue;
			private int _invocationCount;

			public LazyResource(Func<object> factory, bool shared)
			{
				_factory = factory ?? throw new ArgumentNullException(nameof(factory));
				_shared = shared;
			}

			public object GetValue()
			{
				_invocationCount++;
				
				if (!_shared)
					return _factory();

				if (!_hasValue)
				{
					_cachedValue = _factory();
					_hasValue = true;
				}
				return _cachedValue;
			}

			/// <summary>
			/// Gets whether this is a shared (cached) resource.
			/// </summary>
			public bool Shared => _shared;

#if DEBUG
			/// <summary>
			/// Gets whether the value has been resolved (factory was invoked at least once for shared resources).
			/// </summary>
			internal bool IsResolved => _hasValue;

			/// <summary>
			/// Gets the number of times GetValue was called.
			/// </summary>
			internal int InvocationCount => _invocationCount;
#endif
		}

		/// <summary>
		/// Resolves a value, invoking the factory if it's a LazyResource.
		/// </summary>
		static object ResolveValue(object value)
		{
			if (value is LazyResource lazy)
				return lazy.GetValue();
			return value;
		}

#if DEBUG
		/// <summary>
		/// Gets diagnostic information about all resources in this dictionary.
		/// For internal/testing use only.
		/// </summary>
		internal ResourceDiagnostics GetDiagnostics()
		{
			var diag = new ResourceDiagnostics();
			
			foreach (var kvp in _innerDictionary)
			{
				diag.TotalCount++;
				
				if (kvp.Value is LazyResource lazy)
				{
					diag.LazyCount++;
					if (lazy.IsResolved)
					{
						diag.ResolvedLazyCount++;
						diag.ResolvedKeys.Add(kvp.Key);
					}
					else
					{
						diag.UnresolvedKeys.Add(kvp.Key);
					}
					diag.InvocationCounts[kvp.Key] = lazy.InvocationCount;
				}
				else
				{
					diag.EagerCount++;
					diag.EagerKeys.Add(kvp.Key);
				}
			}
			
			return diag;
		}

		/// <summary>
		/// Diagnostic information about resources in a ResourceDictionary.
		/// </summary>
		internal class ResourceDiagnostics
		{
			public int TotalCount { get; set; }
			public int LazyCount { get; set; }
			public int EagerCount { get; set; }
			public int ResolvedLazyCount { get; set; }
			public List<string> EagerKeys { get; } = new();
			public List<string> ResolvedKeys { get; } = new();
			public List<string> UnresolvedKeys { get; } = new();
			public Dictionary<string, int> InvocationCounts { get; } = new();
		}
#endif

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

				return GetUriWithExplicitAssembly(value, rootObjectType.Assembly);
			}

			internal static Uri GetUriWithExplicitAssembly(string value, Assembly defaultAssembly)
			{
				(value, var assembly) = SplitUriAndAssembly(value, defaultAssembly);
				return CombineUriAndAssembly(value, assembly);
			}

			internal static ValueTuple<string, Assembly> SplitUriAndAssembly(string value, Assembly defaultAssembly)
			{
				if (value.IndexOf(";assembly=", StringComparison.Ordinal) != -1)
				{
					var parts = value.Split(new[] { ";assembly=" }, StringSplitOptions.RemoveEmptyEntries);
					return (parts[0], Assembly.Load(parts[1]));
				}

				return (value, defaultAssembly);
			}

			internal static Uri CombineUriAndAssembly(string value, Assembly assembly)
			{
				return new Uri($"{value};assembly={assembly.GetName().Name}", UriKind.Relative);
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
