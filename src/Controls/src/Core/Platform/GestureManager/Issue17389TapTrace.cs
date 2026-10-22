using System.Diagnostics;
using CoreTrace = Microsoft.Maui.Platform.Issue17389CoreTrace;

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
			CommandEntry,
			NativeAttached,
			DisposeRemoveSiteCompleted,
			DisposeNativeDisposing,
			CollectionRemoveSiteCompleted,
			CollectionNativeDisposing
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

		[Conditional("MACCATALYST")]
		internal static void Begin() => CoreTrace.Begin();

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
			nint touchHandle = 0, nint platformViewHandle = 0, Receiver receiver = Receiver.Unknown) =>
			CoreTrace.Record((CoreTrace.Stage)(int)stage, manager, recognizer, view, handler,
				nativeRecognizerHandle, touchHandle, platformViewHandle, (CoreTrace.Receiver)(int)receiver);

		internal static bool TryDrain(out string snapshot) => CoreTrace.TryDrain(out snapshot);
	}
}
