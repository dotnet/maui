using System.Diagnostics;
using CoreGraphics;
using UIKit;

namespace Microsoft.Maui.Platform
{
	internal static partial class Issue17389CoreTrace
	{
		[Conditional("MACCATALYST")]
		internal static void ObserveAddedOwner(UIView originalReceiver, UIGestureRecognizer recognizer)
		{
			if (!IsRecording())
				return;
			var generation = CurrentGeneration();
			if (!TryBinding((long)(nint)recognizer.Handle, generation, out var binding, atAdd: true) ||
				!ReserveAddOwnerRead(generation, binding))
				return;
			var owner = recognizer.View;
			var state = recognizer.State;
			var entry = binding.ToEntry(Stage.NativeOwnerAtAdd);
			entry.AddReceiverHandle = (long)(nint)originalReceiver.Handle;
			entry.OwnerHandle = owner is null ? 0 : (long)(nint)owner.Handle;
			entry.OwnerSampled = entry.StateSampled = true;
			entry.NativeState = (int)state;
			Publish(generation, entry);
		}

		[Conditional("MACCATALYST")]
		internal static void ObserveHit(LayoutView layout, CGPoint point, UIEvent? suppliedEvent,
			UIView? baseResult, HitReturn returnKind)
		{
			if (!IsRecording() || !TryRedTicket(layout.CrossPlatformLayout, out var ticket))
				return;
			if (suppliedEvent is null)
			{
				Count(ticket, Counter.NullEvent);
				return;
			}
			if (!ReserveNativeHitRead(ticket))
				return;
			if (suppliedEvent.Type != UIEventType.Touches)
			{
				Count(ticket, Counter.NonTouchesEvent);
				return;
			}
			var touches = suppliedEvent.AllTouches;
			if (touches is null)
			{
				Count(ticket, Counter.NoTouches);
				return;
			}
			var touchCount = touches.Count;
			if (touchCount == 0)
			{
				Count(ticket, Counter.NoTouches);
				return;
			}
			if (touchCount > MaxTouches)
			{
				Count(ticket, Counter.TouchSetTooLarge);
				return;
			}
			int matches = 0;
			long touchHandle = 0;
			double touchTime = 0;
			CGPoint matchedPoint = default;
			foreach (UITouch touch in touches)
			{
				if (touch.Phase != UITouchPhase.Began)
					continue;
				var location = touch.LocationInView(layout);
				if (location.X != point.X || location.Y != point.Y)
					continue;
				matches++;
				touchHandle = (long)(nint)touch.Handle;
				touchTime = touch.Timestamp;
				matchedPoint = location;
			}
			if (matches != 1)
			{
				Count(ticket, Counter.NoUniqueBeganPointMatch);
				return;
			}
			var key = new QueryKey
			{
				EventHandle = (long)(nint)suppliedEvent.Handle,
				EventTime = suppliedEvent.Timestamp,
				TouchHandle = touchHandle,
				TouchTime = touchTime,
				LayoutHandle = (long)(nint)layout.Handle,
				ViewId = ticket.ViewId,
				ReturnKind = returnKind,
				BaseHitHandle = baseResult is null ? 0 : (long)(nint)baseResult.Handle,
				X = (double)point.X,
				Y = (double)point.Y
			};
			if (!ReserveQuery(ticket, key, out var query))
				return;

			var recognizers = layout.GestureRecognizers;
			int count = recognizers?.Length ?? 0;
			bool truncated = count > MaxRecognizerArray;
			if (truncated)
				Count(ticket, Counter.RecognizerArrayTooLarge);
			var hit = new Entry
			{
				Stage = Stage.HitWithSuppliedTouch,
				Receiver = ticket.Role,
				ViewId = ticket.ViewId,
				Query = query,
				EventHandle = key.EventHandle,
				EventTime = key.EventTime,
				TouchHandle = touchHandle,
				TouchTime = touchTime,
				TouchPhase = (int)UITouchPhase.Began,
				EventTouchCount = (int)touchCount,
				PlatformViewHandle = key.LayoutHandle,
				X = (double)point.X,
				Y = (double)point.Y,
				TouchX = (double)matchedPoint.X,
				TouchY = (double)matchedPoint.Y,
				BaseHitHandle = key.BaseHitHandle,
				ReturnedHitHandle = returnKind == HitReturn.Returned && baseResult != null
					? key.BaseHitHandle : 0,
				HitReturn = returnKind,
				NativeArrayCount = count,
				ArrayTruncated = truncated
			};
			Publish(ticket.Generation, hit);
			if (recognizers is null || truncated)
				return;
			for (int i = 0; i < recognizers.Length; i++)
			{
				var recognizer = recognizers[i];
				if (!TryBinding((long)(nint)recognizer.Handle, ticket.Generation, out var binding))
					continue;
				var owner = recognizer.View;
				var state = recognizer.State;
				var entry = binding.ToEntry(Stage.HitRecognizerOwner);
				entry.Query = query;
				entry.EventHandle = key.EventHandle;
				entry.EventTime = key.EventTime;
				entry.TouchHandle = touchHandle;
				entry.TouchTime = touchTime;
				entry.TouchPhase = (int)UITouchPhase.Began;
				entry.EventTouchCount = (int)touchCount;
				entry.PlatformViewHandle = key.LayoutHandle;
				entry.OwnerHandle = owner is null ? 0 : (long)(nint)owner.Handle;
				entry.OwnerSampled = entry.StateSampled = true;
				entry.NativeState = (int)state;
				Publish(ticket.Generation, entry);
			}
		}
	}
}
