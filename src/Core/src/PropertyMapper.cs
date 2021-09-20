using System;
using System.Collections.Generic;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui
{
	public abstract class PropertyMapper : IPropertyMapper
	{
		readonly Dictionary<string, Action<object, IElement>> _mapper = new();
		readonly IPropertyMapper[]? _propertyMappers;
		IPropertyMapper? _chained;

		// Keep a distinct list of the keys so we don't run any duplicate (overridden) updates more than once
		// when we call UpdateProperties
		HashSet<string>? _updateKeys;

		public PropertyMapper()
		{
		}

		public PropertyMapper(IPropertyMapper chained)
		{
			Chained = chained;
		}
				
		public PropertyMapper(IPropertyMapper chained, params IPropertyMapper[] propertyMappers)
		{
			Chained = chained;
			_propertyMappers = propertyMappers;
		}

		protected virtual void SetPropertyCore(string key, Action<object, IElement> action)
		{
			_mapper[key] = action;
			ClearKeyCache();
		}

		protected virtual void UpdatePropertyCore(string key, object viewHandler, IElement virtualView)
		{
			var action = GetProperty(key);
			action?.Invoke(viewHandler, virtualView);
		}

		public virtual Action<object, IElement>? GetProperty(string key)
		{
			if (_mapper.TryGetValue(key, out var action))
				return action;
			else if (Chained is not null)
			{
				// TODO MAUI make this less messy
				// currently just doing this to get a point across
				// we'll probably create something called "compositeMapper : PropertyMapper"

				if (_propertyMappers != null)
				{
					foreach(var mapper in _propertyMappers)
					{
						var thing = mapper.GetProperty(key);
						if (thing != null)
						{
							return thing;
						}
					}
				}
				return Chained.GetProperty(key);
			}
			else
				return null;
		}

		public void UpdateProperty(object viewHandler, IElement? virtualView, string property)
		{
			if (virtualView == null)
				return;

			UpdatePropertyCore(property, viewHandler, virtualView);
		}

		public void UpdateProperties(object viewHandler, IElement? virtualView)
		{
			if (virtualView == null)
				return;

			foreach (var key in UpdateKeys)
			{
				UpdatePropertyCore(key, viewHandler, virtualView);
			}
		}

		public IPropertyMapper? Chained
		{
			get => _chained;
			set
			{
				_chained = value;
				ClearKeyCache();
			}
		}

		protected HashSet<string> PopulateKeys(ref HashSet<string>? returnList)
		{
			_updateKeys = new HashSet<string>();

			foreach (var key in GetKeys())
			{
				_updateKeys.Add(key);
			}

			return returnList ?? new HashSet<string>();
		}

		protected virtual void ClearKeyCache()
		{
			_updateKeys = null;
		}

		public virtual IReadOnlyCollection<string> UpdateKeys =>
			_updateKeys ?? PopulateKeys(ref _updateKeys);

		public IEnumerable<string> GetKeys()
		{
			foreach (var key in _mapper.Keys)
				yield return key;

			if (Chained is not null)
			{
				foreach (var key in Chained.GetKeys())
					yield return key;
			}
		}
	}

	public interface IPropertyMapper
	{
		Action<object, IElement>? GetProperty(string key);

		IEnumerable<string> GetKeys();

		void UpdateProperties(object elementHandler, IElement virtualView);

		void UpdateProperty(object elementHandler, IElement virtualView, string property);
	}

	public interface IPropertyMapper<out TVirtualView, out TViewHandler> : IPropertyMapper
		where TVirtualView : IElement
	{
		void Add(string key, Action<TViewHandler, TVirtualView> action);
	}

	public class PropertyMapper<TVirtualView, TViewHandler> : PropertyMapper, IPropertyMapper<TVirtualView, TViewHandler>
		where TVirtualView : IElement
	{
		public PropertyMapper()
		{
		}

		public PropertyMapper(IPropertyMapper chained)
			: base(chained)
		{
		}

		public PropertyMapper(IPropertyMapper chained, params IPropertyMapper[] propertyMappers)
			: base(chained, propertyMappers)
		{
		}

		public Action<TViewHandler, TVirtualView> this[string key]
		{
			get
			{
				var action = GetProperty(key) ?? throw new IndexOutOfRangeException($"Unable to find mapping for '{nameof(key)}'.");
				return new Action<TViewHandler, TVirtualView>((h, v) =>
				{
					if (h is TViewHandler viewHandler)
						action.Invoke(viewHandler, v);

					throw new InvalidOperationException($"{h} can't be cast to type {typeof(TViewHandler)}");
				});
			}
			set => Add(key, value);
		}

		public void Add(string key, Action<TViewHandler, TVirtualView> action) =>
			SetPropertyCore(key, (h, v) =>
			{
				if (v is TVirtualView vv)
					action?.Invoke((TViewHandler)h, vv);
				else
					Chained?.UpdateProperty(h, v, key);
			});
	}

	public class PropertyMapper<TVirtualView> : PropertyMapper<TVirtualView, IElementHandler>
		where TVirtualView : IElement
	{
		public PropertyMapper()
		{
		}

		public PropertyMapper(PropertyMapper chained)
			: base(chained)
		{
		}
	}
}