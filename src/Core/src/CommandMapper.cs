using System;
using System.Collections.Generic;
using Command = System.Action<Microsoft.Maui.IElementHandler, Microsoft.Maui.IElement, object?>;

namespace Microsoft.Maui
{
	public abstract class CommandMapper : ICommandMapper
	{
		readonly Dictionary<string, Command> _mapper = new(StringComparer.Ordinal);
		readonly Dictionary<string, MappingCustomization> _mappingCustomizations = new(StringComparer.Ordinal);
		bool _frameworkMappingsSealed;
		bool _preserveMappingCustomizations;

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
			if (!_preserveMappingCustomizations
				&& (_mapper.ContainsKey(key) || (_frameworkMappingsSealed && Chained?.GetCommand(key) is not null)))
			{
				AddMappingCustomization(key, _ => action);
				return;
			}

			_mapper[key] = action;
		}

		void SetCustomizedPropertyCore(string key, Command action)
		{
			_preserveMappingCustomizations = true;
			try
			{
				SetPropertyCore(key, action);
			}
			finally
			{
				_preserveMappingCustomizations = false;
			}
		}

		internal void AddMappingCustomization(string key, Func<Command?, Command> customization)
		{
			if (!_mappingCustomizations.TryGetValue(key, out var mappingCustomization))
			{
				mappingCustomization = CreateMappingCustomization(key);
				_mappingCustomizations[key] = mappingCustomization;
			}

			mappingCustomization.Customizations.Add(customization);
			SetCustomizedPropertyCore(key, mappingCustomization.Compose());
		}

		internal void SealFrameworkMappings() => _frameworkMappingsSealed = true;

		internal void ModifyFrameworkMapping(string key, Func<Command?, Command> modification)
		{
			if (!_mappingCustomizations.TryGetValue(key, out var mappingCustomization))
			{
				mappingCustomization = CreateMappingCustomization(key);
				_mappingCustomizations[key] = mappingCustomization;
			}

			mappingCustomization.ModifyFrameworkMapping(modification);
			SetCustomizedPropertyCore(key, mappingCustomization.Compose());
		}

		MappingCustomization CreateMappingCustomization(string key)
		{
			if (_mapper.TryGetValue(key, out var localMapping))
				return new(() => localMapping);

			return new(() => Chained?.GetCommand(key));
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

		sealed class MappingCustomization
		{
			Func<Command?> _frameworkMapping;

			public MappingCustomization(Func<Command?> frameworkMapping)
			{
				_frameworkMapping = frameworkMapping;
			}

			public List<Func<Command?, Command>> Customizations { get; } = new();

			public void ModifyFrameworkMapping(Func<Command?, Command> modification)
			{
				var previousMapping = CreateDynamicMapping(_frameworkMapping);
				var newMapping = modification(previousMapping);
				_frameworkMapping = () => newMapping;
			}

			public Command Compose()
			{
				var mapping = CreateDynamicMapping(_frameworkMapping);
				foreach (var customization in Customizations)
				{
					mapping = customization(mapping);
				}

				return mapping!;
			}

			static Command? CreateDynamicMapping(Func<Command?> mappingAccessor)
			{
				if (mappingAccessor() is null)
					return null;

				return (handler, view, args) => mappingAccessor()?.Invoke(handler, view, args);
			}
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

		internal CommandMapper<TVirtualView, TViewHandler> WithFrameworkMappingsSealed()
		{
			SealFrameworkMappings();
			return this;
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