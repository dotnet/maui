using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace Microsoft.Maui.Controls.Platform
{
	internal static class Issue17389TapTrace
	{
		internal enum Stage
		{
			ReceiverRegistered,
			RejectMissingManager,
			RejectMissingView,
			RejectTransparent,
			RejectDisabled,
			AcceptDirect,
			AcceptDescendant,
			RejectTarget,
			NativeTapAction,
			ManagedTapDispatch,
			CommandEntry
		}

		internal enum Receiver
		{
			Unknown,
			RedChild,
			RedParent,
			GreenChild,
			GreenParent,
			BlueBorder,
			PurpleContent
		}

		const int Capacity = 64;
		static readonly object Sync = new();
		static readonly Entry[] Entries = new Entry[Capacity];
		static ConditionalWeakTable<object, Identity> _identities = new();
		static bool _recording;
		static int _count, _dropped, _nextId;
		static long _generation;

		[Conditional("MACCATALYST")]
		internal static void Begin()
		{
			lock (Sync)
			{
				_generation++;
				_count = _dropped = _nextId = 0;
				_identities = new();
				_recording = true;
			}
		}

		[Conditional("MACCATALYST")]
		internal static void RegisterReceiver(object view, string receiver)
		{
			var role = receiver switch
			{
				"RedGrid.Child" => Receiver.RedChild,
				"RedGrid.Parent" => Receiver.RedParent,
				"GreenGrid.Child" => Receiver.GreenChild,
				"GreenGrid.Parent" => Receiver.GreenParent,
				"BlueBorder" => Receiver.BlueBorder,
				"PurpleContent" => Receiver.PurpleContent,
				_ => Receiver.Unknown
			};
			Record(Stage.ReceiverRegistered, view: view, receiver: role);
		}

		[Conditional("MACCATALYST")]
		internal static void Record(Stage stage, object? manager = null, object? recognizer = null,
			object? view = null, object? handler = null, nint nativeRecognizerHandle = 0,
			nint touchHandle = 0, nint platformViewHandle = 0, Receiver receiver = Receiver.Unknown)
		{
			lock (Sync)
			{
				if (!_recording)
					return;
				if (_count == Capacity)
				{
					if (_dropped != int.MaxValue)
						_dropped++;
					return;
				}
				Entries[_count++] = new Entry
				{
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
					.Append(" droppedAtCapacity=").Append(_dropped).AppendLine();
				for (int i = 0; i < _count; i++)
				{
					var entry = Entries[i];
					text.Append("sequence=").Append(i + 1).Append(" stage=").Append(entry.Stage)
						.Append(" receiver=").Append(entry.Receiver)
						.Append(" managerManagedId=").Append(entry.ManagerId)
						.Append(" recognizerManagedId=").Append(entry.RecognizerId)
						.Append(" viewManagedId=").Append(entry.ViewId)
						.Append(" handlerManagedId=").Append(entry.HandlerId)
						.Append(" nativeRecognizerHandle=").Append(entry.NativeRecognizerHandle)
						.Append(" touchHandle=").Append(entry.TouchHandle)
						.Append(" platformViewNativeHandle=").Append(entry.PlatformViewHandle).AppendLine();
				}
				_identities = new();
				snapshot = text.ToString();
				return true;
			}
		}

		static int GetId(object? value) =>
			value is null ? 0 : _identities.GetValue(value, static _ => new Identity(++_nextId)).Value;

		sealed class Identity
		{
			internal readonly int Value;
			internal Identity(int value) => Value = value;
		}

		struct Entry
		{
			internal Stage Stage;
			internal Receiver Receiver;
			internal int ManagerId, RecognizerId, ViewId, HandlerId;
			internal long NativeRecognizerHandle, TouchHandle, PlatformViewHandle;
		}
	}
}
