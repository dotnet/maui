using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Xamarin.Platform
{
	public class PropertyMapper
	{
		internal Dictionary<string, (Action<IViewHandler, IFrameworkElement> Action, bool RunOnUpdateAll)> _mapper = new Dictionary<string, (Action<IViewHandler, IFrameworkElement> action, bool runOnUpdateAll)>();

		protected virtual void UpdatePropertyCore(string key, IViewHandler viewHandler, IFrameworkElement virtualView)
		{
			var action = Get(key);
			action.Action?.Invoke(viewHandler, virtualView);
		}

		public void UpdateProperty(IViewHandler viewHandler, IFrameworkElement virtualView, string property)
		{
			if (virtualView == null)
				return;
			UpdatePropertyCore(property, viewHandler, virtualView);
		}

		public void UpdateProperties(IViewHandler viewHandler, IFrameworkElement virtualView)
		{
			if (virtualView == null)
				return;
			foreach (var key in UpdateKeys)
			{
				UpdatePropertyCore(key, viewHandler, virtualView);
			}
		}

		public virtual ICollection<string> Keys => _mapper.Keys;
		protected List<string> PopulateKeys(ref List<string> returnList)
		{
			updateKeys = new List<string>();
			actionKeys = new List<string>();
			foreach (var key in Keys)
			{
				var result = Get(key);
				if (result.RunOnUpdateAll)
					updateKeys.Add(key);
				else
					actionKeys.Add(key);

			}
			return returnList;
		}
		protected virtual void ClearKeyCache()
		{
			updateKeys = null;
			actionKeys = null;
		}
		public virtual (Action<IViewHandler, IFrameworkElement> Action, bool RunOnUpdateAll) Get(string key)
		{
			_mapper.TryGetValue(key, out var action);
			return action;
		}
		List<string> actionKeys;
		List<string> updateKeys;
		public virtual IReadOnlyList<string> ActionKeys => actionKeys ?? PopulateKeys(ref actionKeys);
		public virtual IReadOnlyList<string> UpdateKeys => updateKeys ?? PopulateKeys(ref updateKeys);
	}

	public class PropertyMapper<TVirtualView, TViewHandler> : PropertyMapper, IEnumerable
		where TVirtualView : IFrameworkElement
		where TViewHandler : IViewHandler
	{
		private PropertyMapper chained;
		public PropertyMapper Chained
		{
			get => chained;
			set
			{
				chained = value;
				ClearKeyCache();
			}
		}

		ICollection<string> cachedKeys;
		public override ICollection<string> Keys => cachedKeys ??= (Chained?.Keys.Union(_mapper.Keys).ToList() as ICollection<string> ?? _mapper.Keys);

		public int Count => Keys.Count;

		public bool IsReadOnly => false;

		public Action<TViewHandler, TVirtualView> this[string key]
		{
			set => Add(key, value, true);
		}

		public PropertyMapper()
		{
		}

		public PropertyMapper(PropertyMapper chained)
		{
			Chained = chained;
		}

		ActionMapper<TVirtualView, TViewHandler> actions;
		public ActionMapper<TVirtualView, TViewHandler> Actions
		{
			get => actions ??= new ActionMapper<TVirtualView, TViewHandler>(this);
		}

		protected override void ClearKeyCache()
		{
			base.ClearKeyCache();
			cachedKeys = null;
		}


		public override (Action<IViewHandler, IFrameworkElement> Action, bool RunOnUpdateAll) Get(string key)
		{

			if (_mapper.TryGetValue(key, out var action))
				return action;
			else
				return Chained?.Get(key) ?? (null, false);
		}

		public void Add(string key, Action<TViewHandler, TVirtualView> action)
			=> this[key] = action;

		public void Add(string key, Action<TViewHandler, TVirtualView> action, bool ignoreOnStartup)
			=> _mapper[key] = ((r, v) => action?.Invoke((TViewHandler)r, (TVirtualView)v), ignoreOnStartup);

		IEnumerator IEnumerable.GetEnumerator() => _mapper.GetEnumerator();
	}




	public class PropertyMapper<TVirtualView> : PropertyMapper<TVirtualView, IViewHandler>
		where TVirtualView : IFrameworkElement
	{
		public PropertyMapper()
		{
		}

		public PropertyMapper(PropertyMapper chained) : base(chained)
		{
		}
	}
}
