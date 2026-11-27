using System;
using System.Collections.Generic;
using System.Threading;
using Command = System.Action<Microsoft.Maui.IElementHandler, Microsoft.Maui.IElement, object?>;

namespace Microsoft.Maui
{
	public abstract class CommandMapper : ICommandMapper
	{
		readonly Dictionary<string, Command> _mapper = new(StringComparer.Ordinal);
		readonly Dictionary<string, MappingCustomization> _mappingCustomizations = new(StringComparer.Ordinal);
		Dictionary<string, FrameworkCell>? _frameworkCells;
		bool _frameworkMappingsSealed;
		bool _preserveMappingCustomizations;

		CommandMapper? _chained;
		long _version;

		/// <summary>
		/// Changes whenever this mapper or a chained mapper changes. Versions only increase, so the sum
		/// identifies a state of the whole chain.
		/// </summary>
		internal long ChainVersion => Volatile.Read(ref _version) + (Chained?.ChainVersion ?? 0);

		public CommandMapper()
		{
		}

		public CommandMapper(CommandMapper chained)
		{
			Chained = chained;
		}

		private protected virtual void SetPropertyCore(string key, Command action)
		{
			// Replacing an existing mapping, or assigning any key after the built-in mappings are sealed, is
			// an app customization. Record it so a later (lazy) framework remap composes underneath it.
			if (!_preserveMappingCustomizations
				&& (_mapper.ContainsKey(key) || _frameworkMappingsSealed))
			{
				AddMappingCustomization(key, _ => action);
				return;
			}

			_mapper[key] = action;
			Interlocked.Increment(ref _version);
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
			var sequence = MappingCustomizationSequence.Next();
			if (!_mappingCustomizations.TryGetValue(key, out var mappingCustomization))
			{
				mappingCustomization = new MappingCustomization(this, key, sequence);
				_mappingCustomizations[key] = mappingCustomization;
			}

			mappingCustomization.AddCustomization(sequence, customization);
			SetCustomizedPropertyCore(key, mappingCustomization.Mapping);
		}

		/// <summary>
		/// Marks the commands added so far as the built-in framework commands; see the property mapper's equivalent.
		/// </summary>
		internal void SealFrameworkMappings()
		{
			_frameworkMappingsSealed = true;
			_frameworkCells ??= new(StringComparer.Ordinal);
			foreach (var key in new List<string>(_mapper.Keys))
			{
				if (_frameworkCells.ContainsKey(key) || _mappingCustomizations.ContainsKey(key))
					continue;

				var cell = new FrameworkCell(_mapper[key]);
				_frameworkCells[key] = cell;
				_mapper[key] = cell.Mapping;
			}

			Interlocked.Increment(ref _version);
		}

		internal void ModifyFrameworkMapping(string key, Func<Command?, Command> modification)
		{
			if (_frameworkCells is not null && _frameworkCells.TryGetValue(key, out var cell))
			{
				cell.Modify(modification);
				Interlocked.Increment(ref _version);
				return;
			}

			if (!_mappingCustomizations.TryGetValue(key, out var mappingCustomization))
			{
				mappingCustomization = new MappingCustomization(this, key, MappingCustomizationSequence.Framework);
				_mappingCustomizations[key] = mappingCustomization;
			}

			mappingCustomization.ModifyFrameworkMapping(modification);
			SetCustomizedPropertyCore(key, mappingCustomization.Mapping);
		}

		/// <summary>
		/// Gets the command for <paramref name="key"/> as it was after the app customizations recorded up to
		/// <paramref name="sequence"/>, including all framework mappings and remaps.
		/// </summary>
		internal Command? GetCommandAsOf(string key, long sequence)
		{
			if (_mappingCustomizations.TryGetValue(key, out var mappingCustomization))
				return mappingCustomization.GetMappingAsOf(sequence);

			// Honor GetCommand overrides; only an inherited chained command has an earlier state.
			var command = GetCommand(key);
			if (command is not null && !_mapper.ContainsKey(key) && ReferenceEquals(command, Chained?.GetCommand(key)))
				return Chained!.GetCommandAsOf(key, sequence);

			return command;
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
			if (_mappingCustomizations.TryGetValue(key, out var mappingCustomization))
				return mappingCustomization.GetCurrentMapping();

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
			set
			{
				_chained = value;
				Interlocked.Increment(ref _version);
			}
		}

		/// <summary>
		/// A built-in framework command of a sealed mapper, changed in place by lazy framework remaps.
		/// </summary>
		sealed class FrameworkCell
		{
			volatile Command _current;

			public FrameworkCell(Command command)
			{
				_current = command;
				Mapping = (handler, view, args) => _current(handler, view, args);
			}

			public Command Mapping { get; }

			public void Modify(Func<Command?, Command> modification) =>
				_current = modification(_current);
		}

		/// <summary>
		/// The framework command of one key and the app customizations composed on top of it.
		/// </summary>
		/// <remarks>
		/// See the property mapper's equivalent for the ordering rules: framework remaps compose underneath
		/// recorded customizations, and a chained command only reflects chained app customizations recorded
		/// before this key was first customized.
		/// </remarks>
		sealed class MappingCustomization
		{
			readonly CommandMapper _owner;
			readonly string _key;
			readonly Command? _localMapping;
			readonly bool _hasLocalMapping;
			readonly List<Func<Command?, Command>> _frameworkModifications = new();
			readonly List<long> _sequences = new();
			readonly List<Func<Command?, Command>> _customizations = new();
			long _chainedMappingSequence;
			volatile Compositions? _compositions;
			volatile Snapshot? _snapshot;

			public MappingCustomization(CommandMapper owner, string key, long chainedMappingSequence)
			{
				_owner = owner;
				_key = key;
				// Start from the command GetCommand reports, which honors overrides. Only an inherited command is
				// resolved through the chained mapper, at the sequence of the customization.
				var command = owner.GetCommand(key);
				_hasLocalMapping = owner._mapper.ContainsKey(key)
					|| (command is not null && !ReferenceEquals(command, owner.Chained?.GetCommand(key)));
				_localMapping = _hasLocalMapping ? command : null;

				_chainedMappingSequence = chainedMappingSequence;
				Mapping = (handler, view, args) =>
				{
					var mappings = GetCompositions().Mappings;
					mappings[mappings.Length - 1]?.Invoke(handler, view, args);
				};
			}

			/// <summary>
			/// The complete command, stored in the mapper so the key stays registered. It always dispatches to the
			/// latest composition and is never returned to callers.
			/// </summary>
			public Command Mapping { get; }

			/// <summary>
			/// Gets the command including the app customizations recorded so far; see the property mapper's equivalent.
			/// </summary>
			public Command GetCurrentMapping()
			{
				var sequence = MappingCustomizationSequence.Current;
				var snapshot = _snapshot;
				if (snapshot is not null && snapshot.Sequence == sequence)
					return snapshot.Mapping;

				snapshot = new Snapshot(sequence, (handler, view, args) => GetMappingAsOf(sequence)?.Invoke(handler, view, args));
				_snapshot = snapshot;
				return snapshot.Mapping;
			}

			public void AddCustomization(long sequence, Func<Command?, Command> customization)
			{
				_sequences.Add(sequence);
				_customizations.Add(customization);
				_compositions = null;
			}

			public void ModifyFrameworkMapping(Func<Command?, Command> modification)
			{
				_chainedMappingSequence = MappingCustomizationSequence.Framework;
				_frameworkModifications.Add(modification);
				_compositions = null;
			}

			public Command? GetMappingAsOf(long sequence)
			{
				var count = 0;
				while (count < _sequences.Count && _sequences[count] <= sequence)
					count++;

				// Before this key's first customization, an inherited command is the chained one at that time.
				if (count == 0 && _frameworkModifications.Count == 0 && !_hasLocalMapping)
					return _owner.Chained?.GetCommandAsOf(_key, sequence);

				return GetCompositions().Mappings[count];
			}

			Compositions GetCompositions()
			{
				var compositions = _compositions;
				var version = _owner.ChainVersion;
				if (compositions is not null && compositions.Version == version)
					return compositions;

				var mappings = new Command?[_customizations.Count + 1];
				var mapping = _hasLocalMapping ? _localMapping : _owner.Chained?.GetCommandAsOf(_key, _chainedMappingSequence);
				foreach (var modification in _frameworkModifications)
				{
					mapping = modification(mapping);
				}

				mappings[0] = mapping;
				for (var i = 0; i < _customizations.Count; i++)
				{
					mapping = _customizations[i](mapping);
					mappings[i + 1] = mapping;
				}

				compositions = new Compositions(mappings, version);
				_compositions = compositions;
				return compositions;
			}

			sealed class Snapshot
			{
				public Snapshot(long sequence, Command mapping)
				{
					Sequence = sequence;
					Mapping = mapping;
				}

				public long Sequence { get; }

				public Command Mapping { get; }
			}

			sealed class Compositions
			{
				public Compositions(Command?[] mappings, long version)
				{
					Mappings = mappings;
					Version = version;
				}

				public Command?[] Mappings { get; }

				public long Version { get; }
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
				_ = GetCommand(key) ?? throw new IndexOutOfRangeException($"Unable to find mapping for '{nameof(key)}'.");

				// See the property mapper indexer: resolve on invocation so lazy framework remaps are included.
				var sequence = MappingCustomizationSequence.Current;
				return new Action<TViewHandler, TVirtualView, object?>((h, v, o) => GetCommandAsOf(key, sequence)?.Invoke(h, v, o));
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