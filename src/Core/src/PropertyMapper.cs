using System;
using System.Collections.Generic;
using System.Linq;

#if IOS || MACCATALYST
using PlatformView = UIKit.UIView;
#elif ANDROID
using PlatformView = Android.Views.View;
#elif WINDOWS
using PlatformView = Microsoft.UI.Xaml.FrameworkElement;
#elif (NETSTANDARD || !PLATFORM) || (NET6_0_OR_GREATER && !IOS && !ANDROID)
using PlatformView = System.Object;
#endif

namespace Microsoft.Maui
{
	public abstract class PropertyMapper : IPropertyMapper
	{
		private protected readonly Dictionary<string, Action<IElementHandler, IElement>> _mapper = new(StringComparer.Ordinal);
		readonly Dictionary<string, MappingCustomization> _mappingCustomizations = new(StringComparer.Ordinal);
		bool _frameworkMappingsSealed;
		bool _preserveMappingCustomizations;
		IPropertyMapper[]? _chained;

		List<string>? _updatePropertiesKeys;
		List<Action<IElementHandler, IElement>>? _updatePropertiesMappers;
		Dictionary<string, Action<IElementHandler, IElement>?>? _cachedMappers;

		List<string> UpdatePropertiesKeys => _updatePropertiesKeys ?? SnapshotMappers().UpdatePropertiesKeys;
		List<Action<IElementHandler, IElement>> UpdatePropertiesMappers => _updatePropertiesMappers ?? SnapshotMappers().UpdatePropertiesMappers;
		Dictionary<string, Action<IElementHandler, IElement>?> CachedMappers => _cachedMappers ?? SnapshotMappers().CachedMappers;

		public PropertyMapper()
		{
		}

		public PropertyMapper(params IPropertyMapper[]? chained)
		{
			Chained = chained;
		}

		protected virtual void SetPropertyCore(string key, Action<IElementHandler, IElement> action)
		{
			if (!_preserveMappingCustomizations
				&& (_mapper.ContainsKey(key) || (_frameworkMappingsSealed && GetChainedProperty(key) is not null)))
			{
				AddMappingCustomization(key, _ => action);
				return;
			}

			_mapper[key] = action;

			ClearMergedMappers();
		}

		void SetCustomizedPropertyCore(string key, Action<IElementHandler, IElement> action)
		{
			_preserveMappingCustomizations = true;
			try
			{
				SetPropertyCore(key, action);
			}
			finally
			{
				_preserveMappingCustomizations = false;
			}
		}

		internal void AddMappingCustomization(
			string key,
			Func<Action<IElementHandler, IElement>?, Action<IElementHandler, IElement>> customization)
		{
			if (!_mappingCustomizations.TryGetValue(key, out var mappingCustomization))
			{
				mappingCustomization = CreateMappingCustomization(key);
				_mappingCustomizations[key] = mappingCustomization;
			}

			mappingCustomization.Customizations.Add(customization);
			SetCustomizedPropertyCore(key, mappingCustomization.Compose());
		}

		internal void SealFrameworkMappings() => _frameworkMappingsSealed = true;

		internal void ModifyFrameworkMapping(
			string key,
			Func<Action<IElementHandler, IElement>?, Action<IElementHandler, IElement>> modification)
		{
			if (!_mappingCustomizations.TryGetValue(key, out var mappingCustomization))
			{
				mappingCustomization = CreateMappingCustomization(key);
				_mappingCustomizations[key] = mappingCustomization;
			}

			mappingCustomization.ModifyFrameworkMapping(modification);
			SetCustomizedPropertyCore(key, mappingCustomization.Compose());
		}

		MappingCustomization CreateMappingCustomization(string key)
		{
			if (_mapper.TryGetValue(key, out var localMapping))
				return new(() => localMapping);

			return new(() => GetChainedProperty(key));
		}

		protected virtual void UpdatePropertyCore(string key, IElementHandler viewHandler, IElement virtualView)
		{
			if (!viewHandler.CanInvokeMappers())
			{
				return;
			}

			TryUpdatePropertyCore(key, viewHandler, virtualView);
		}

		internal bool TryUpdatePropertyCore(string key, IElementHandler viewHandler, IElement virtualView)
		{
			var cachedMappers = CachedMappers;
			if (cachedMappers.TryGetValue(key, out var action))
			{
				if (action is not null)
				{
					action(viewHandler, virtualView);
					return true;
				}

				return false;
			}

			// CachedMappers initially contains only the UpdateProperties keys which may not contain the key we are looking for.
			// This should never happen, but there's a chance someone may have customized `GetKeys` to return a subset of the actual registered mapper keys.
			var mapper = GetProperty(key);
			cachedMappers[key] = mapper;

			if (mapper is not null)
			{
				mapper(viewHandler, virtualView);
				return true;
			}

			return false;
		}

		public virtual Action<IElementHandler, IElement>? GetProperty(string key)
		{
			if (_mapper.TryGetValue(key, out var action))
			{
				return action;
			}

			return GetChainedProperty(key);
		}

		Action<IElementHandler, IElement>? GetChainedProperty(string key)
		{
			var chainedPropertyMappers = Chained;
			if (chainedPropertyMappers is not null)
			{
				foreach (var ch in chainedPropertyMappers)
				{
					var returnValue = ch.GetProperty(key);
					if (returnValue != null)
					{
						return returnValue;
					}
				}
			}

			return null;
		}

		public void UpdateProperty(IElementHandler viewHandler, IElement? virtualView, string property)
		{
			if (virtualView == null || !viewHandler.CanInvokeMappers())
			{
				return;
			}

			TryUpdatePropertyCore(property, viewHandler, virtualView);
		}

		public void UpdateProperties(IElementHandler viewHandler, IElement? virtualView)
		{
			if (virtualView == null || !viewHandler.CanInvokeMappers())
			{
				return;
			}

			foreach (var mapper in UpdatePropertiesMappers)
			{
				mapper(viewHandler, virtualView);
			}
		}

		public IPropertyMapper[]? Chained
		{
			get => _chained;
			set
			{
				_chained = value;
				ClearMergedMappers();
			}
		}

		void ClearMergedMappers()
		{
			_updatePropertiesMappers = null;
			_updatePropertiesKeys = null;
			_cachedMappers = null;
		}

		public virtual IEnumerable<string> GetKeys()
		{
			// We want to retain the initial order of the keys to avoid race conditions
			// when a property mapping is overridden by a new instance of property mapper.
			// As an example, the container view mapper should always run first.
			// Siblings mapper should not have keys intersection.
			var chainedPropertyMappers = Chained;
			if (chainedPropertyMappers is not null)
			{
				for (int i = chainedPropertyMappers.Length - 1; i >= 0; i--)
				{
					foreach (var key in chainedPropertyMappers[i].GetKeys())
					{
						yield return key;
					}
				}
			}

			// Enqueue keys from this mapper.
			foreach (var mapper in _mapper)
			{
				yield return mapper.Key;
			}
		}

		private (List<string> UpdatePropertiesKeys, List<Action<IElementHandler, IElement>> UpdatePropertiesMappers, Dictionary<string, Action<IElementHandler, IElement>?> CachedMappers) SnapshotMappers()
		{
			var updatePropertiesKeys = GetKeys().Distinct().ToList();
			var updatePropertiesMappers = new List<Action<IElementHandler, IElement>>(updatePropertiesKeys.Count);
			var cachedMappers = new Dictionary<string, Action<IElementHandler, IElement>?>(updatePropertiesKeys.Count);

			foreach (var key in updatePropertiesKeys)
			{
				var mapper = GetProperty(key)!;
				updatePropertiesMappers.Add(mapper);
				cachedMappers[key] = mapper;
			}

			_updatePropertiesKeys = updatePropertiesKeys;
			_updatePropertiesMappers = updatePropertiesMappers;
			_cachedMappers = cachedMappers;

			return (updatePropertiesKeys, updatePropertiesMappers, cachedMappers);
		}

		sealed class MappingCustomization
		{
			Func<Action<IElementHandler, IElement>?> _frameworkMapping;

			public MappingCustomization(Func<Action<IElementHandler, IElement>?> frameworkMapping)
			{
				_frameworkMapping = frameworkMapping;
			}

			public List<Func<Action<IElementHandler, IElement>?, Action<IElementHandler, IElement>>> Customizations { get; } = new();

			public void ModifyFrameworkMapping(
				Func<Action<IElementHandler, IElement>?, Action<IElementHandler, IElement>> modification)
			{
				var previousFrameworkMapping = _frameworkMapping;
				var previousMapping = CreateDynamicMapping(previousFrameworkMapping);
				var newMapping = modification(previousMapping);
				_frameworkMapping = () => newMapping;
			}

			public Action<IElementHandler, IElement> Compose()
			{
				var mapping = CreateDynamicMapping(_frameworkMapping);
				foreach (var customization in Customizations)
				{
					mapping = customization(mapping);
				}

				return mapping!;
			}

			static Action<IElementHandler, IElement>? CreateDynamicMapping(
				Func<Action<IElementHandler, IElement>?> mappingAccessor)
			{
				if (mappingAccessor() is null)
					return null;

				return (handler, view) => mappingAccessor()?.Invoke(handler, view);
			}
		}
	}

	public interface IPropertyMapper
	{
		Action<IElementHandler, IElement>? GetProperty(string key);

		IEnumerable<string> GetKeys();

		void UpdateProperties(IElementHandler elementHandler, IElement virtualView);

		void UpdateProperty(IElementHandler elementHandler, IElement virtualView, string property);
	}

	public interface IPropertyMapper<out TVirtualView, out TViewHandler> : IPropertyMapper
		where TVirtualView : IElement
		where TViewHandler : IElementHandler
	{
		void Add(string key, Action<TViewHandler, TVirtualView> action);
	}

	public class PropertyMapper<TVirtualView, TViewHandler> : PropertyMapper, IPropertyMapper<TVirtualView, TViewHandler>
		where TVirtualView : IElement
		where TViewHandler : IElementHandler
	{
		public PropertyMapper()
		{
		}

		public PropertyMapper(params IPropertyMapper[] chained)
			: base(chained)
		{
		}

		internal PropertyMapper<TVirtualView, TViewHandler> WithFrameworkMappingsSealed()
		{
			SealFrameworkMappings();
			return this;
		}

		public Action<TViewHandler, TVirtualView> this[string key]
		{
			get
			{
				var action = GetProperty(key) ?? throw new IndexOutOfRangeException($"Unable to find mapping for '{nameof(key)}'.");
				return new Action<TViewHandler, TVirtualView>((h, v) => action.Invoke(h, v));
			}
			set => Add(key, value);
		}

		public void Add(string key, Action<TViewHandler, TVirtualView> action) =>
			SetPropertyCore(key, (h, v) =>
			{
				if (v is TVirtualView vv)
				{
					action?.Invoke((TViewHandler)h, vv);
				}
				else if (Chained != null)
				{
					foreach (var chain in Chained)
					{
						// Try to leverage our internal method which uses merged mappers
						if (chain is PropertyMapper propertyMapper)
						{
							if (propertyMapper.TryUpdatePropertyCore(key, h, v))
							{
								break;
							}
						}
						else if (chain.GetProperty(key) != null)
						{
							chain.UpdateProperty(h, v, key);
							break;
						}
					}
				}
			});
	}

	public class PropertyMapper<TVirtualView> : PropertyMapper<TVirtualView, IElementHandler>
		where TVirtualView : IElement
	{
		public PropertyMapper()
		{
		}

		public PropertyMapper(params PropertyMapper[] chained)
			: base(chained)
		{
		}
	}
}