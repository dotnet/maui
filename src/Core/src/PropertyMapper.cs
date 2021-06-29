using System;
using System.Collections.Generic;

namespace Microsoft.Maui
{
	public abstract class PropertyMapper
	{
		readonly Dictionary<string, (Action<IElementHandler, IElement> Action, bool RunOnUpdateAll)> _mapper = new();

		PropertyMapper? _chained;

		HashSet<string>? _allKeys;
		HashSet<string>? _actionKeys;
		HashSet<string>? _updateKeys;

		public PropertyMapper()
		{
		}

		public PropertyMapper(PropertyMapper chained)
		{
			Chained = chained;
		}

		private protected virtual void SetPropertyCore(string key, Action<IElementHandler, IElement> action, bool runOnUpdateAll)
		{
			_mapper[key] = (action, runOnUpdateAll);
			ClearKeyCache();
		}

		private protected virtual void UpdatePropertyCore(string key, IElementHandler viewHandler, IElement virtualView)
		{
			var action = GetPropertyCore(key);
			action.Action?.Invoke(viewHandler, virtualView);
		}

		private protected virtual (Action<IElementHandler, IElement>? Action, bool RunOnUpdateAll) GetPropertyCore(string key)
		{
			if (_mapper.TryGetValue(key, out var action))
				return action;
			else if (Chained is not null)
				return Chained.GetPropertyCore(key);
			else
				return (null, false);
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
			_allKeys = new HashSet<string>();
			_updateKeys = new HashSet<string>();
			_actionKeys = new HashSet<string>();

			foreach (var key in GetKeys())
			{
				_allKeys.Add(key);

				var result = GetPropertyCore(key);

				if (result.RunOnUpdateAll)
					_updateKeys.Add(key);
				else
					_actionKeys.Add(key);
			}

			return returnList ?? new HashSet<string>();
		}

		protected virtual void ClearKeyCache()
		{
			_allKeys = null;
			_updateKeys = null;
			_actionKeys = null;
		}

		public virtual IReadOnlyCollection<string> Keys =>
			_allKeys ?? PopulateKeys(ref _allKeys);

		public virtual IReadOnlyCollection<string> ActionKeys =>
			_actionKeys ?? PopulateKeys(ref _actionKeys);

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
		ActionMapper<TVirtualView, TViewHandler>? _actions;

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
				var action = GetPropertyCore(key).Action ?? throw new IndexOutOfRangeException($"Unable to find mapping for '{nameof(key)}'.");
				return new Action<TViewHandler, TVirtualView>((h, v) => action.Invoke(h, v));
			}
			set => Add(key, value, true);
		}

		public ActionMapper<TVirtualView, TViewHandler> Actions =>
			_actions ??= new ActionMapper<TVirtualView, TViewHandler>(this);

		public void Add(string key, Action<TViewHandler, TVirtualView> action) =>
			Add(key, action, true);

		public void Add(string key, Action<TViewHandler, TVirtualView> action, bool runOnUpdateAll) =>
			SetPropertyCore(key, (h, v) => action?.Invoke((TViewHandler)h, (TVirtualView)v), runOnUpdateAll);
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