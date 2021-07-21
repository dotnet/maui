using System;
using System.Collections.Generic;

namespace Microsoft.Maui
{
	public abstract class PropertyMapper
	{
		readonly Dictionary<string, Action<IElementHandler, IElement>> _mapper = new();

		PropertyMapper? _chained;

		// Keep a distinct list of the keys so we don't run any duplicate (overridden) updates more than once
		// when we call UpdateProperties
		HashSet<string>? _updateKeys;

		public PropertyMapper()
		{
		}

		public PropertyMapper(PropertyMapper chained)
		{
			Chained = chained;
		}

		private protected virtual void SetPropertyCore(string key, Action<IElementHandler, IElement> action)
		{
			_mapper[key] = action;
			ClearKeyCache();
		}

		private protected virtual void UpdatePropertyCore(string key, IElementHandler viewHandler, IElement virtualView)
		{
			var action = GetPropertyCore(key);
			action?.Invoke(viewHandler, virtualView);
		}

		private protected virtual Action<IElementHandler, IElement>? GetPropertyCore(string key)
		{
			if (_mapper.TryGetValue(key, out var action))
				return action;
			else if (Chained is not null)
				return Chained.GetPropertyCore(key);
			else
				return null;
		}

		internal void UpdateProperty(IElementHandler viewHandler, IElement? virtualView, string property)
		{
			if (virtualView == null)
				return;

			UpdatePropertyCore(property, viewHandler, virtualView);
		}

		internal void UpdateProperties(IElementHandler viewHandler, IElement? virtualView)
		{
			if (virtualView == null)
				return;

			foreach (var key in UpdateKeys)
			{
				UpdatePropertyCore(key, viewHandler, virtualView);
			}
		}

		public PropertyMapper? Chained
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

		IEnumerable<string> GetKeys()
		{
			foreach (var key in _mapper.Keys)
				yield return key;

			if (Chained is not null)
			{
				foreach (var key in Chained._mapper.Keys)
					yield return key;
			}
		}
	}

	public class PropertyMapper<TVirtualView, TViewHandler> : PropertyMapper
		where TVirtualView : IElement
		where TViewHandler : IElementHandler
	{
		public PropertyMapper()
		{
		}

		public PropertyMapper(PropertyMapper chained)
			: base(chained)
		{
		}

		public Action<TViewHandler, TVirtualView> this[string key]
		{
			get
			{
				var action = GetPropertyCore(key) ?? throw new IndexOutOfRangeException($"Unable to find mapping for '{nameof(key)}'.");
				return new Action<TViewHandler, TVirtualView>((h, v) => action.Invoke(h, v));
			}
			set => Add(key, value);
		}

		public void Add(string key, Action<TViewHandler, TVirtualView> action) =>
			SetPropertyCore(key, (h, v) => action?.Invoke((TViewHandler)h, (TVirtualView)v));
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