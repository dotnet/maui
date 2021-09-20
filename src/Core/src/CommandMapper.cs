using System;
using System.Collections.Generic;
using Command = System.Action<object, Microsoft.Maui.IElement, object?>;

namespace Microsoft.Maui
{
	public abstract class CommandMapper
	{
		readonly Dictionary<string, Command> _mapper = new();

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

		private protected virtual void InvokeCore(string key, object viewHandler, IElement virtualView, object? args)
		{
			var action = GetCommandCore(key);
			action?.Invoke(viewHandler, virtualView, args);
		}

		private protected virtual Command? GetCommandCore(string key)
		{
			if (_mapper.TryGetValue(key, out var action))
				return action;
			else if (Chained is not null)
				return Chained.GetCommandCore(key);
			else
				return null;
		}

		internal void Invoke(IElementHandler viewHandler, IElement? virtualView, string property, object? args)
		{
			if (virtualView == null)
				return;

			InvokeCore(property, viewHandler, virtualView, args);
		}

		public CommandMapper? Chained
		{
			get => _chained;
			set
			{
				_chained = value;
			}
		}
	}

	public class CommandMapper<TVirtualView, TViewHandler> : CommandMapper
		where TVirtualView : IElement
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
				var action = GetCommandCore(key) ?? throw new IndexOutOfRangeException($"Unable to find mapping for '{nameof(key)}'.");
				return new Action<TViewHandler, TVirtualView, object?>((h, v, o) =>
				{
					if (h is TViewHandler viewHandler)
						action.Invoke(viewHandler, v, o);
				});
			}
			set => Add(key, value);
		}


		public void Add(string key, Action<TViewHandler, TVirtualView> action) =>
			Add(key, action);

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