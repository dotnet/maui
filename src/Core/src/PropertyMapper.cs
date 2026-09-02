using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

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
			_mapper[key] = action;

			ClearMergedMappers();
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

			// Scope this bulk enumeration as a single "pass" for viewHandler (see PropertyMapperPassScope)
			// so mapper implementations can tell a genuine step of *this* bulk pass apart from an unrelated
			// single-key UpdateProperty call that merely happens to touch the same handler at a different
			// time. Ended in a finally so the pass is never left open - and any pass-scoped state that a
			// mapper implementation may have recorded against this pass's id can never again be matched -
			// even if a mapper throws partway through the enumeration.
			var passId = PropertyMapperPassScope.BeginPass(viewHandler);
			try
			{
				foreach (var mapper in UpdatePropertiesMappers)
				{
					mapper(viewHandler, virtualView);
				}
			}
			finally
			{
				PropertyMapperPassScope.EndPass(viewHandler, passId);
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
	}

	/// <summary>
	/// Tracks, per handler, whether a bulk <see cref="PropertyMapper.UpdateProperties"/> pass is currently
	/// in progress for it, and a unique id for that specific pass. Mapper implementations that redirect one
	/// property key's update into another (see VisualElement's BackgroundColor/BackgroundImageSource/
	/// SemanticProperties redirects) can use this to tell whether two invocations happened within the very
	/// same bulk pass - as opposed to two unrelated single-key <see cref="PropertyMapper.UpdateProperty"/>
	/// calls (e.g. <c>Handler.UpdateValue(name)</c>) that merely happen to touch the same handler at
	/// different times, possibly well after the pass that originally triggered a redirect has ended.
	/// </summary>
	internal static class PropertyMapperPassScope
	{
		static readonly ConditionalWeakTable<IElementHandler, StrongBox<int>> s_currentPassId = new();
		static int s_nextPassId;

		/// <summary>
		/// Marks the start of a new bulk mapping pass for <paramref name="handler"/> and returns its
		/// unique, non-zero id. Must be paired with a call to <see cref="EndPass"/> in a <c>finally</c>
		/// block so the pass is considered over - and its id can never again be matched by
		/// <see cref="GetCurrentPassId"/> - even if a mapper throws partway through the pass.
		/// </summary>
		public static int BeginPass(IElementHandler? handler)
		{
			var passId = Interlocked.Increment(ref s_nextPassId);

			// A null handler (e.g. some unit tests call UpdateProperties(null!, ...) directly) can never be
			// looked up again via GetCurrentPassId with that same null reference in a meaningful way, and
			// ConditionalWeakTable does not accept null keys - so there is nothing to track for it.
			if (handler is null)
			{
				return passId;
			}

			s_currentPassId.Remove(handler);
			s_currentPassId.Add(handler, new StrongBox<int>(passId));
			return passId;
		}

		/// <summary>
		/// Marks the end of the bulk mapping pass identified by <paramref name="passId"/> for
		/// <paramref name="handler"/>. A no-op if a nested/reentrant pass has already begun (and not yet
		/// ended) for the same handler by the time this runs, so it never clears a more recent pass's id.
		/// </summary>
		public static void EndPass(IElementHandler? handler, int passId)
		{
			if (handler is null)
			{
				return;
			}

			if (s_currentPassId.TryGetValue(handler, out var box) && box.Value == passId)
			{
				s_currentPassId.Remove(handler);
			}
		}

		/// <summary>
		/// Returns the id of the bulk mapping pass currently in progress for <paramref name="handler"/>,
		/// or <c>0</c> if no bulk mapping pass is currently in progress for it (including when
		/// <paramref name="handler"/> is <see langword="null"/>).
		/// </summary>
		public static int GetCurrentPassId(IElementHandler? handler) =>
			handler is not null && s_currentPassId.TryGetValue(handler, out var box) ? box.Value : 0;
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