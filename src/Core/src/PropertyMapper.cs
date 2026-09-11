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
		IPropertyMapper[]? _chained;

		// The merged view of this mapper (and its chain) is published as a single reference so that
		// everything derived from one merge - the keys, the mappings, and the key->mapping cache - is
		// always read as a matched set. Mutating the mapper (SetPropertyCore/Chained) drops the reference;
		// the next reader merges again. Readers must therefore snapshot it into a local exactly once.
		// Volatile so a reader on another thread can never see the reference before the merge it points at
		// is fully built.
		volatile MergedMappers? _merged;

		MergedMappers Merged => _merged ?? SnapshotMappers();

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
			var cachedMappers = Merged.CachedMappers;
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

			// One read of the merged reference: `keys[i]` is then guaranteed to be the key whose mapping is
			// `mappers[i]`, even if another thread replaces the merge (with a same-sized one or not) while
			// this pass is running.
			var merged = Merged;
			var keys = merged.Keys;
			var mappers = merged.Mappers;

			// Scope this bulk enumeration as a single "pass" for viewHandler (see PropertyMapperPassScope)
			// so mapper implementations can tell a genuine step of *this* bulk pass apart from an unrelated
			// single-key UpdateProperty call that merely happens to touch the same handler at a different
			// time. Ended in a finally so the pass is never left open - even if a mapper throws partway
			// through the enumeration - and so a nested pass always restores the pass that enclosed it.
			var pass = PropertyMapperPassScope.BeginPass(viewHandler, keys);
			try
			{
				for (int i = 0; i < mappers.Count; i++)
				{
					pass.CurrentKeyIndex = i;
					mappers[i](viewHandler, virtualView);
				}
			}
			finally
			{
				PropertyMapperPassScope.EndPass(pass);
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

		void ClearMergedMappers() => _merged = null;

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

		MergedMappers SnapshotMappers()
		{
			var keys = GetKeys().Distinct().ToList();
			var mappers = new List<Action<IElementHandler, IElement>>(keys.Count);
			var cachedMappers = new Dictionary<string, Action<IElementHandler, IElement>?>(keys.Count);

			foreach (var key in keys)
			{
				var mapper = GetProperty(key)!;
				mappers.Add(mapper);
				cachedMappers[key] = mapper;
			}

			var merged = new MergedMappers(keys, mappers, cachedMappers);
			_merged = merged;

			return merged;
		}

		/// <summary>
		/// The result of merging a mapper with its chain: the keys to enumerate, their mappings in the very
		/// same order, and the key-to-mapping lookup used by single-key updates. Published as one reference
		/// so readers can never observe a mix of two different merges.
		/// </summary>
		sealed class MergedMappers
		{
			public readonly List<string> Keys;

			public readonly List<Action<IElementHandler, IElement>> Mappers;

			/// <summary>
			/// Lazily grown by <see cref="TryUpdatePropertyCore"/> for keys outside <see cref="Keys"/>.
			/// </summary>
			public readonly Dictionary<string, Action<IElementHandler, IElement>?> CachedMappers;

			public MergedMappers(
				List<string> keys,
				List<Action<IElementHandler, IElement>> mappers,
				Dictionary<string, Action<IElementHandler, IElement>?> cachedMappers)
			{
				Keys = keys;
				Mappers = mappers;
				CachedMappers = cachedMappers;
			}
		}
	}

	/// <summary>
	/// Tracks the bulk <see cref="PropertyMapper.UpdateProperties"/> passes currently in progress on the
	/// calling thread: which handler each one is mapping, and which key of that pass is being mapped right
	/// now. Mapper implementations that redirect one property key's update into another (see
	/// VisualElement's BackgroundColor/BackgroundImageSource/SemanticProperties redirects) use this to tell
	/// their own natural, provably-redundant turn within a bulk pass apart from every other invocation -
	/// an explicit single-key <see cref="PropertyMapper.UpdateProperty"/> call (e.g.
	/// <c>Handler.UpdateValue(name)</c>), a re-entrant call raised by a mapper running later in the same
	/// pass, or a call made when no bulk pass is running at all.
	/// </summary>
	/// <remarks>
	/// Passes are tracked in a per-thread stack of reusable frames rather than in a table keyed by handler.
	/// A bulk pass is a strictly synchronous, lexically scoped construct (<c>BeginPass</c>/<c>EndPass</c>
	/// in a <c>try</c>/<c>finally</c>), so a thread-local stack models it exactly: nested passes - whether
	/// on the same handler or on another one - naturally restore their enclosing pass when they end,
	/// concurrent passes on different threads cannot interfere with (or race against) each other, and no
	/// per-pass allocation is needed once a thread's frames have been created.
	/// </remarks>
	internal static class PropertyMapperPassScope
	{
		[ThreadStatic]
		static PropertyMapperPass[]? t_passes;

		[ThreadStatic]
		static int t_depth;

		/// <summary>
		/// Marks the start of a new bulk mapping pass for <paramref name="handler"/> over
		/// <paramref name="keys"/> (the keys of the mapping snapshot being enumerated, in enumeration
		/// order), and returns the frame describing it. The caller must set
		/// <see cref="PropertyMapperPass.CurrentKeyIndex"/> before invoking each key's mapping, and must
		/// pair this with a call to <see cref="EndPass"/> in a <c>finally</c> block so the pass is always
		/// popped - and the enclosing pass, if any, restored - even if a mapping throws.
		/// </summary>
		public static PropertyMapperPass BeginPass(IElementHandler? handler, List<string>? keys)
		{
			var passes = t_passes;
			var depth = t_depth;

			if (passes is null)
			{
				t_passes = passes = new PropertyMapperPass[4];
			}
			else if (depth == passes.Length)
			{
				Array.Resize(ref passes, depth * 2);
				t_passes = passes;
			}

			// Frames are reused across passes on this thread, so a steady-state pass allocates nothing.
			var pass = passes[depth] ??= new PropertyMapperPass();

			pass.Depth = depth;
			pass.Handler = handler;
			pass.Keys = keys;
			pass.CurrentKeyIndex = -1;

			t_depth = depth + 1;

			return pass;
		}

		/// <summary>
		/// Marks the end of <paramref name="pass"/>, restoring whichever pass enclosed it as the current
		/// one. Clears the frame's references so a finished pass never keeps a handler alive and can never
		/// be matched again.
		/// </summary>
		public static void EndPass(PropertyMapperPass pass)
		{
			pass.Handler = null;
			pass.Keys = null;
			pass.CurrentKeyIndex = -1;

			t_depth = pass.Depth;
		}

		/// <summary>
		/// The innermost bulk mapping pass currently running on this thread, or <see langword="null"/> if
		/// none is.
		/// </summary>
		public static PropertyMapperPass? Current
		{
			get
			{
				var depth = t_depth;
				return depth > 0 ? t_passes![depth - 1] : null;
			}
		}

		/// <summary>
		/// Returns <see langword="true"/> when the caller is running as <paramref name="redirectKey"/>'s
		/// own natural step of a bulk mapping pass for <paramref name="handler"/>, and
		/// <paramref name="canonicalKey"/> has already been mapped earlier in that very same pass - i.e.
		/// when re-dispatching <paramref name="redirectKey"/> into <paramref name="canonicalKey"/> is
		/// provably redundant.
		/// </summary>
		/// <remarks>
		/// This deliberately says nothing about <em>which</em> mapping is registered for
		/// <paramref name="canonicalKey"/>: a derived handler is free to own/override it (ButtonHandler,
		/// PageHandler, ...). All that matters is that the pass already ran whatever mapping that key
		/// resolves to, because that is exactly what re-dispatching would invoke again.
		/// Returns <see langword="false"/> for every other shape of invocation - a standalone
		/// <c>UpdateValue</c>, a re-entrant notification raised by a later mapper in the same pass (the
		/// current step is then that other key, not <paramref name="redirectKey"/>), a pass belonging to a
		/// different handler, or a mapper ordering in which <paramref name="canonicalKey"/> does not
		/// precede <paramref name="redirectKey"/> (or is absent entirely).
		/// </remarks>
		public static bool IsRedundantBulkPassRedirect(IElementHandler? handler, string redirectKey, string canonicalKey)
		{
			if (handler is null)
			{
				return false;
			}

			var pass = Current;
			if (pass is null || !ReferenceEquals(pass.Handler, handler))
			{
				return false;
			}

			var keys = pass.Keys;
			var currentKeyIndex = pass.CurrentKeyIndex;

			// Only the pass's own enumeration step for `redirectKey` is skippable. Anything else reaching
			// this mapping (a re-entrant `UpdateValue` raised while some other key is being mapped, for
			// instance) is a genuine request that must be honored.
			if (keys is null || currentKeyIndex < 0 || currentKeyIndex >= keys.Count ||
				!string.Equals(keys[currentKeyIndex], redirectKey, StringComparison.Ordinal))
			{
				return false;
			}

			// `canonicalKey` must have already had its turn in this same pass. Scanning is bounded by the
			// current step and only ever happens on a redirect key's own turn (a handful of times per
			// pass), so it stays well below the cost of the platform work a redundant redirect would do.
			for (int i = 0; i < currentKeyIndex; i++)
			{
				if (string.Equals(keys[i], canonicalKey, StringComparison.Ordinal))
				{
					return true;
				}
			}

			return false;
		}
	}

	/// <summary>
	/// A single, in-progress bulk <see cref="PropertyMapper.UpdateProperties"/> pass. Instances are pooled
	/// and reused per thread by <see cref="PropertyMapperPassScope"/>; they are only meaningful between the
	/// <c>BeginPass</c> that hands one out and the matching <c>EndPass</c>.
	/// </summary>
	internal sealed class PropertyMapperPass
	{
		/// <summary>This pass's index in its thread's pass stack; restored as the depth when it ends.</summary>
		public int Depth;

		/// <summary>The handler whose properties this pass is mapping.</summary>
		public IElementHandler? Handler;

		/// <summary>The keys being enumerated by this pass, in enumeration order.</summary>
		public List<string>? Keys;

		/// <summary>The index in <see cref="Keys"/> of the key currently being mapped, or -1 if none.</summary>
		public int CurrentKeyIndex;
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