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
		protected readonly Dictionary<string, Action<IElementHandler, IElement>> _mapper = new(StringComparer.Ordinal);

#pragma warning disable RS0016 // Add public types and members to the declared API
		protected internal readonly Dictionary<string, Func<IElement, bool>> _initSkipChecks = new(StringComparer.Ordinal);
#pragma warning restore RS0016 // Add public types and members to the declared API

		IPropertyMapper[]? _chained;

		// Keep a distinct list of the keys so we don't run any duplicate (overridden) updates more than once
		// when we call UpdateProperties
		HashSet<string>? _updateKeys;

		public PropertyMapper()
		{
		}

		public PropertyMapper(params IPropertyMapper[]? chained)
		{
			Chained = chained;
		}

#pragma warning disable RS0016 // Add public types and members to the declared API
		public PropertyMapper(Dictionary<string, Func<IElement, bool>> initSkipChecks, params IPropertyMapper[]? chained)
		{
			_initSkipChecks = initSkipChecks;
			Chained = chained;
		}
#pragma warning restore RS0016 // Add public types and members to the declared API

		protected virtual void SetPropertyCore(string key, Action<IElementHandler, IElement> action)
		{
			_mapper[key] = action;
			ClearKeyCache();
		}

		protected virtual void UpdatePropertyCore(string key, IElementHandler viewHandler, IElement virtualView)
		{
			if (!viewHandler.CanInvokeMappers())
				return;

			var action = GetProperty(key);
			action?.Invoke(viewHandler, virtualView);
		}

		public virtual Action<IElementHandler, IElement>? GetProperty(string key)
		{
			if (_mapper.TryGetValue(key, out var action))
			{
				return action;
			}
			else if (Chained is not null)
			{
				foreach (var ch in Chained)
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
			if (virtualView == null)
			{
				return;
			}

			UpdatePropertyCore(property, viewHandler, virtualView);
		}

#pragma warning disable RS0016 // Add public types and members to the declared API
		public void UpdateProperties(IElementHandler viewHandler, IElement? virtualView, bool initial = false)
		{
			if (virtualView == null)
			{
				return;
			}

			foreach (var key in UpdateKeys)
			{
				if (initial)
				{
					if (_initSkipChecks.TryGetValue(key, out Func<IElement, bool>? skipCheck) && skipCheck(virtualView))
					{
						continue;
					}
				}

				UpdatePropertyCore(key, viewHandler, virtualView);
			}
		}
#pragma warning restore RS0016 // Add public types and members to the declared API

		public IPropertyMapper[]? Chained
		{
			get => _chained;
			set
			{
				_chained = value;
				ClearKeyCache();
			}
		}

		private HashSet<string> PopulateKeys()
		{
			var keys = new HashSet<string>(StringComparer.Ordinal);
			foreach (var key in GetKeys())
			{
				keys.Add(key);
			}
			return keys;
		}

		protected virtual void ClearKeyCache()
		{
			_updateKeys = null;
		}

		public virtual IReadOnlyCollection<string> UpdateKeys => _updateKeys ??= PopulateKeys();

		public virtual IEnumerable<string> GetKeys()
		{
			foreach (var key in _mapper.Keys)
				yield return key;

			if (Chained is not null)
			{
				foreach (var chain in Chained)
					foreach (var key in chain.GetKeys())
						yield return key;
			}
		}
	}

	public interface IPropertyMapper
	{
		Action<IElementHandler, IElement>? GetProperty(string key);

		IEnumerable<string> GetKeys();

		void UpdateProperties(IElementHandler elementHandler, IElement virtualView, bool initial = false);

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

#pragma warning disable RS0016 // Add public types and members to the declared API
		public PropertyMapper(Dictionary<string, Func<IElement, bool>> initSkipChecks, params IPropertyMapper[] chained)
			: base(initSkipChecks: initSkipChecks, chained: chained)
		{
		}
#pragma warning restore RS0016 // Add public types and members to the declared API

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
						if (chain.GetProperty(key) != null)
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