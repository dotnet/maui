using System;
using System.Collections.Generic;
using Factory = System.Func<Microsoft.Maui.IElementHandler, Microsoft.Maui.IElement, object?, object?>;

namespace Microsoft.Maui
{
	public abstract class FactoryMapper
	{
		readonly Dictionary<string, Factory> _mapper = new();

		FactoryMapper? _chained;

		public FactoryMapper()
		{
		}

		public FactoryMapper(FactoryMapper chained)
		{
			Chained = chained;
		}

		private protected virtual void SetPropertyCore(string key, Factory action)
		{
			_mapper[key] = action;
		}

		private protected virtual object? InvokeCore(string key, IElementHandler viewHandler, IElement virtualView, object? args)
		{
			var action = GetFactoryCore(key);
			return action?.Invoke(viewHandler, virtualView, args);
		}

		private protected virtual Factory? GetFactoryCore(string key)
		{
			if (_mapper.TryGetValue(key, out var action))
				return action;
			else if (Chained is not null)
				return Chained.GetFactoryCore(key);
			else
				return null;
		}

		internal object? Invoke(IElementHandler viewHandler, IElement? virtualView, string property, object? args)
		{
			if (virtualView == null)
				return default;

			return InvokeCore(property, viewHandler, virtualView, args);
		}

		public FactoryMapper? Chained
		{
			get => _chained;
			set
			{
				_chained = value;
			}
		}
	}

	public class FactoryMapper<TVirtualView, TViewHandler> : FactoryMapper
		where TVirtualView : IElement
		where TViewHandler : IElementHandler
	{
		public FactoryMapper()
		{
		}

		public FactoryMapper(FactoryMapper chained)
			: base(chained)
		{
		}

		public Func<TViewHandler, TVirtualView, object?, object?> this[string key]
		{
			get
			{
				var action = GetFactoryCore(key) ?? throw new IndexOutOfRangeException($"Unable to find mapping for '{nameof(key)}'.");
				return new Func<TViewHandler, TVirtualView, object?, object?>((h, v, args) => action.Invoke(h, v, args));
			}
			set => Add(key, value);
		}


		public void Add(string key, Func<TViewHandler, TVirtualView> action) =>
			Add(key, action);

		public void Add(string key, Func<TViewHandler, TVirtualView, object?, object?> action) =>
			SetPropertyCore(key, (h, v, args) => action?.Invoke((TViewHandler)h, (TVirtualView)v, args));
	}

	public class FactoryMapper<TVirtualView> : FactoryMapper<TVirtualView, IElementHandler>
		where TVirtualView : IElement
	{
		public FactoryMapper()
		{
		}

		public FactoryMapper(FactoryMapper chained)
			: base(chained)
		{
		}
	}
}