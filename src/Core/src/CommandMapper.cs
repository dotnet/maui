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
		Dictionary<string, InheritedCapture>? _inheritedCaptures;
		bool _frameworkMappingsSealed;
		bool _preserveMappingCustomizations;

		CommandMapper? _chained;
		long _version;

		// Every change stamps the changed mapper with the next value of this counter, which is shared by all
		// command mappers.
		static long s_lastVersion;

		// Nonzero while the mapper resolves commands to invoke or compose them; see the property mapper's equivalent.
		[ThreadStatic]
		static int t_directResolution;

		/// <summary>
		/// Identifies a state of this mapper and its chain; see the property mapper's equivalent.
		/// </summary>
		internal long ChainVersion => Math.Max(Volatile.Read(ref _version), Chained?.ChainVersion ?? 0);

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
				// The action already went through this method, so an override's transformation applies once.
				AddMappingCustomization(key, _ => action, transformed: true);
				return;
			}

			_mapper[key] = action;
			UpdateVersion();
		}

		void UpdateVersion() => Volatile.Write(ref _version, Interlocked.Increment(ref s_lastVersion));

		void StoreMapping(string key, Command action)
		{
			_mapper[key] = action;
			UpdateVersion();
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

		internal void AddMappingCustomization(string key, Func<Command?, Command> customization, bool transformed = false)
		{
			var sequence = MappingCustomizationSequence.Next();
			if (!_mappingCustomizations.TryGetValue(key, out var mappingCustomization))
			{
				mappingCustomization = new MappingCustomization(this, key, sequence);
				_mappingCustomizations[key] = mappingCustomization;
			}

			mappingCustomization.AddCustomization(sequence, customization);
			mappingCustomization.Register(transformed);
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

			UpdateVersion();
		}

		internal void ModifyFrameworkMapping(string key, Func<Command?, Command> modification)
		{
			if (_frameworkCells is not null && _frameworkCells.TryGetValue(key, out var cell))
			{
				cell.Modify(modification);
				UpdateVersion();
				return;
			}

			if (!_mappingCustomizations.TryGetValue(key, out var mappingCustomization))
			{
				// A remap of a key without a framework cell builds on the chained command as it is now, like the
				// remaps that captured the previous command when they ran at startup.
				mappingCustomization = new MappingCustomization(this, key, MappingCustomizationSequence.Current);
				_mappingCustomizations[key] = mappingCustomization;
			}

			mappingCustomization.ModifyFrameworkMapping(modification);
			mappingCustomization.Register();
		}

		/// <summary>
		/// Gets the command for <paramref name="key"/> as it was after the app customizations recorded up to
		/// <paramref name="sequence"/>, including all framework mappings and remaps.
		/// </summary>
		internal Command? GetCommandAsOf(string key, long sequence)
		{
			if (_mappingCustomizations.TryGetValue(key, out var mappingCustomization))
			{
				return mappingCustomization.TryGetMappingAsOf(sequence, out var mapping)
					? mapping
					: mappingCustomization.Chained?.GetCommandAsOf(key, sequence);
			}

			// Honor GetCommand overrides; only an inherited chained command has an earlier state.
			var command = GetCommandDirect(key);
			if (command is not null && !_mapper.ContainsKey(key) && ReferenceEquals(command, GetChainedCommandDirect(key)))
				return Chained!.GetCommandAsOf(key, sequence);

			return command;
		}

		Command? GetInheritedCommandAsOf(string key, long sequence, CommandMapper? chained)
		{
			if (_mappingCustomizations.TryGetValue(key, out var mappingCustomization)
				&& mappingCustomization.TryGetMappingAsOf(sequence, out var mapping))
			{
				return mapping;
			}

			return chained?.GetCommandAsOf(key, sequence);
		}

		Command? GetCommandDirect(string key)
		{
			t_directResolution++;
			try
			{
				return GetCommand(key);
			}
			finally
			{
				t_directResolution--;
			}
		}

		Command? GetChainedCommandDirect(string key) => Chained?.GetCommandDirect(key);

		internal bool HasLocalCommand(string key) => _mapper.ContainsKey(key);

		/// <summary>
		/// Gets a command for an inherited key that is bound to this mapper; see the property mapper's equivalent.
		/// </summary>
		Command CaptureInheritedCommand(string key)
		{
			var sequence = MappingCustomizationSequence.Current;
			var chained = Chained;
			lock (_mappingCustomizations)
			{
				_inheritedCaptures ??= new(StringComparer.Ordinal);
				if (_inheritedCaptures.TryGetValue(key, out var capture)
					&& capture.Sequence == sequence
					&& ReferenceEquals(capture.Chained, chained))
				{
					return capture.Mapping;
				}

				capture = new InheritedCapture(this, key, sequence, chained);
				_inheritedCaptures[key] = capture;
				return capture.Mapping;
			}
		}

		private protected virtual void InvokeCore(string key, IElementHandler viewHandler, IElement virtualView, object? args)
		{
			if (!viewHandler.CanInvokeMappers())
				return;

			var action = GetCommandDirect(key);
			action?.Invoke(viewHandler, virtualView, args);
		}

		public virtual Command? GetCommand(string key)
		{
			// A customized key holds the registration that includes the app customizations recorded so far.
			if (_mapper.TryGetValue(key, out var action))
				return action;

			if (Chained is null)
				return null;

			if (!_frameworkMappingsSealed || t_directResolution > 0)
				return Chained.GetCommand(key);

			// See the property mapper: a wrapped inherited command still observes later remaps of the key here.
			return GetChainedCommandDirect(key) is null ? null : CaptureInheritedCommand(key);
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
				UpdateVersion();
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
		/// recorded customizations, and an inherited command is resolved from the chained mapper the owner had when
		/// the record was created, only reflecting its app customizations recorded before then.
		/// </remarks>
		sealed class MappingCustomization
		{
			readonly CommandMapper _owner;
			readonly string _key;
			readonly Command? _localMapping;
			readonly bool _hasLocalMapping;
			readonly long _chainedMappingSequence;
			readonly List<Func<Command?, Command>> _frameworkModifications = new();
			readonly List<long> _sequences = new();
			readonly List<Func<Command?, Command>> _customizations = new();
			readonly List<Command?> _registeredMappings = new();
			volatile Compositions? _compositions;
			int _revision;

			public MappingCustomization(CommandMapper owner, string key, long chainedMappingSequence)
			{
				_owner = owner;
				_key = key;
				// Start from the command GetCommand reports, which honors overrides. Only an inherited command is
				// resolved through the chained mapper, at the sequence of the record.
				var command = owner.GetCommandDirect(key);
				_hasLocalMapping = owner._mapper.ContainsKey(key)
					|| (command is not null && !ReferenceEquals(command, owner.GetChainedCommandDirect(key)));
				_localMapping = _hasLocalMapping ? command : null;
				Chained = owner.Chained;
				_chainedMappingSequence = chainedMappingSequence;
			}

			/// <summary>
			/// The chained mapper an inherited command is resolved from.
			/// </summary>
			public CommandMapper? Chained { get; }

			public void AddCustomization(long sequence, Func<Command?, Command> customization)
			{
				_sequences.Add(sequence);
				_customizations.Add(customization);
				Interlocked.Increment(ref _revision);
				_compositions = null;
			}

			public void ModifyFrameworkMapping(Func<Command?, Command> modification)
			{
				_frameworkModifications.Add(modification);
				Interlocked.Increment(ref _revision);
				_compositions = null;
			}

			/// <summary>
			/// Registers the command that includes every customization recorded so far; see the property mapper's
			/// equivalent.
			/// </summary>
			public void Register(bool transformed = false)
			{
				var index = _customizations.Count;
				Command mapping = (handler, view, args) => GetMappingAt(index)?.Invoke(handler, view, args);
				if (transformed)
					_owner.StoreMapping(_key, mapping);
				else
					_owner.SetCustomizedPropertyCore(_key, mapping);

				while (_registeredMappings.Count <= index)
					_registeredMappings.Add(null);

				_registeredMappings[index] = _owner._mapper.TryGetValue(_key, out var registered) ? registered : null;
			}

			public bool TryGetMappingAsOf(long sequence, out Command? mapping)
			{
				var count = 0;
				while (count < _sequences.Count && _sequences[count] <= sequence)
					count++;

				if (count == 0 && _frameworkModifications.Count == 0 && !_hasLocalMapping)
				{
					mapping = null;
					return false;
				}

				mapping = (count < _registeredMappings.Count ? _registeredMappings[count] : null)
					?? GetMappingAt(count);
				return true;
			}

			Command? GetMappingAt(int index)
			{
				var mappings = GetCompositions().Mappings;
				return index < mappings.Length ? mappings[index] : null;
			}

			Compositions GetCompositions()
			{
				var compositions = _compositions;
				var revision = Volatile.Read(ref _revision);
				var version = Chained?.ChainVersion ?? 0;
				if (compositions is not null && compositions.Version == version && compositions.Revision == revision)
					return compositions;

				// Read the counts once: another thread may record a customization while this one composes, and
				// the revision then makes the next caller compose again.
				var modificationCount = _frameworkModifications.Count;
				var customizationCount = _customizations.Count;
				var mappings = new Command?[customizationCount + 1];
				var mapping = _hasLocalMapping ? _localMapping : Chained?.GetCommandAsOf(_key, _chainedMappingSequence);
				for (var i = 0; i < modificationCount; i++)
				{
					mapping = _frameworkModifications[i](mapping);
				}

				mappings[0] = mapping;
				for (var i = 0; i < customizationCount; i++)
				{
					mapping = _customizations[i](mapping);
					mappings[i + 1] = mapping;
				}

				compositions = new Compositions(mappings, version, revision);
				_compositions = compositions;
				return compositions;
			}

			sealed class Compositions
			{
				public Compositions(Command?[] mappings, long version, int revision)
				{
					Mappings = mappings;
					Version = version;
					Revision = revision;
				}

				public Command?[] Mappings { get; }

				public long Version { get; }

				public int Revision { get; }
			}
		}

		/// <summary>
		/// An inherited command of a sealed mapper handed to a caller, bound to the state it was read in.
		/// </summary>
		sealed class InheritedCapture
		{
			readonly CommandMapper _owner;
			readonly string _key;

			public InheritedCapture(CommandMapper owner, string key, long sequence, CommandMapper? chained)
			{
				_owner = owner;
				_key = key;
				Sequence = sequence;
				Chained = chained;
				Mapping = Invoke;
			}

			public long Sequence { get; }

			public CommandMapper? Chained { get; }

			public Command Mapping { get; }

			void Invoke(IElementHandler handler, IElement view, object? args) =>
				_owner.GetInheritedCommandAsOf(_key, Sequence, Chained)?.Invoke(handler, view, args);
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
				var command = GetCommand(key) ?? throw new IndexOutOfRangeException($"Unable to find mapping for '{nameof(key)}'.");

				// See the property mapper indexer: an inherited command is used as GetCommand reports it, and a local
				// one resolves on invocation so lazy framework remaps are included.
				if (!HasLocalCommand(key))
					return new Action<TViewHandler, TVirtualView, object?>((h, v, o) => command(h, v, o));

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