using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;

namespace Microsoft.Maui.Platform
{
	internal static partial class Issue17389CoreTrace
	{
		internal enum Stage
		{
			ReceiverRegistered, RejectMissingManager, RejectMissingView,
			RejectTransparent, RejectDisabled, AcceptDirect, AcceptDescendant,
			RejectTarget, NativeTapAction, ManagedTapDispatch, CommandEntry,
			NativeAttached, DisposeRemoveSiteCompleted, DisposeNativeDisposing,
			CollectionRemoveSiteCompleted, CollectionNativeDisposing,
			NativeOwnerAtAdd, HitWithSuppliedTouch, HitRecognizerOwner, HitWithoutTouchAssociation
		}

		internal enum Receiver
		{
			Unknown, RedChild, RedParent, GreenChild, GreenParent, BlueBorder, PurpleContent
		}

		internal enum HitReturn { BaseNull, ExcludedSelf, ExcludedLayout, Returned }
		enum Counter
		{
			NonRedOrUnregistered, NullEvent, NonTouchesEvent, NoTouches,
			TouchSetTooLarge, NoUniqueBeganPointMatch, DuplicateHit,
			HitQueryLimit, RecognizerArrayTooLarge, UnknownRecognizer,
			AmbiguousBinding, BindingOverflow, StaleSample,
			NativeReadBudget, RepeatedAddOwnerSample, UntrackedAdd, BufferFullBeforeNativeRead
		}

		const int Capacity = 64;
		const int MaxBindings = 8;
		const int MaxQueries = 8;
		const int MaxQueriesPerView = 4;
		const int MaxNonNullHitReads = 256;
		const int MaxTouches = 4;
		const int MaxRecognizerArray = 8;
		static readonly object Sync = new();
		static readonly Entry[] Entries = new Entry[Capacity];
		static readonly Binding[] Bindings = new Binding[MaxBindings];
		static readonly QueryKey[] Queries = new QueryKey[MaxQueries];
		static readonly int[] Counters = new int[Enum.GetValues(typeof(Counter)).Length];
		static ConditionalWeakTable<object, Identity> _identities = new();
		static bool _recording;
		static int _count, _dropped, _nextId, _bindingCount, _queryCount, _hitReadCount;
		static long _generation;

		[Conditional("MACCATALYST")]
		internal static void Begin()
		{
			lock (Sync)
			{
				_generation++;
				_count = _dropped = _nextId = _bindingCount = _queryCount = _hitReadCount = 0;
				Array.Clear(Counters, 0, Counters.Length);
				_identities = new();
				_recording = true;
			}
		}

		[Conditional("MACCATALYST")]
		internal static void Record(Stage stage, object? manager = null, object? recognizer = null,
			object? view = null, object? handler = null, nint nativeRecognizerHandle = 0,
			nint touchHandle = 0, nint platformViewHandle = 0, Receiver receiver = Receiver.Unknown)
		{
			lock (Sync)
			{
				if (!_recording || Full())
					return;
				var entry = new Entry
				{
					ObservedUtcTicks = DateTime.UtcNow.Ticks,
					Stage = stage,
					Receiver = receiver,
					ManagerId = GetId(manager),
					RecognizerId = GetId(recognizer),
					ViewId = GetId(view),
					HandlerId = GetId(handler),
					NativeRecognizerHandle = (long)nativeRecognizerHandle,
					TouchHandle = (long)touchHandle,
					PlatformViewHandle = (long)platformViewHandle
				};
				if (stage == Stage.ReceiverRegistered && view != null &&
					_identities.TryGetValue(view, out var identity))
					identity.Role = receiver;
				Entries[_count++] = entry;
				if (stage == Stage.NativeAttached && IsRed(GetRole(view)))
					RegisterBinding(entry);
			}
		}

		internal static bool TryDrain(out string snapshot)
		{
			lock (Sync)
			{
				if (!_recording)
				{
					snapshot = "";
					return false;
				}
				_recording = false;
				var text = new StringBuilder();
				text.Append("TapAdmissionSnapshot scope=InitialEnabled emission=PurpleReceiverAfterExistingLog complete=false generation=")
					.Append(_generation).Append(" retained=").Append(_count).Append(" capacity=").Append(Capacity)
					.Append(" droppedAtCapacity=").Append(_dropped)
					.Append(" association=PerRecordSampleFlags touchAssociation=SuppliedTouchesArgumentAndUniqueBeganPointMatch unassociatedHitAssociation=SuppliedEventDecisionOnly physicalInputProven=false")
					.Append(" nonNullHitReadLimit=").Append(MaxNonNullHitReads)
					.Append(" touchSetLimit=").Append(MaxTouches)
					.Append(" nativeArrayLimit=").Append(MaxRecognizerArray)
					.Append(" hitQueryLimit=").Append(MaxQueries)
					.Append(" perViewHitQueryLimit=").Append(MaxQueriesPerView)
					.Append(" nativeBindingLimit=").Append(MaxBindings)
					.Append(" nonNullHitReadsObserved=").Append(_hitReadCount)
					.Append(" acceptedHitQueries=").Append(_queryCount)
					.Append(" registeredNativeBindings=").Append(_bindingCount);
				for (int i = 0; i < Counters.Length; i++)
					text.Append(' ').Append((Counter)i).Append('=').Append(Counters[i]);
				text.AppendLine();
				for (int i = 0; i < _count; i++)
				{
					var entry = Entries[i];
					text.Append("sequence=").Append(i + 1).Append(" stage=").Append(entry.Stage)
						.Append(" observedUtcTicks=").Append(entry.ObservedUtcTicks)
						.Append(" receiver=").Append(entry.Receiver)
						.Append(" managerManagedId=").Append(entry.ManagerId)
						.Append(" recognizerManagedId=").Append(entry.RecognizerId)
						.Append(" viewManagedId=").Append(entry.ViewId)
						.Append(" handlerManagedId=").Append(entry.HandlerId)
						.Append(" nativeRecognizerHandle=").Append(entry.NativeRecognizerHandle)
						.Append(" touchHandle=").Append(entry.TouchHandle)
						.Append(" platformViewNativeHandle=").Append(entry.PlatformViewHandle);
					if (entry.Stage >= Stage.NativeOwnerAtAdd)
					{
						bool hitSampled = entry.Stage is Stage.HitWithSuppliedTouch or Stage.HitWithoutTouchAssociation;
						text.Append(" hitSampled=").Append(hitSampled)
							.Append(" suppliedEventSampled=").Append(entry.Query > 0)
							.Append(" eventTouchSampled=").Append(entry.TouchSampled)
							.Append(" queryPointSampled=").Append(hitSampled)
							.Append(" addReceiverSampled=").Append(entry.Stage == Stage.NativeOwnerAtAdd)
							.Append(" nativeArraySampled=").Append(hitSampled)
							.Append(" query=").Append(entry.Query)
							.Append(" suppliedEventType=").Append(entry.EventType)
							.Append(" eventArgumentHandle=").Append(entry.EventHandle)
							.Append(" eventTimestamp=").Append(entry.EventTime.ToString("R", CultureInfo.InvariantCulture))
							.Append(" touchTimestamp=").Append(entry.TouchTime.ToString("R", CultureInfo.InvariantCulture))
							.Append(" touchPhase=").Append(entry.TouchPhase)
							.Append(" eventTouchCount=").Append(entry.EventTouchCount)
							.Append(" queryPoint=").Append(entry.X.ToString("R", CultureInfo.InvariantCulture))
							.Append(',').Append(entry.Y.ToString("R", CultureInfo.InvariantCulture))
							.Append(" touchPoint=").Append(entry.TouchX.ToString("R", CultureInfo.InvariantCulture))
							.Append(',').Append(entry.TouchY.ToString("R", CultureInfo.InvariantCulture))
							.Append(" hitReturn=").Append(entry.HitReturn)
							.Append(" baseHitHandle=").Append(entry.BaseHitHandle)
							.Append(" returnedHitHandle=").Append(entry.ReturnedHitHandle)
							.Append(" originalAddReceiverHandle=").Append(entry.AddReceiverHandle)
							.Append(" ownerSampled=").Append(entry.OwnerSampled)
							.Append(" sampledOwnerHandle=").Append(entry.OwnerHandle)
							.Append(" stateSampled=").Append(entry.StateSampled)
							.Append(" sampledNativeState=").Append(entry.NativeState)
							.Append(" recognizerEnabledSampled=").Append(entry.RecognizerEnabledSampled)
							.Append(" sampledRecognizerEnabled=").Append(entry.RecognizerEnabled)
							.Append(" nativeArrayCount=").Append(entry.NativeArrayCount)
							.Append(" nativeArrayTruncated=").Append(entry.ArrayTruncated)
							.Append(" viewStateSampled=").Append(entry.ViewStateSampled)
							.Append(" interactionOverride=").Append(entry.InteractionOverride)
							.Append(" nativeInteractionEnabled=").Append(entry.NativeInteractionEnabled)
							.Append(" nativeHidden=").Append(entry.NativeHidden)
							.Append(" nativeAlpha=").Append(entry.NativeAlpha.ToString("R", CultureInfo.InvariantCulture))
							.Append(" bounds=").Append(entry.BoundsX.ToString("R", CultureInfo.InvariantCulture))
							.Append(',').Append(entry.BoundsY.ToString("R", CultureInfo.InvariantCulture))
							.Append(',').Append(entry.BoundsWidth.ToString("R", CultureInfo.InvariantCulture))
							.Append(',').Append(entry.BoundsHeight.ToString("R", CultureInfo.InvariantCulture))
							.Append(" windowSampled=").Append(entry.WindowSampled)
							.Append(" windowHandle=").Append(entry.WindowHandle)
							.Append(" windowKeySampled=").Append(entry.WindowKeySampled)
							.Append(" windowIsKey=").Append(entry.WindowIsKey);
					}
					text.AppendLine();
				}
				_identities = new();
				snapshot = text.ToString();
				return true;
			}
		}

		static bool Full()
		{
			if (_count < Capacity)
				return false;
			if (_dropped != int.MaxValue)
				_dropped++;
			return true;
		}

		static int GetId(object? value) =>
			value is null ? 0 : _identities.GetValue(value, static _ => new Identity(++_nextId)).Value;

		static Receiver GetRole(object? view) =>
			view != null && _identities.TryGetValue(view, out var identity) ? identity.Role : Receiver.Unknown;

		static bool IsRed(Receiver role) => role is Receiver.RedChild or Receiver.RedParent;

		static void Increment(Counter counter)
		{
			if (Counters[(int)counter] != int.MaxValue)
				Counters[(int)counter]++;
		}

		static void RegisterBinding(Entry entry)
		{
			for (int i = 0; i < _bindingCount; i++)
				if (Bindings[i].NativeRecognizerHandle == entry.NativeRecognizerHandle &&
					Bindings[i].RecognizerId == entry.RecognizerId && Bindings[i].ManagerId == entry.ManagerId &&
					Bindings[i].ViewId == entry.ViewId && Bindings[i].HandlerId == entry.HandlerId)
					return;
			if (_bindingCount == MaxBindings)
			{
				Increment(Counter.BindingOverflow);
				return;
			}
			Bindings[_bindingCount++] = new Binding
			{
				NativeRecognizerHandle = entry.NativeRecognizerHandle,
				ManagerId = entry.ManagerId,
				RecognizerId = entry.RecognizerId,
				ViewId = entry.ViewId,
				HandlerId = entry.HandlerId
			};
		}

		static bool IsRecording()
		{
			lock (Sync)
				return _recording;
		}

		static bool TryRedTicket(object? view, out Ticket ticket)
		{
			lock (Sync)
			{
				ticket = default;
				if (!_recording)
					return false;
				if (!IsRed(GetRole(view)) || view == null || !_identities.TryGetValue(view, out var identity))
				{
					Increment(Counter.NonRedOrUnregistered);
					return false;
				}
				ticket = new Ticket { Generation = _generation, ViewId = identity.Value, Role = identity.Role };
				return true;
			}
		}

		static bool TryBinding(long handle, long generation, out Binding binding, bool atAdd = false)
		{
			lock (Sync)
			{
				binding = default;
				if (!_recording || generation != _generation)
					return false;
				int matches = 0;
				for (int i = 0; i < _bindingCount; i++)
					if (Bindings[i].NativeRecognizerHandle == handle)
					{
						binding = Bindings[i];
						matches++;
					}
				if (matches != 1)
				{
					Increment(matches == 0 ? (atAdd ? Counter.UntrackedAdd : Counter.UnknownRecognizer) : Counter.AmbiguousBinding);
					return false;
				}
				return true;
			}
		}

		static long CurrentGeneration()
		{
			lock (Sync)
				return _generation;
		}

		static bool ReserveNativeHitRead(Ticket ticket)
		{
			lock (Sync)
			{
				if (!_recording || ticket.Generation != _generation)
					return false;
				if (_hitReadCount == MaxNonNullHitReads)
				{
					Increment(Counter.NativeReadBudget);
					return false;
				}
				if (_count == Capacity)
				{
					Increment(Counter.BufferFullBeforeNativeRead);
					return false;
				}
				_hitReadCount++;
				return true;
			}
		}

		static bool ReserveAddOwnerRead(long generation, Binding binding)
		{
			lock (Sync)
			{
				if (!_recording || generation != _generation)
					return false;
				if (_count == Capacity)
				{
					Increment(Counter.BufferFullBeforeNativeRead);
					return false;
				}
				for (int i = 0; i < _bindingCount; i++)
					if (Bindings[i].NativeRecognizerHandle == binding.NativeRecognizerHandle &&
						Bindings[i].RecognizerId == binding.RecognizerId &&
						Bindings[i].ManagerId == binding.ManagerId &&
						Bindings[i].ViewId == binding.ViewId && Bindings[i].HandlerId == binding.HandlerId)
					{
						if (Bindings[i].AddOwnerSampled)
						{
							Increment(Counter.RepeatedAddOwnerSample);
							return false;
						}
						Bindings[i].AddOwnerSampled = true;
						return true;
					}
				return false;
			}
		}

		static void Count(Ticket ticket, Counter counter)
		{
			lock (Sync)
				if (_recording && ticket.Generation == _generation)
					Increment(counter);
		}

		static bool ReserveQuery(Ticket ticket, QueryKey key, out int query)
		{
			lock (Sync)
			{
				query = 0;
				if (!_recording || ticket.Generation != _generation)
					return false;
				for (int i = 0; i < _queryCount; i++)
					if (Queries[i].Equals(key))
					{
						Increment(Counter.DuplicateHit);
						return false;
					}
				int roleQueries = 0;
				for (int i = 0; i < _queryCount; i++)
					if (Queries[i].ViewId == ticket.ViewId)
						roleQueries++;
				if (_queryCount == MaxQueries || roleQueries == MaxQueriesPerView)
				{
					Increment(Counter.HitQueryLimit);
					return false;
				}
				Queries[_queryCount++] = key;
				query = _queryCount;
				return true;
			}
		}

		static void Publish(long generation, Entry entry)
		{
			lock (Sync)
			{
				if (!_recording || generation != _generation)
				{
					if (_recording)
						Increment(Counter.StaleSample);
					return;
				}
				if (!Full())
				{
					entry.ObservedUtcTicks = DateTime.UtcNow.Ticks;
					Entries[_count++] = entry;
				}
			}
		}

		sealed class Identity
		{
			internal readonly int Value;
			internal Receiver Role;
			internal Identity(int value) => Value = value;
		}

		struct Binding
		{
			internal long NativeRecognizerHandle;
			internal int ManagerId, RecognizerId, ViewId, HandlerId;
			internal bool AddOwnerSampled;
			internal Entry ToEntry(Stage stage) => new Entry
			{
				Stage = stage,
				ManagerId = ManagerId,
				RecognizerId = RecognizerId,
				ViewId = ViewId,
				HandlerId = HandlerId,
				NativeRecognizerHandle = NativeRecognizerHandle
			};
		}

		struct Ticket
		{
			internal long Generation;
			internal int ViewId;
			internal Receiver Role;
		}

		struct QueryKey
		{
			internal long EventHandle, TouchHandle, LayoutHandle, BaseHitHandle;
			internal double EventTime, TouchTime, X, Y;
			internal int ViewId;
			internal HitReturn ReturnKind;

			public QueryKey()
			{
				EventHandle = TouchHandle = LayoutHandle = BaseHitHandle = 0;
				EventTime = TouchTime = X = Y = 0;
				ViewId = 0;
				ReturnKind = default;
			}
		}

		struct Entry
		{
			internal long ObservedUtcTicks;
			internal Stage Stage;
			internal Receiver Receiver;
			internal int ManagerId, RecognizerId, ViewId, HandlerId;
			internal long NativeRecognizerHandle, TouchHandle, PlatformViewHandle;
			internal int Query, TouchPhase, NativeState, NativeArrayCount, EventTouchCount;
			internal long EventHandle, BaseHitHandle, ReturnedHitHandle, AddReceiverHandle, OwnerHandle;
			internal double EventTime, TouchTime, X, Y, TouchX, TouchY;
			internal HitReturn HitReturn;
			internal bool OwnerSampled, StateSampled, ArrayTruncated;
			internal int EventType;
			internal bool TouchSampled, RecognizerEnabledSampled, RecognizerEnabled;
			internal bool ViewStateSampled, InteractionOverride, NativeInteractionEnabled, NativeHidden;
			internal double NativeAlpha, BoundsX, BoundsY, BoundsWidth, BoundsHeight;
			internal bool WindowSampled, WindowKeySampled, WindowIsKey;
			internal long WindowHandle;

			public Entry()
			{
				ObservedUtcTicks = 0;
				Stage = default;
				Receiver = default;
				ManagerId = RecognizerId = ViewId = HandlerId = 0;
				NativeRecognizerHandle = TouchHandle = PlatformViewHandle = 0;
				Query = TouchPhase = NativeState = NativeArrayCount = EventTouchCount = 0;
				EventHandle = BaseHitHandle = ReturnedHitHandle = AddReceiverHandle = OwnerHandle = 0;
				EventTime = TouchTime = X = Y = TouchX = TouchY = 0;
				HitReturn = default;
				OwnerSampled = StateSampled = ArrayTruncated = false;
				EventType = 0;
				TouchSampled = RecognizerEnabledSampled = RecognizerEnabled = false;
				ViewStateSampled = InteractionOverride = NativeInteractionEnabled = NativeHidden = false;
				NativeAlpha = BoundsX = BoundsY = BoundsWidth = BoundsHeight = 0;
				WindowSampled = WindowKeySampled = WindowIsKey = false;
				WindowHandle = 0;
			}
		}
	}
}
