using System;
using System.Collections.Generic;
using Command = System.Action<Microsoft.Maui.IElementHandler, Microsoft.Maui.IElement, object?>;

namespace Microsoft.Maui
{
	public abstract class CommandMapper : ICommandMapper
	{
		readonly Dictionary<string, Command> _mapper = new(StringComparer.Ordinal);

		CommandMapper? _chained;

		public CommandMapper()
		{
		}

		public CommandMapper(CommandMapper chained)
		{
			Chained = chained;
		}

		private protected virtual void SetPropertyCore(string key, Command action)
		{
			_mapper[key] = action;
		}

		private protected virtual void InvokeCore(string key, IElementHandler viewHandler, IElement virtualView, object? args)
		{
			if (!viewHandler.CanInvokeMappers())
				return;

			var action = GetCommand(key);
			action?.Invoke(viewHandler, virtualView, args);
		}

		public virtual Command? GetCommand(string key)
		{
			if (_mapper.TryGetValue(key, out var action))
				return action;
			else if (Chained is not null)
				return Chained.GetCommand(key);
			else
				return null;
		}

		public void Invoke(IElementHandler viewHandler, IElement? virtualView, string property, object? args)
		{
			if (virtualView == null)
				return;

			InvokeCore(property, viewHandler, virtualView, args);
		}

		public CommandMapper? Chained
		{
			get => _chained;
			set => _chained = value;
		}
	}

	public interface ICommandMapper
	{
		Command? GetCommand(string key);

		void Invoke(IElementHandler viewHandler, IElement? virtualView, string property, object? args);
	}

	public interface ICommandMapper<out TVirtualView, out TViewHandler> : ICommandMapper
		where TVirtualView : IElement
		where TViewHandler : IElementHandler
	{
		void Add(string key, Action<TViewHandler, TVirtualView> action);

		void Add(string key, Action<TViewHandler, TVirtualView, object?> action);
	}

	public class CommandMapper<TVirtualView, TViewHandler> : CommandMapper, ICommandMapper<TVirtualView, TViewHandler>
		where TVirtualView : IElement
		where TViewHandler : IElementHandler
	{
		public CommandMapper()
		{
		}

		public CommandMapper(CommandMapper chained)
			: base(chained)
		{
		}

		public Action<TViewHandler, TVirtualView, object?> this[string key]
		{
			get
			{
				var action = GetCommand(key) ?? throw new IndexOutOfRangeException($"Unable to find mapping for '{nameof(key)}'.");
				return new Action<TViewHandler, TVirtualView, object?>((h, v, o) => action.Invoke(h, v, o));
			}
			set => Add(key, value);
		}


		public void Add(string key, Action<TViewHandler, TVirtualView> action) =>
			SetPropertyCore(key, (h, v, _) => action?.Invoke((TViewHandler)h, (TVirtualView)v));

		public void Add(string key, Action<TViewHandler, TVirtualView, object?> action) =>
			SetPropertyCore(key, (h, v, o) => action?.Invoke((TViewHandler)h, (TVirtualView)v, o));
	}

	public class CommandMapper<TVirtualView> : CommandMapper<TVirtualView, IElementHandler>
		where TVirtualView : IElement
	{
		public CommandMapper()
		{
		}

		public CommandMapper(CommandMapper chained)
			: base(chained)
		{
		}
	}
}