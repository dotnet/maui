using System;
using System.Collections;
using System.Collections.Specialized;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.Gtk.Items
{
	// Scroll events, the remaining items threshold, scroll-to alignment, item sizing and reordering of the items controller.
	sealed partial class GtkItemsController
	{
		const uint ScrollEvaluationDelayMilliseconds = 32;

		bool _raiseScrolledQueued;

		// ------------------------------------------------------------------ scroll-to alignment

		/// <summary>
		/// Moves the scroll offset so that the widget of <paramref name="row"/> sits at the start, centre or end of the viewport. GTK scrolls
		/// the row into view asynchronously and only the bounds of a bound widget are known, so this retries until the row is laid out.
		/// </summary>
		void AlignRow(GtkRow row, ScrollToPosition position, int attemptsLeft)
		{
			GLib.Timeout.Add(16, () =>
			{
				if (_disposed || row.Removed)
					return false;

				return !TryAlign(row, position) && --attemptsLeft > 0;
			});
		}

		/// <summary>Aligns the widget of <paramref name="row"/> in the viewport.</summary>
		/// <returns><see langword="false"/> when the row has no laid-out widget yet.</returns>
		internal bool TryAlign(GtkRow row, ScrollToPosition position)
		{
			MauiGtkItemHost? host = null;
			foreach (var candidate in _bound)
			{
				if (ReferenceEquals(candidate.Row, row))
				{
					host = candidate;
					break;
				}
			}

			if (host is null || !host.IsDrawable || host.Height <= 0 || !host.ComputeBounds(_scroller, out var bounds))
				return false;

			bool horizontal = _list?.Orientation == global::Gtk.Orientation.Horizontal;
			var adjustment = horizontal ? _scroller.Hadjustment : _scroller.Vadjustment;
			double start = horizontal ? bounds.X : bounds.Y;
			double size = horizontal ? bounds.Width : bounds.Height;
			double viewport = horizontal ? _scroller.Width : _scroller.Height;

			double delta = position switch
			{
				ScrollToPosition.Start => start,
				ScrollToPosition.Center => start + size / 2 - viewport / 2,
				ScrollToPosition.End => start + size - viewport,
				_ => 0,
			};

			if (Math.Abs(delta) >= 0.5)
				adjustment.Value += delta;

			return true;
		}

		// ------------------------------------------------------------------ scroll events, threshold

		void OnAdjustmentValueChanged(object? sender, EventArgs args) => QueueScrollEvaluation(raiseScrolled: true);

		void OnAdjustmentBoundsChanged(object? sender, EventArgs args) => QueueScrollEvaluation(raiseScrolled: false);

		// Adjustments change inside GTK's allocation pass and the rows only move in the next one: the app's handlers run a couple of frames
		// later (and once for any number of changes), when the bounds of the rows match the offset.
		void QueueScrollEvaluation(bool raiseScrolled)
		{
			_raiseScrolledQueued |= raiseScrolled;
			if (_scrollEvaluationQueued || _disposed)
				return;

			_scrollEvaluationQueued = true;
			GLib.Timeout.Add(ScrollEvaluationDelayMilliseconds, () =>
			{
				_scrollEvaluationQueued = false;
				bool raise = _raiseScrolledQueued;
				_raiseScrolledQueued = false;
				EvaluateScroll(raise);
				return false;
			});
		}

		/// <summary>Judges the remaining items threshold again (it changed): the event may fire again if the end is within it.</summary>
		public void UpdateRemainingItemsThreshold()
		{
			_thresholdReached = false;
			QueueScrollEvaluation(raiseScrolled: false);
		}

		/// <summary>
		/// Raises <see cref="ItemsView.Scrolled"/> (when the offset changed and <paramref name="raiseScrolled"/>) and
		/// <see cref="ItemsView.RemainingItemsThresholdReached"/> (once each time the end of the list comes within the threshold).
		/// </summary>
		/// <param name="raiseScrolled">Whether a scroll offset change should be reported.</param>
		internal void EvaluateScroll(bool raiseScrolled)
		{
			if (_disposed || _source is null || !TryGetVisibleRange(out int first, out int center, out int last))
				return;

			double horizontal = _scroller.Hadjustment.Value;
			double vertical = _scroller.Vadjustment.Value;

			if (raiseScrolled && (horizontal != _lastHorizontalOffset || vertical != _lastVerticalOffset))
			{
				var args = new ItemsViewScrolledEventArgs
				{
					HorizontalDelta = horizontal - _lastHorizontalOffset,
					VerticalDelta = vertical - _lastVerticalOffset,
					HorizontalOffset = horizontal,
					VerticalOffset = vertical,
					FirstVisibleItemIndex = first,
					CenterItemIndex = center,
					LastVisibleItemIndex = last,
				};

				_lastHorizontalOffset = horizontal;
				_lastVerticalOffset = vertical;
				_view.SendScrolled(args);
			}

			int threshold = _view.RemainingItemsThreshold;
			if (threshold < 0)
			{
				_thresholdReached = false;
				return;
			}

			bool reached = _source.ItemCount - 1 - last <= threshold;
			if (reached && !_thresholdReached)
			{
				_thresholdReached = true;
				_view.SendRemainingItemsThresholdReached();
			}
			else if (!reached)
			{
				_thresholdReached = false;
			}
		}

		/// <summary>The indices (in items, not rows) of the first, centre and last item that intersect the viewport.</summary>
		bool TryGetVisibleRange(out int first, out int center, out int last)
		{
			first = center = last = -1;
			double width = _scroller.Width;
			double height = _scroller.Height;
			if (_source is null || width <= 0 || height <= 0)
				return false;

			int firstRow = int.MaxValue, lastRow = -1, centerRow = -1;
			double bestDistance = double.MaxValue;

			foreach (var host in _bound)
			{
				// A row GTK scrolled out of view stays bound with its old allocation, but it is no longer drawable.
				if (!host.IsDrawable || host.Row is not { Kind: GtkRowKind.Item } || host.ListItem is not { } listItem || !host.ComputeBounds(_scroller, out var bounds))
					continue;

				if (bounds.Width <= 0 || bounds.Height <= 0 || bounds.X >= width || bounds.X + bounds.Width <= 0 || bounds.Y >= height || bounds.Y + bounds.Height <= 0)
					continue;

				int position = (int)listItem.Position;
				firstRow = Math.Min(firstRow, position);
				lastRow = Math.Max(lastRow, position);

				double distance = Math.Abs(bounds.X + bounds.Width / 2 - width / 2) + Math.Abs(bounds.Y + bounds.Height / 2 - height / 2);
				if (distance < bestDistance)
				{
					bestDistance = distance;
					centerRow = position;
				}
			}

			if (lastRow < 0)
				return false;

			first = ItemIndexOfRow(firstRow);
			center = ItemIndexOfRow(centerRow);
			last = ItemIndexOfRow(lastRow);
			return true;
		}

		int ItemIndexOfRow(int position)
		{
			if (_source is null || !_source.HasGroupRows)
				return position;

			int index = 0;
			for (int i = 0; i < position && i < _source.Count; i++)
			{
				if (_source.RowAt(i).Kind == GtkRowKind.Item)
					index++;
			}

			return index;
		}

		// ------------------------------------------------------------------ item sizing strategy

		bool UsesFirstItemSize => _view is StructuredItemsView { ItemSizingStrategy: ItemSizingStrategy.MeasureFirstItem };

		double? SharedSizeFor(global::Gtk.Orientation orientation)
		{
			if (!UsesFirstItemSize || _list is null || orientation != _list.Orientation)
				return null;

			// A height depends on the width it was measured at: the cache is only valid for that viewport.
			if (_firstItemViewport != _scroller.Width)
			{
				_firstItemWidth = _firstItemHeight = null;
				return null;
			}

			return orientation == global::Gtk.Orientation.Vertical ? _firstItemHeight : _firstItemWidth;
		}

		void OnHostMeasured(MauiGtkItemHost host, global::Gtk.Orientation orientation, double size)
		{
			if (!UsesFirstItemSize || _list is null || _source is null || orientation != _list.Orientation || host.Row is not { Kind: GtkRowKind.Item } row)
				return;

			if (!ReferenceEquals(row, FirstItemRow()))
				return;

			var cached = orientation == global::Gtk.Orientation.Vertical ? _firstItemHeight : _firstItemWidth;
			if (cached == size && _firstItemViewport == _scroller.Width)
				return;

			if (orientation == global::Gtk.Orientation.Vertical)
				_firstItemHeight = size;
			else
				_firstItemWidth = size;
			_firstItemViewport = _scroller.Width;

			// The rows that were measured before the first one are asked again (not from inside this measure pass).
			GLib.Idle.Add(() =>
			{
				if (!_disposed)
				{
					foreach (var other in _bound)
						other.QueueResize();
				}

				return false;
			});
		}

		GtkRow? FirstItemRow()
		{
			for (int i = 0; _source is not null && i < _source.Count; i++)
			{
				if (_source.RowAt(i).Kind == GtkRowKind.Item)
					return _source.RowAt(i);
			}

			return null;
		}

		/// <summary>Forgets the size of the first item (the strategy, items, layout or template changed) and measures the rows again.</summary>
		public void InvalidateSharedSize()
		{
			_firstItemWidth = _firstItemHeight = null;
			foreach (var host in _bound)
				host.QueueResize();
		}

		// ------------------------------------------------------------------ reordering

		void InstallReorder(MauiGtkItemHost host)
		{
			var source = new global::Gtk.DragSource { Actions = Gdk.DragAction.Move };
			source.Prepare += (_, args) => args.RetVal = BeginDrag(host);
			source.DragEnd += (_, _) => _dragRow = null;
			host.AddController(source);

			var target = new global::Gtk.DropTarget(GLib.GType.String, Gdk.DragAction.Move);
			target.Enter += (_, args) => args.RetVal = CanDropOn(host) ? Gdk.DragAction.Move : (Gdk.DragAction)0;
			target.Motion += (_, args) => args.RetVal = CanDropOn(host) ? Gdk.DragAction.Move : (Gdk.DragAction)0;
			target.Dropped += (_, args) => args.RetVal = DropOn(host, args.X, args.Y);
			host.AddController(target);
		}

		bool CanReorder => _view is ReorderableItemsView { CanReorderItems: true };

		/// <summary>Starts dragging the row of <paramref name="host"/> when <see cref="ReorderableItemsView.CanReorderItems"/> is set.</summary>
		/// <param name="host">The row widget the drag starts on.</param>
		/// <returns>The drag payload, or <see langword="null"/> when the row cannot be dragged.</returns>
		internal Gdk.ContentProvider? BeginDrag(MauiGtkItemHost host)
		{
			if (!CanReorder || host.Row is not { Kind: GtkRowKind.Item } row)
				return null;

			_dragRow = row;
			return new Gdk.ContentProvider(new GLib.Value("maui-reorder"));
		}

		bool CanDropOn(MauiGtkItemHost host) => CanReorder && _dragRow is not null && host.Row is not null && !ReferenceEquals(host.Row, _dragRow);

		/// <summary>Drops the dragged row on <paramref name="host"/>: before it in the first half, after it in the second.</summary>
		/// <param name="host">The row widget the drop happened on.</param>
		/// <param name="x">The x coordinate in <paramref name="host"/>.</param>
		/// <param name="y">The y coordinate in <paramref name="host"/>.</param>
		internal bool DropOn(MauiGtkItemHost host, double x, double y)
		{
			var dragged = _dragRow;
			if (!CanDropOn(host) || dragged is null || host.Row is not { } target)
			{
				_dragRow = null;
				return false;
			}

			_dragRow = null;
			bool horizontal = _list is global::Gtk.ListView { Orientation: global::Gtk.Orientation.Horizontal };
			bool after = horizontal ? x > host.Width / 2.0 : y > host.Height / 2.0;
			return Reorder(dragged, target, after);
		}

		/// <summary>
		/// Moves the item of <paramref name="source"/> next to <paramref name="target"/> in the items source (or in the groups: within a group, or
		/// into another one when <see cref="ReorderableItemsView.CanMixGroups"/> is set; dropping on a group header inserts first, on a footer last)
		/// and raises <see cref="ReorderableItemsView.ReorderCompleted"/>.
		/// </summary>
		/// <param name="source">The row that moves.</param>
		/// <param name="target">The row it is dropped on.</param>
		/// <param name="after">Whether it goes after <paramref name="target"/> instead of before.</param>
		/// <returns><see langword="false"/> when the move is not possible (read-only or fixed-size source, groups not mixable, no change).</returns>
		internal bool Reorder(GtkRow source, GtkRow target, bool after)
		{
			if (_source is null || _view is not ReorderableItemsView reorderable || source.Kind != GtkRowKind.Item || ReferenceEquals(source, target))
				return false;

			IList? fromList, toList;
			int from, to;

			if (_view is GroupableItemsView { IsGrouped: true })
			{
				if (!reorderable.CanMixGroups && !ReferenceEquals(source.Group, target.Group))
					return false;

				fromList = source.Group as IList;
				toList = target.Group as IList;
				from = IndexInGroup(source);
				to = target.Kind switch
				{
					GtkRowKind.Item => IndexInGroup(target) + (after ? 1 : 0),
					GtkRowKind.GroupHeader => 0,
					_ => toList?.Count ?? 0,
				};
			}
			else
			{
				fromList = toList = _view.ItemsSource as IList;
				if (target.Kind != GtkRowKind.Item)
					return false;

				from = RowPosition(source);
				to = RowPosition(target) + (after ? 1 : 0);
			}

			if (fromList is null || toList is null || fromList.IsReadOnly || fromList.IsFixedSize || toList.IsReadOnly || toList.IsFixedSize
				|| from < 0 || from >= fromList.Count || to < 0)
				return false;

			if (ReferenceEquals(fromList, toList))
			{
				if (to > from)
					to--;
				if (to == from)
					return false;
			}

			var data = fromList[from];
			fromList.RemoveAt(from);
			toList.Insert(Math.Min(to, toList.Count), data);

			// A plain list does not tell the model; observable ones do.
			if (_view.ItemsSource is not INotifyCollectionChanged)
				UpdateItemsSource();

			reorderable.SendReorderCompleted();
			return true;
		}

		int RowPosition(GtkRow row)
		{
			for (int i = 0; _source is not null && i < _source.Count; i++)
			{
				if (ReferenceEquals(_source.RowAt(i), row))
					return i;
			}

			return -1;
		}

		int IndexInGroup(GtkRow row)
		{
			int index = 0;
			for (int i = 0; _source is not null && i < _source.Count; i++)
			{
				var other = _source.RowAt(i);
				if (ReferenceEquals(other, row))
					return index;

				if (other.Kind == GtkRowKind.Item && ReferenceEquals(other.Group, row.Group))
					index++;
			}

			return -1;
		}
	}
}
