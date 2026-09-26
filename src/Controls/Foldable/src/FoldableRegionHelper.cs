using System;
using System.Collections.Generic;
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
		readonly Dictionary<TKey, TMonitor> _monitors = new Dictionary<TKey, TMonitor>();
		readonly Action<TMonitor> _disposeMonitor;

		public FoldableMonitorRegistry(Action<TMonitor> disposeMonitor)
		{
			_disposeMonitor = disposeMonitor;
		}

		public int Count => _monitors.Count;

		public TMonitor GetOrAdd(TKey key, Func<TMonitor> createMonitor)
		{
			if (_monitors.TryGetValue(key, out var monitor))
				return monitor;

			monitor = createMonitor();
			_monitors.Add(key, monitor);
			return monitor;
		}

		public bool Remove(TKey key)
		{
			if (!_monitors.Remove(key, out var monitor))
				return false;

			_disposeMonitor(monitor);
			return true;
		}

		public void Dispose()
		{
			foreach (var monitor in _monitors.Values)
				_disposeMonitor(monitor);

			_monitors.Clear();
		}
	}
}
