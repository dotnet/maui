using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Foldable
{
	internal readonly struct FoldableRegion
	{
		public FoldableRegion(Rect bounds, bool isActive)
		{
			Bounds = bounds;
			IsActive = isActive;
		}

		public Rect Bounds { get; }
		public bool IsActive { get; }
	}

	internal sealed class FoldableHingeAngleState
	{
		readonly object _lock = new object();
		TaskCompletionSource<int> _pendingAngle;
		double _angle;
		bool _hasValue;

		public Task<int> GetAngleAsync()
		{
			lock (_lock)
			{
				if (_hasValue)
					return Task.FromResult((int)Math.Round(_angle));

				_pendingAngle ??= new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
				return _pendingAngle.Task;
			}
		}

		public bool SetAngle(double angle)
		{
			TaskCompletionSource<int> pendingAngle;
			bool changed;

			lock (_lock)
			{
				changed = !_hasValue || _angle != angle;
				_angle = angle;
				_hasValue = true;
				pendingAngle = _pendingAngle;
				_pendingAngle = null;
			}

			pendingAngle?.TrySetResult((int)Math.Round(angle));
			return changed;
		}
	}

	internal static class FoldableRegionHelper
	{
		public static Rect GetActiveDivisionRegion(IReadOnlyList<FoldableRegion> regions, Rect viewBounds)
		{
			if (regions == null || viewBounds.Width <= 0 || viewBounds.Height <= 0)
				return Rect.Zero;

			for (int i = 0; i < regions.Count; i++)
			{
				var region = regions[i];
				if (!region.IsActive || !DividesView(region.Bounds, viewBounds))
					continue;

				return region.Bounds;
			}

			return Rect.Zero;
		}

		public static double RadiansToDegrees(double angleInRadians)
		{
			return angleInRadians * 180d / Math.PI;
		}

		static bool DividesView(Rect region, Rect viewBounds)
		{
			if (region.Width <= 0 || region.Height <= 0)
				return false;

			var intersection = Rect.Intersect(region, viewBounds);
			if (intersection.Width <= 0 || intersection.Height <= 0)
				return false;

			bool isVertical = region.Height > region.Width;
			if (isVertical)
				return region.X > viewBounds.Left && region.Right < viewBounds.Right;

			return region.Y > viewBounds.Top && region.Bottom < viewBounds.Bottom;
		}
	}

	internal sealed class FoldableMonitorRegistry<TKey, TMonitor> : IDisposable
		where TKey : class
		where TMonitor : class
	{
		readonly Dictionary<TKey, MonitorEntry> _monitors = new Dictionary<TKey, MonitorEntry>();
		readonly Action<TMonitor> _disposeMonitor;

		public FoldableMonitorRegistry(Action<TMonitor> disposeMonitor)
		{
			_disposeMonitor = disposeMonitor;
		}

		public int Count => _monitors.Count;

		public TMonitor GetOrAdd(
			TKey key,
			Func<TMonitor> createMonitor,
			Func<TMonitor, bool> replaceMonitor = null)
		{
			if (_monitors.TryGetValue(key, out var entry))
			{
				if (replaceMonitor?.Invoke(entry.Monitor) == true)
				{
					_disposeMonitor(entry.Monitor);
					entry.Monitor = createMonitor();
				}

				entry.ReferenceCount++;
				return entry.Monitor;
			}

			var monitor = createMonitor();
			_monitors.Add(key, new MonitorEntry(monitor));
			return monitor;
		}

		public bool Remove(TKey key)
		{
			if (!_monitors.TryGetValue(key, out var entry))
				return false;

			entry.ReferenceCount--;
			if (entry.ReferenceCount > 0)
				return true;

			_monitors.Remove(key);
			_disposeMonitor(entry.Monitor);
			return true;
		}

		public void ForEach(Action<TMonitor> action)
		{
			foreach (var entry in _monitors.Values)
				action(entry.Monitor);
		}

		public void Dispose()
		{
			foreach (var entry in _monitors.Values)
				_disposeMonitor(entry.Monitor);

			_monitors.Clear();
		}

		sealed class MonitorEntry
		{
			public MonitorEntry(TMonitor monitor)
			{
				Monitor = monitor;
				ReferenceCount = 1;
			}

			public TMonitor Monitor { get; set; }
			public int ReferenceCount { get; set; }
		}
	}
}
