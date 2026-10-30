using System;
using System.Collections.Specialized;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Gtk.Platform;
using GtkWidget = Gtk.Widget;

namespace Microsoft.Maui.Controls.Gtk.Platform
{
	/// <summary>
	/// Connects the <see cref="View.GestureRecognizers"/> of a view to GTK 4 event controllers on its platform widget (or container):
	/// tap and double tap (<c>GestureClick</c>), pan and swipe (<c>GestureDrag</c>), pinch (<c>GestureZoom</c>), pointer
	/// (<c>EventControllerMotion</c> + <c>GestureClick</c>) and drag and drop (<c>DragSource</c>/<c>DropTarget</c>, text payloads).
	/// </summary>
	/// <remarks>
	/// <para>Controllers are added only for the recognizer kinds the view has, and removed again when the collection changes. They never
	/// claim an event sequence, so the widget's own behaviour (a button click, text selection) still runs.</para>
	/// <para>Dragging an image or any other payload than text is not supported; an in-process drop gets the <see cref="DataPackage"/> of the
	/// drag source unchanged (text, image and properties).</para>
	/// </remarks>
	sealed class GtkGestureConnection : IDisposable
	{
		/// <summary>Movement, in pixels, before a drag counts as a pan.</summary>
		const double PanThreshold = 4;

		/// <summary>The data package of the drag in progress in this process.</summary>
		static DataPackage? s_activeDrag;

		/// <summary>The data package of the drag in progress, or <see langword="null"/>; for tests.</summary>
		internal static DataPackage? ActiveDrag => s_activeDrag;

		readonly GtkWidget _widget;
		readonly View _view;
		readonly INotifyCollectionChanged? _collection;

		global::Gtk.GestureClick? _click;
		global::Gtk.GestureDrag? _drag;
		global::Gtk.GestureZoom? _zoom;
		global::Gtk.EventControllerMotion? _motion;
		global::Gtk.DragSource? _dragSource;
		global::Gtk.DropTarget? _dropTarget;

		bool _panStarted;
		bool _pinchStarted;
		double _lastScale = 1;
		double _pointerX;
		double _pointerY;
		bool _disposed;

		static readonly System.Runtime.CompilerServices.ConditionalWeakTable<GtkWidget, GtkGestureConnection> s_connections = new();

		/// <summary>The connection attached to <paramref name="widget"/>, or <see langword="null"/>; for tests.</summary>
		/// <param name="widget">The widget.</param>
		internal static GtkGestureConnection? For(GtkWidget widget) => s_connections.TryGetValue(widget, out var connection) ? connection : null;

		GtkGestureConnection(GtkWidget widget, View view)
		{
			_widget = widget;
			_view = view;
			s_connections.Remove(widget);
			s_connections.Add(widget, this);
			_collection = view.GestureRecognizers as INotifyCollectionChanged;
			if (_collection is not null)
				_collection.CollectionChanged += OnRecognizersChanged;

			Refresh();
		}

		/// <summary>Creates the connection for a handler, or <see langword="null"/> when its platform view is not a GTK widget or a view.</summary>
		/// <param name="handler">The view handler.</param>
		public static IDisposable? Create(IViewHandler handler)
		{
			if ((handler.ContainerView ?? handler.PlatformView) is not GtkWidget widget || handler.VirtualView is not View view)
				return null;

			return new GtkGestureConnection(widget, view);
		}

		/// <summary>Attached controllers, for tests.</summary>
		internal int ControllerCount =>
			(_click is null ? 0 : 1) + (_drag is null ? 0 : 1) + (_zoom is null ? 0 : 1) + (_motion is null ? 0 : 1) + (_dragSource is null ? 0 : 1) + (_dropTarget is null ? 0 : 1);

		void OnRecognizersChanged(object? sender, NotifyCollectionChangedEventArgs e) => Refresh();

		bool Has<T>() where T : GestureRecognizer => _view.GestureRecognizers.GetGesturesFor<T>().Any();

		/// <summary>Adds the controllers the current recognizers need and removes the ones they no longer need.</summary>
		void Refresh()
		{
			if (_disposed)
				return;

			bool click = Has<TapGestureRecognizer>() || Has<PointerGestureRecognizer>();
			Toggle(ref _click, click, CreateClick);
			Toggle(ref _drag, Has<PanGestureRecognizer>() || Has<SwipeGestureRecognizer>(), CreateDrag);
			Toggle(ref _zoom, Has<PinchGestureRecognizer>(), CreateZoom);
			Toggle(ref _motion, Has<PointerGestureRecognizer>(), CreateMotion);
			Toggle(ref _dragSource, Has<DragGestureRecognizer>(), CreateDragSource);
			Toggle(ref _dropTarget, Has<DropGestureRecognizer>(), CreateDropTarget);
		}

		void Toggle<T>(ref T? controller, bool wanted, Func<T> create)
			where T : global::Gtk.EventController
		{
			if (wanted && controller is null)
			{
				controller = create();
				_widget.AddController(controller);
			}
			else if (!wanted && controller is not null)
			{
				_widget.RemoveController(controller);
				controller = null;
			}
		}

		// ------------------------------------------------------------------ position

		Point? GetPosition(IElement? relativeTo, double x, double y)
		{
			var point = GtkInterop.ToGraphene(x, y);

			if (relativeTo is null)
				return _widget.Root as GtkWidget is { } root && _widget.ComputePoint(root, point, out var inRoot) ? new Point(inRoot.X, inRoot.Y) : new Point(x, y);

			if (ReferenceEquals(relativeTo, _view))
				return new Point(x, y);

			var target = (relativeTo as VisualElement)?.Handler?.PlatformView as GtkWidget;
			if (target is not null && _widget.ComputePoint(target, point, out var inTarget))
				return new Point(inTarget.X, inTarget.Y);

			return null;
		}

		Func<IElement?, Point?> PositionAt(double x, double y) => relativeTo => GetPosition(relativeTo, x, y);

		// ------------------------------------------------------------------ click: tap and pointer press/release

		global::Gtk.GestureClick CreateClick()
		{
			var click = new global::Gtk.GestureClick { Button = 0 };
			click.Pressed += OnPressed;
			click.Released += OnReleased;
			return click;
		}

		static ButtonsMask ToMask(uint button) => button == 3 ? ButtonsMask.Secondary : ButtonsMask.Primary;

		void OnPressed(object? sender, global::Gtk.PressedArgs args)
		{
			var button = ToMask(((global::Gtk.GestureSingle)sender!).CurrentButton);
			foreach (var recognizer in _view.GestureRecognizers.GetGesturesFor<PointerGestureRecognizer>())
			{
				if ((recognizer.Buttons & button) != 0)
					recognizer.SendPointerPressed(_view, PositionAt(args.X, args.Y), null, button);
			}
		}

		void OnReleased(object? sender, global::Gtk.ReleasedArgs args)
		{
			uint gtkButton = ((global::Gtk.GestureSingle)sender!).CurrentButton;
			var button = ToMask(gtkButton);

			foreach (var recognizer in _view.GestureRecognizers.GetGesturesFor<PointerGestureRecognizer>())
			{
				if ((recognizer.Buttons & button) != 0)
					recognizer.SendPointerReleased(_view, PositionAt(args.X, args.Y), null, button);
			}

			// The middle button is neither primary nor secondary.
			if (gtkButton == 2)
				return;

			foreach (var recognizer in _view.GestureRecognizers.GetGesturesFor<TapGestureRecognizer>())
			{
				if ((recognizer.Buttons & button) == 0)
					continue;

				// A single-tap recognizer fires on every click (a double click is two taps); a multi-tap one on the click that completes the count.
				int required = Math.Max(1, recognizer.NumberOfTapsRequired);
				if (required == 1 || required == args.NPress)
					recognizer.SendTapped(_view, PositionAt(args.X, args.Y));
			}
		}

		// ------------------------------------------------------------------ drag: pan and swipe

		global::Gtk.GestureDrag CreateDrag()
		{
			var drag = new global::Gtk.GestureDrag();
			drag.DragStarted += (_, _) => _panStarted = false;
			drag.DragUpdate += OnDragUpdate;
			drag.DragEnded += OnDragEnded;
			drag.Cancel += (_, _) => FinishPan(success: false);
			return drag;
		}

		void OnDragUpdate(object? sender, global::Gtk.DragUpdateArgs args)
		{
			double dx = args.OffsetX, dy = args.OffsetY;

			if (!_panStarted)
			{
				if (Math.Sqrt((dx * dx) + (dy * dy)) < PanThreshold)
					return;

				_panStarted = true;
				foreach (IPanGestureController recognizer in PanRecognizers())
					recognizer.SendPanStarted(_view, PanGestureRecognizer.CurrentId.Value);
			}

			foreach (IPanGestureController recognizer in PanRecognizers())
				recognizer.SendPan(_view, dx, dy, PanGestureRecognizer.CurrentId.Value);
		}

		void OnDragEnded(object? sender, global::Gtk.DragEndedArgs args)
		{
			foreach (var recognizer in _view.GestureRecognizers.GetGesturesFor<SwipeGestureRecognizer>())
			{
				var controller = (ISwipeGestureController)recognizer;
				controller.SendSwipe(_view, args.OffsetX, args.OffsetY);
				controller.DetectSwipe(_view, recognizer.Direction);
			}

			FinishPan(success: true);
		}

		// A mouse is one touch point.
		System.Collections.Generic.IEnumerable<PanGestureRecognizer> PanRecognizers() =>
			_view.GestureRecognizers.GetGesturesFor<PanGestureRecognizer>(g => g.TouchPoints == 1);

		void FinishPan(bool success)
		{
			if (!_panStarted)
				return;

			_panStarted = false;
			foreach (IPanGestureController recognizer in PanRecognizers())
			{
				if (success)
					recognizer.SendPanCompleted(_view, PanGestureRecognizer.CurrentId.Value);
				else
					recognizer.SendPanCanceled(_view, PanGestureRecognizer.CurrentId.Value);
			}

			PanGestureRecognizer.CurrentId.Increment();
		}

		// ------------------------------------------------------------------ zoom: pinch

		global::Gtk.GestureZoom CreateZoom()
		{
			var zoom = new global::Gtk.GestureZoom();
			zoom.Begin += (_, _) =>
			{
				_pinchStarted = false;
				_lastScale = 1;
			};
			zoom.ScaleChanged += OnScaleChanged;
			zoom.End += (_, _) => FinishPinch(success: true);
			zoom.Cancel += (_, _) => FinishPinch(success: false);
			return zoom;
		}

		void OnScaleChanged(object? sender, global::Gtk.ScaleChangedArgs args)
		{
			var origin = PinchOrigin((global::Gtk.GestureZoom)sender!);

			if (!_pinchStarted)
			{
				_pinchStarted = true;
				foreach (IPinchGestureController recognizer in _view.GestureRecognizers.GetGesturesFor<PinchGestureRecognizer>())
					recognizer.SendPinchStarted(_view, origin);
			}

			// MAUI reports the change since the previous event, GTK the scale since the gesture began.
			double delta = _lastScale == 0 ? 1 : args.Scale / _lastScale;
			_lastScale = args.Scale;

			foreach (IPinchGestureController recognizer in _view.GestureRecognizers.GetGesturesFor<PinchGestureRecognizer>())
				recognizer.SendPinch(_view, delta, origin);
		}

		Point PinchOrigin(global::Gtk.GestureZoom zoom)
		{
			// Relative to the view, 0..1 in both directions.
			if (!zoom.GetBoundingBoxCenter(out double x, out double y))
				return new Point(0.5, 0.5);

			int width = Math.Max(1, _widget.Width);
			int height = Math.Max(1, _widget.Height);
			return new Point(x / width, y / height);
		}

		void FinishPinch(bool success)
		{
			if (!_pinchStarted)
				return;

			_pinchStarted = false;
			foreach (IPinchGestureController recognizer in _view.GestureRecognizers.GetGesturesFor<PinchGestureRecognizer>())
			{
				if (success)
					recognizer.SendPinchEnded(_view);
				else
					recognizer.SendPinchCanceled(_view);
			}
		}

		// ------------------------------------------------------------------ motion: pointer enter, move, exit

		global::Gtk.EventControllerMotion CreateMotion()
		{
			var motion = new global::Gtk.EventControllerMotion();
			motion.Enter += (_, args) =>
			{
				_pointerX = args.X;
				_pointerY = args.Y;
				SendPointer(static (r, view, position) => r.SendPointerEntered(view, position));
			};
			motion.Motion += (_, args) =>
			{
				_pointerX = args.X;
				_pointerY = args.Y;
				SendPointer(static (r, view, position) => r.SendPointerMoved(view, position));
			};
			motion.Leave += (_, _) => SendPointer(static (r, view, position) => r.SendPointerExited(view, position));
			return motion;
		}

		void SendPointer(Action<PointerGestureRecognizer, View, Func<IElement?, Point?>> send)
		{
			var position = PositionAt(_pointerX, _pointerY);
			foreach (var recognizer in _view.GestureRecognizers.GetGesturesFor<PointerGestureRecognizer>())
				send(recognizer, _view, position);
		}

		// ------------------------------------------------------------------ drag and drop

		global::Gtk.DragSource CreateDragSource()
		{
			var source = new global::Gtk.DragSource { Actions = Gdk.DragAction.Copy | Gdk.DragAction.Move };
			source.Prepare += (_, args) => args.RetVal = PrepareDrag(args.X, args.Y);
			source.DragEnd += (_, _) => EndDrag();
			return source;
		}

		/// <summary>Asks the drag recognizers whether a drag starts at the point, and builds the payload.</summary>
		/// <returns>The content provider of the dragged text, or <see langword="null"/> when no recognizer starts a drag.</returns>
		internal Gdk.ContentProvider? PrepareDrag(double x, double y)
		{
			foreach (var recognizer in _view.GestureRecognizers.GetGesturesFor<DragGestureRecognizer>(g => g.CanDrag))
			{
				var starting = recognizer.SendDragStarting(_view, PositionAt(x, y), new PlatformDragStartingEventArgs());

#pragma warning disable CS0618 // Handled is obsolete but still honoured
				if (starting.Cancel || starting.Handled)
					continue;
#pragma warning restore CS0618

				s_activeDrag = starting.Data;
				return new Gdk.ContentProvider(new GLib.Value(starting.Data.Text ?? string.Empty));
			}

			return null;
		}

		/// <summary>The drag ended: tells the drag recognizers and forgets the payload.</summary>
		internal void EndDrag()
		{
			s_activeDrag = null;
			var completed = new DropCompletedEventArgs(new PlatformDropCompletedEventArgs());
			foreach (var recognizer in _view.GestureRecognizers.GetGesturesFor<DragGestureRecognizer>())
				recognizer.SendDropCompleted(completed);
		}

		global::Gtk.DropTarget CreateDropTarget()
		{
			var target = new global::Gtk.DropTarget(GLib.GType.String, Gdk.DragAction.Copy | Gdk.DragAction.Move);
			target.Enter += (_, args) => args.RetVal = DragOver(args.X, args.Y);
			target.Motion += (_, args) => args.RetVal = DragOver(args.X, args.Y);
			target.Leave += (_, _) => DragLeave();
			target.Dropped += (_, args) => args.RetVal = Drop(args.Value.Val as string, args.X, args.Y);
			return target;
		}

		/// <summary>A drag moved over the view: asks the drop recognizers which operation they accept.</summary>
		/// <returns>The accepted action, or no action when no recognizer allows a drop.</returns>
		internal Gdk.DragAction DragOver(double x, double y)
		{
			_pointerX = x;
			_pointerY = y;

			var dragArgs = new DragEventArgs(s_activeDrag ?? new DataPackage(), PositionAt(x, y), new PlatformDragEventArgs());
			bool any = false;
			foreach (var recognizer in _view.GestureRecognizers.GetGesturesFor<DropGestureRecognizer>(g => g.AllowDrop))
			{
				any = true;
				recognizer.SendDragOver(dragArgs);
			}

			return any && dragArgs.AcceptedOperation != DataPackageOperation.None ? Gdk.DragAction.Copy : (Gdk.DragAction)0;
		}

		/// <summary>A drag left the view.</summary>
		internal void DragLeave()
		{
			var dragArgs = new DragEventArgs(s_activeDrag ?? new DataPackage(), PositionAt(_pointerX, _pointerY), new PlatformDragEventArgs());
			foreach (var recognizer in _view.GestureRecognizers.GetGesturesFor<DropGestureRecognizer>(g => g.AllowDrop))
				recognizer.SendDragLeave(dragArgs);
		}

		/// <summary>Text was dropped on the view; an in-process drag delivers the whole data package of the source.</summary>
		/// <returns>Whether a drop recognizer took the drop.</returns>
		internal bool Drop(string? text, double x, double y)
		{
			var package = s_activeDrag ?? new DataPackage { Text = text };
			var dropArgs = new DropEventArgs(package.View, PositionAt(x, y), new PlatformDropEventArgs());

			bool accepted = false;
			foreach (var recognizer in _view.GestureRecognizers.GetGesturesFor<DropGestureRecognizer>(g => g.AllowDrop))
			{
				accepted = true;
				_ = SendDropAsync(recognizer, dropArgs);
			}

			return accepted;
		}

		async System.Threading.Tasks.Task SendDropAsync(DropGestureRecognizer recognizer, DropEventArgs args)
		{
			try
			{
				await recognizer.SendDrop(args);
			}
			catch (Exception exception)
			{
				Application.Current?.FindMauiContext()?.CreateLogger<DropGestureRecognizer>()?.LogWarning(exception, "Error sending the drop");
			}
		}

		/// <inheritdoc/>
		public void Dispose()
		{
			if (_disposed)
				return;

			_disposed = true;
			if (_collection is not null)
				_collection.CollectionChanged -= OnRecognizersChanged;

			foreach (var controller in new global::Gtk.EventController?[] { _click, _drag, _zoom, _motion, _dragSource, _dropTarget })
			{
				if (controller is not null)
					_widget.RemoveController(controller);
			}

			_click = null;
			_drag = null;
			_zoom = null;
			_motion = null;
			_dragSource = null;
			_dropTarget = null;
		}
	}
}
