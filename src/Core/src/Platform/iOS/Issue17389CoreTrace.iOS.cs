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
			var enabled = recognizer.Enabled;
			var entry = binding.ToEntry(Stage.NativeOwnerAtAdd);
			entry.AddReceiverHandle = (long)(nint)originalReceiver.Handle;
			entry.OwnerHandle = owner is null ? 0 : (long)(nint)owner.Handle;
			entry.OwnerSampled = entry.StateSampled = true;
			entry.NativeState = (int)state;
			entry.RecognizerEnabledSampled = true;
			entry.RecognizerEnabled = enabled;
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
			var eventType = suppliedEvent.Type;
			if (eventType != UIEventType.Touches)
			{
				Count(ticket, Counter.NonTouchesEvent);
				var unassociatedKey = new QueryKey
				{
					EventHandle = (long)(nint)suppliedEvent.Handle,
					EventTime = suppliedEvent.Timestamp,
					LayoutHandle = (long)(nint)layout.Handle,
					ViewId = ticket.ViewId,
					ReturnKind = returnKind,
					BaseHitHandle = baseResult is null ? 0 : (long)(nint)baseResult.Handle,
					X = (double)point.X,
					Y = (double)point.Y
				};
				CaptureQuery(layout, ticket, unassociatedKey, default, 0, false, (int)eventType);
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
			CaptureQuery(layout, ticket, key, matchedPoint, (int)touchCount, true, (int)eventType);
		}

		static void CaptureQuery(LayoutView layout, Ticket ticket, QueryKey key,
			CGPoint matchedPoint, int touchCount, bool touchSampled, int eventType)
		{
			if (!ReserveQuery(ticket, key, out var query))
				return;

			var bounds = layout.Bounds;
			var window = layout.Window;
			var nativeInteraction = layout.UserInteractionEnabled;
			var hidden = layout.Hidden;
			var alpha = layout.Alpha;
			bool windowIsKey = window?.IsKeyWindow ?? false;
			var recognizers = layout.GestureRecognizers;
			int count = recognizers?.Length ?? 0;
			bool truncated = count > MaxRecognizerArray;
			if (truncated)
				Count(ticket, Counter.RecognizerArrayTooLarge);
			var hit = new Entry
			{
				Stage = touchSampled ? Stage.HitWithSuppliedTouch : Stage.HitWithoutTouchAssociation,
				Receiver = ticket.Role,
				ViewId = ticket.ViewId,
				Query = query,
				EventHandle = key.EventHandle,
				EventTime = key.EventTime,
				TouchHandle = key.TouchHandle,
				TouchTime = key.TouchTime,
				TouchPhase = touchSampled ? (int)UITouchPhase.Began : 0,
				TouchSampled = touchSampled,
				EventType = eventType,
				EventTouchCount = (int)touchCount,
				PlatformViewHandle = key.LayoutHandle,
				X = key.X,
				Y = key.Y,
				TouchX = (double)matchedPoint.X,
				TouchY = (double)matchedPoint.Y,
				BaseHitHandle = key.BaseHitHandle,
				ReturnedHitHandle = key.ReturnKind == HitReturn.Returned ? key.BaseHitHandle : 0,
				HitReturn = key.ReturnKind,
				NativeArrayCount = count,
				ArrayTruncated = truncated,
				ViewStateSampled = true,
				InteractionOverride = layout.UserInteractionEnabledOverride,
				NativeInteractionEnabled = nativeInteraction,
				NativeHidden = hidden,
				NativeAlpha = (double)alpha,
				BoundsX = (double)bounds.X,
				BoundsY = (double)bounds.Y,
				BoundsWidth = (double)bounds.Width,
				BoundsHeight = (double)bounds.Height,
				WindowSampled = true,
				WindowHandle = window is null ? 0 : (long)(nint)window.Handle,
				WindowKeySampled = window is not null,
				WindowIsKey = windowIsKey
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
				var enabled = recognizer.Enabled;
				var entry = binding.ToEntry(Stage.HitRecognizerOwner);
				entry.Query = query;
				entry.EventHandle = key.EventHandle;
				entry.EventTime = key.EventTime;
				entry.TouchHandle = key.TouchHandle;
				entry.TouchTime = key.TouchTime;
				entry.TouchPhase = touchSampled ? (int)UITouchPhase.Began : 0;
				entry.TouchSampled = touchSampled;
				entry.EventType = eventType;
				entry.EventTouchCount = (int)touchCount;
				entry.PlatformViewHandle = key.LayoutHandle;
				entry.OwnerHandle = owner is null ? 0 : (long)(nint)owner.Handle;
				entry.OwnerSampled = entry.StateSampled = true;
				entry.NativeState = (int)state;
				entry.RecognizerEnabledSampled = true;
				entry.RecognizerEnabled = enabled;
				Publish(ticket.Generation, entry);
			}
		}
	}
}
