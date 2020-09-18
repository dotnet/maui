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
			if (_mapper.TryGetValue(key, out var action))
			{
				action.Action?.Invoke(viewHandler, virtualView);
			}
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
			foreach (var key in Keys)
			{
				UpdatePropertyCore(key, viewHandler, virtualView);
			}
		}

		public virtual ICollection<string> Keys => _mapper.Keys;
	}

	public class PropertyMapper<TVirtualView> : PropertyMapper, IEnumerable
		where TVirtualView : IFrameworkElement
	{
		PropertyMapper _chained;
		ICollection<string> _cachedKeys;

		public PropertyMapper Chained
		{
			get => _chained;
			set
			{
				_chained = value;
				_cachedKeys = null;
			}
		}

		public override ICollection<string> Keys => _cachedKeys ??= (Chained?.Keys.Union(keysForStartup) as ICollection<string> ?? _mapper.Keys);
		ICollection<string> keysForStartup => _mapper.Where(x => x.Value.RunOnUpdateAll).Select(x => x.Key).ToList();

		public int Count => Keys.Count;

		public bool IsReadOnly => false;

		public Action<IViewHandler, TVirtualView> this[string key]
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

		ActionMapper<TVirtualView> actions;
		public ActionMapper<TVirtualView> Actions
		{
			get => actions ??= new ActionMapper<TVirtualView>(this);
		}

		protected override void UpdatePropertyCore(string key, IViewHandler viewHandler, IFrameworkElement virtualView)
		{
			if (_mapper.TryGetValue(key, out var action))
				action.Action?.Invoke(viewHandler, virtualView);
			else
				Chained?.UpdateProperty(viewHandler, virtualView, key);
		}

		public void Add(string key, Action<IViewHandler, TVirtualView> action)
			=> this[key] = action;

		public void Add(string key, Action<IViewHandler, TVirtualView> action, bool ignoreOnStartup)
			=> _mapper[key] = ((r, v) => action?.Invoke(r, (TVirtualView)v), ignoreOnStartup);

		IEnumerator IEnumerable.GetEnumerator() => _mapper.GetEnumerator();
	}
}