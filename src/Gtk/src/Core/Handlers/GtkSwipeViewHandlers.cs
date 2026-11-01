using System;
using System.Collections.Generic;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Gtk.Hosting;
using Microsoft.Maui.Gtk.ImageSources;
using Microsoft.Maui.Gtk.Platform;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;

namespace Microsoft.Maui.Gtk.Handlers
{
	/// <summary>
	/// The panel behind a <c>SwipeView</c>: the content on top and, underneath it, one box of item widgets per side. A horizontal or vertical drag slides the
	/// content (a translation the platform applies, see <see cref="GtkViewState.PlatformOffsetX"/>) and so reveals the items of the side being dragged towards;
	/// releasing past half of the items closes or opens (or, in <see cref="SwipeMode.Execute"/>, executes) them.
	/// </summary>
	/// <remarks>Opening and closing are instant (not animated).</remarks>
	public sealed class MauiGtkSwipePanel : MauiGtkPanel
	{
		const double DragThreshold = 8;

		static readonly OpenSwipeItem[] s_sides = { OpenSwipeItem.LeftItems, OpenSwipeItem.TopItems, OpenSwipeItem.RightItems, OpenSwipeItem.BottomItems };

		readonly global::Gtk.Box?[] _boxes = new global::Gtk.Box?[4];
		readonly SwipeMode[] _modes = new SwipeMode[4];
		readonly SwipeBehaviorOnInvoked[] _behaviors = new SwipeBehaviorOnInvoked[4];
		readonly Action<int>?[] _invokers = new Action<int>?[4];
		readonly List<(global::Gtk.Widget Widget, EventHandler? Clicked, global::Gtk.GestureClick? Gesture)>[] _hooks = new List<(global::Gtk.Widget, EventHandler?, global::Gtk.GestureClick?)>[4];
		global::Gtk.Widget? _content;
		global::Gtk.GestureDrag? _drag;
		bool _horizontalDrag;
		bool _axisDecided;
		bool _axisIgnored;
		double _dragStartX;
		double _dragStartY;
		double _x;
		double _y;

		/// <summary>Creates the panel.</summary>
		public MauiGtkSwipePanel()
		{
			for (int i = 0; i < _hooks.Length; i++)
				_hooks[i] = new();

			_drag = new global::Gtk.GestureDrag { PropagationPhase = global::Gtk.PropagationPhase.Capture };
			_drag.DragStarted += OnDragBegin;
			_drag.DragUpdate += OnDragUpdate;
			_drag.DragEnded += OnDragEnd;
			AddController(_drag);
		}

		/// <summary>Whether the content slides alone (<see cref="SwipeTransitionMode.Reveal"/>) or drags the items along (<see cref="SwipeTransitionMode.Drag"/>).</summary>
		public SwipeTransitionMode TransitionMode { get; set; }

		/// <summary>The side that is open or being swiped, or <see langword="null"/> when closed.</summary>
		public OpenSwipeItem? OpenSide { get; private set; }

		/// <summary>Whether a side is fully or partly open.</summary>
		public bool IsOpen => OpenSide is not null;

		/// <summary>The current content offset (positive: to the right / down).</summary>
		public Point ContentOffset => new(_x, _y);

		/// <summary>Raised when a swipe starts (the direction of the finger).</summary>
		public event Action<SwipeDirection>? SwipeStarted;

		/// <summary>Raised while swiping with the offset magnitude.</summary>
		public event Action<SwipeDirection, double>? SwipeChanging;

		/// <summary>Raised when the swipe ended; the flag tells whether the items stay open.</summary>
		public event Action<SwipeDirection, bool>? SwipeEnded;

		/// <summary>Sets the content widget (replaces the previous one).</summary>
		/// <param name="content">The content widget, or <see langword="null"/>.</param>
		public void SetContent(global::Gtk.Widget? content)
		{
			if (DirectChild(_content) is { } old)
				RemoveChild(old);

			_content = content;
			if (content is not null)
				AddChild(content);
		}

		/// <summary>The extent of the items of <paramref name="side"/> along the swipe axis, in pixels (0 without items).</summary>
		/// <param name="side">The side.</param>
		public int ExtentOf(OpenSwipeItem side)
		{
			var box = _boxes[(int)side];
			if (box is null || box.FirstChild is null)
				return 0;

			bool horizontal = side is OpenSwipeItem.LeftItems or OpenSwipeItem.RightItems;
			box.Measure(horizontal ? global::Gtk.Orientation.Horizontal : global::Gtk.Orientation.Vertical, -1, out _, out int natural, out _, out _);
			return natural;
		}

		/// <summary>Replaces the items of one side.</summary>
		/// <param name="side">The side.</param>
		/// <param name="widgets">The item widgets, in order.</param>
		/// <param name="mode">Reveal or execute.</param>
		/// <param name="behavior">What happens after an item was invoked.</param>
		/// <param name="invoke">Called with the index of the item the user invoked.</param>
		public void SetItems(OpenSwipeItem side, IReadOnlyList<global::Gtk.Widget> widgets, SwipeMode mode, SwipeBehaviorOnInvoked behavior, Action<int>? invoke)
		{
			int index = (int)side;
			bool horizontal = side is OpenSwipeItem.LeftItems or OpenSwipeItem.RightItems;

			// Unhook and drop the previous items.
			foreach (var (widget, clicked, gesture) in _hooks[index])
			{
				if (clicked is not null && widget is global::Gtk.Button button)
					button.Clicked -= clicked;
				if (gesture is not null)
					widget.RemoveController(gesture);
			}

			_hooks[index].Clear();

			if (_boxes[index] is { } oldBox)
			{
				for (global::Gtk.Widget? child; (child = oldBox.FirstChild) is not null;)
					oldBox.Remove(child);

				RemoveChild(oldBox);
				_boxes[index] = null;
			}

			_modes[index] = mode;
			_behaviors[index] = behavior;
			_invokers[index] = invoke;

			if (widgets.Count > 0)
			{
				var box = new global::Gtk.Box(horizontal ? global::Gtk.Orientation.Horizontal : global::Gtk.Orientation.Vertical, 0) { Opacity = 0, CanTarget = false };
				for (int i = 0; i < widgets.Count; i++)
				{
					int itemIndex = i;
					var widget = widgets[i];
					if (widget.Parent is not null)
						widget.Unparent();

					box.Append(widget);

					if (widget is global::Gtk.Button button)
					{
						EventHandler clicked = (s, e) => ItemInvoked(side, itemIndex);
						button.Clicked += clicked;
						_hooks[index].Add((widget, clicked, null));
					}
					else
					{
						var gesture = new global::Gtk.GestureClick();
						gesture.Released += (o, args) => ItemInvoked(side, itemIndex);
						widget.AddController(gesture);
						_hooks[index].Add((widget, null, gesture));
					}
				}

				_boxes[index] = box;
				// The items sit under the content in z-order.
				InsertChild(0, box);
			}

			if (OpenSide == side && widgets.Count == 0)
				Close();
		}

		/// <summary>Opens the items of <paramref name="side"/> completely. Does nothing when the side has no items.</summary>
		/// <param name="side">The side.</param>
		public void Open(OpenSwipeItem side)
		{
			int extent = ExtentOf(side);
			if (extent == 0)
				return;

			Close(raise: false);
			SetOffsetFor(side, extent);
			OpenSide = side;
			ApplyOffsets();
			SwipeEnded?.Invoke(ToDirection(side), true);
		}

		/// <summary>Closes any open side.</summary>
		public void Close() => Close(raise: true);

		void Close(bool raise)
		{
			var side = OpenSide;
			_x = _y = 0;
			OpenSide = null;
			ApplyOffsets();

			if (raise && side is { } closed)
				SwipeEnded?.Invoke(ToDirection(closed), false);
		}

		/// <summary>Moves the content to a signed offset as a swipe would (used by the gesture and by tests); it does not raise swipe events.</summary>
		/// <param name="x">The horizontal offset: positive reveals the left items, negative the right items.</param>
		/// <param name="y">The vertical offset: positive reveals the top items, negative the bottom items.</param>
		internal void MoveContent(double x, double y)
		{
			_x = Math.Max(-ExtentOf(OpenSwipeItem.RightItems), Math.Min(ExtentOf(OpenSwipeItem.LeftItems), x));
			_y = Math.Max(-ExtentOf(OpenSwipeItem.BottomItems), Math.Min(ExtentOf(OpenSwipeItem.TopItems), y));
			OpenSide = _x > 0 ? OpenSwipeItem.LeftItems : _x < 0 ? OpenSwipeItem.RightItems : _y > 0 ? OpenSwipeItem.TopItems : _y < 0 ? OpenSwipeItem.BottomItems : null;
			ApplyOffsets();
		}

		/// <summary>Finishes a swipe the way releasing the pointer does.</summary>
		internal void ReleaseSwipe()
		{
			if (OpenSide is not { } side)
				return;

			int extent = ExtentOf(side);
			double magnitude = side is OpenSwipeItem.LeftItems or OpenSwipeItem.RightItems ? Math.Abs(_x) : Math.Abs(_y);
			var direction = ToDirection(side);

			if (extent == 0 || magnitude < extent / 2.0)
			{
				Close(raise: false);
				SwipeEnded?.Invoke(direction, false);
				return;
			}

			if (_modes[(int)side] == SwipeMode.Execute)
			{
				int count = ItemCount(side);
				for (int i = 0; i < count; i++)
					_invokers[(int)side]?.Invoke(i);

				Close(raise: false);
				SwipeEnded?.Invoke(direction, false);
				return;
			}

			SetOffsetFor(side, extent);
			ApplyOffsets();
			SwipeEnded?.Invoke(direction, true);
		}

		int ItemCount(OpenSwipeItem side)
		{
			int count = 0;
			for (var child = _boxes[(int)side]?.FirstChild; child is not null; child = child.NextSibling)
				count++;
			return count;
		}

		void ItemInvoked(OpenSwipeItem side, int itemIndex)
		{
			_invokers[(int)side]?.Invoke(itemIndex);

			var behavior = _behaviors[(int)side];
			bool close = behavior == SwipeBehaviorOnInvoked.Close
				|| (behavior == SwipeBehaviorOnInvoked.Auto && _modes[(int)side] == SwipeMode.Reveal);

			if (close && OpenSide == side)
				Close();
		}

		void SetOffsetFor(OpenSwipeItem side, double extent)
		{
			_x = _y = 0;
			switch (side)
			{
				case OpenSwipeItem.LeftItems:
					_x = extent;
					break;
				case OpenSwipeItem.RightItems:
					_x = -extent;
					break;
				case OpenSwipeItem.TopItems:
					_y = extent;
					break;
				case OpenSwipeItem.BottomItems:
					_y = -extent;
					break;
			}
		}

		static SwipeDirection ToDirection(OpenSwipeItem side) => side switch
		{
			OpenSwipeItem.LeftItems => SwipeDirection.Right,
			OpenSwipeItem.RightItems => SwipeDirection.Left,
			OpenSwipeItem.TopItems => SwipeDirection.Down,
			_ => SwipeDirection.Up,
		};

		global::Gtk.Widget? DirectChild(global::Gtk.Widget? widget)
		{
			// The content may have been wrapped in a container (clip/shadow) after it was added.
			while (widget is not null && !ReferenceEquals(widget.Parent, this))
				widget = widget.Parent;
			return widget;
		}

		void ApplyOffsets()
		{
			// The boxes stay visible (a hidden widget measures as 0, and the extent of the items is needed to size the swipe); the ones not
			// being revealed are transparent and ignore the pointer.
			for (int i = 0; i < _boxes.Length; i++)
			{
				if (_boxes[i] is not { } box)
					continue;

				bool revealed = (_x > 0 && s_sides[i] == OpenSwipeItem.LeftItems) || (_x < 0 && s_sides[i] == OpenSwipeItem.RightItems)
					|| (_y > 0 && s_sides[i] == OpenSwipeItem.TopItems) || (_y < 0 && s_sides[i] == OpenSwipeItem.BottomItems);
				box.Opacity = revealed ? 1 : 0;
				box.CanTarget = revealed;
			}

			QueueAllocate();
			QueueResize();
		}

		/// <inheritdoc/>
		protected override void ArrangeChildren(int width, int height)
		{
			if (DirectChild(_content) is { } content)
			{
				var state = GtkViewState.For(content);
				state.PlatformOffsetX = _x;
				state.PlatformOffsetY = _y;
			}

			base.ArrangeChildren(width, height);

			for (int i = 0; i < _boxes.Length; i++)
			{
				if (_boxes[i] is not { } box)
					continue;

				var side = s_sides[i];
				bool horizontal = side is OpenSwipeItem.LeftItems or OpenSwipeItem.RightItems;

				box.Measure(global::Gtk.Orientation.Horizontal, -1, out _, out int naturalWidth, out _, out _);
				box.Measure(global::Gtk.Orientation.Vertical, -1, out _, out int naturalHeight, out _, out _);
				int boxWidth = horizontal ? naturalWidth : width;
				int boxHeight = horizontal ? height : naturalHeight;
				box.Measure(global::Gtk.Orientation.Horizontal, -1, out int minWidth, out _, out _, out _);
				box.Measure(global::Gtk.Orientation.Vertical, boxWidth, out int minHeight, out _, out _, out _);
				boxWidth = Math.Max(boxWidth, minWidth);
				boxHeight = Math.Max(boxHeight, minHeight);

				bool drag = TransitionMode == SwipeTransitionMode.Drag;
				double x = 0;
				double y = 0;
				switch (side)
				{
					case OpenSwipeItem.LeftItems:
						x = drag ? _x - boxWidth : 0;
						break;
					case OpenSwipeItem.RightItems:
						x = drag ? width + _x : width - boxWidth;
						break;
					case OpenSwipeItem.TopItems:
						y = drag ? _y - boxHeight : 0;
						break;
					case OpenSwipeItem.BottomItems:
						y = drag ? height + _y : height - boxHeight;
						break;
				}

				box.Allocate(boxWidth, boxHeight, -1, new Gsk.Transform().Translate(GtkInterop.ToGraphene(x, y)));
			}
		}

		void OnDragBegin(object o, global::Gtk.DragStartedArgs args)
		{
			_axisDecided = false;
			_axisIgnored = false;
			_dragStartX = _x;
			_dragStartY = _y;
		}

		void OnDragUpdate(object o, global::Gtk.DragUpdateArgs args)
		{
			if (_axisIgnored)
				return;

			if (!_axisDecided)
			{
				if (Math.Abs(args.OffsetX) < DragThreshold && Math.Abs(args.OffsetY) < DragThreshold)
					return;

				_horizontalDrag = Math.Abs(args.OffsetX) >= Math.Abs(args.OffsetY);
				bool hasItems = _horizontalDrag
					? ExtentOf(OpenSwipeItem.LeftItems) > 0 || ExtentOf(OpenSwipeItem.RightItems) > 0
					: ExtentOf(OpenSwipeItem.TopItems) > 0 || ExtentOf(OpenSwipeItem.BottomItems) > 0;

				if (!hasItems)
				{
					_axisIgnored = true;
					return;
				}

				_axisDecided = true;

				// The swipe takes the sequence away from anything underneath (a scroller, a button).
				_drag?.SetState(global::Gtk.EventSequenceState.Claimed);

				var first = _horizontalDrag
					? (args.OffsetX > 0 ? SwipeDirection.Right : SwipeDirection.Left)
					: (args.OffsetY > 0 ? SwipeDirection.Down : SwipeDirection.Up);
				SwipeStarted?.Invoke(first);
			}

			if (_horizontalDrag)
				MoveContent(_dragStartX + args.OffsetX, 0);
			else
				MoveContent(0, _dragStartY + args.OffsetY);

			if (OpenSide is { } side)
				SwipeChanging?.Invoke(ToDirection(side), _horizontalDrag ? Math.Abs(_x) : Math.Abs(_y));
		}

		void OnDragEnd(object o, global::Gtk.DragEndedArgs args)
		{
			if (_axisDecided)
				ReleaseSwipe();

			_axisDecided = false;
			_axisIgnored = false;
		}
	}

	/// <summary>
	/// <see cref="SwipeViewHandler"/> over a <see cref="MauiGtkSwipePanel"/>. Menu items are buttons, view items are their own widgets; both are invoked by a click.
	/// </summary>
	public class GtkSwipeViewHandler : SwipeViewHandler
	{
		/// <summary>Creates the handler with the stock mappers.</summary>
		public GtkSwipeViewHandler()
			: base(GtkMapper, GtkCommandMapper)
		{
		}

		/// <summary>Creates the handler with custom mappers.</summary>
		/// <param name="mapper">The property mapper, or <see langword="null"/> for the stock one.</param>
		/// <param name="commandMapper">The command mapper, or <see langword="null"/> for the stock one.</param>
		public GtkSwipeViewHandler(IPropertyMapper? mapper, CommandMapper? commandMapper = null)
			: base(mapper ?? GtkMapper, commandMapper ?? GtkCommandMapper)
		{
		}

		/// <inheritdoc/>
		protected override object CreatePlatformView() => new MauiGtkSwipePanel { CrossPlatformLayout = VirtualView, IsMeasuredByMaui = true };

		/// <inheritdoc/>
		protected override void ConnectHandler(object platformView)
		{
			base.ConnectHandler(platformView);

			if (platformView is not MauiGtkSwipePanel panel)
				return;

			panel.CrossPlatformLayout = VirtualView;
			panel.SwipeStarted += OnSwipeStarted;
			panel.SwipeChanging += OnSwipeChanging;
			panel.SwipeEnded += OnSwipeEnded;
		}

		/// <inheritdoc/>
		protected override void DisconnectHandler(object platformView)
		{
			if (platformView is MauiGtkSwipePanel panel)
			{
				panel.SwipeStarted -= OnSwipeStarted;
				panel.SwipeChanging -= OnSwipeChanging;
				panel.SwipeEnded -= OnSwipeEnded;
				panel.SetContent(null);
				panel.CrossPlatformLayout = null;
			}

			base.DisconnectHandler(platformView);
		}

		// The virtual view is null once the handler disconnected; the generic VirtualView property would throw then.
		ISwipeView? View => ((IElementHandler)this).VirtualView as ISwipeView;

		void OnSwipeStarted(SwipeDirection direction) => View?.SwipeStarted(new SwipeViewSwipeStarted(direction));

		void OnSwipeChanging(SwipeDirection direction, double offset) => View?.SwipeChanging(new SwipeViewSwipeChanging(direction, offset));

		void OnSwipeEnded(SwipeDirection direction, bool isOpen)
		{
			if (View is not { } view)
				return;

			view.SwipeEnded(new SwipeViewSwipeEnded(direction, isOpen));
			if (view.IsOpen != isOpen)
				view.IsOpen = isOpen;
		}

		static void UpdateItems(ISwipeViewHandler handler, ISwipeView view, MauiGtkSwipePanel panel, OpenSwipeItem side)
		{
			var items = side switch
			{
				OpenSwipeItem.LeftItems => view.LeftItems,
				OpenSwipeItem.TopItems => view.TopItems,
				OpenSwipeItem.RightItems => view.RightItems,
				_ => view.BottomItems,
			};

			var context = handler.MauiContext;
			var widgets = new List<global::Gtk.Widget>();
			var invokable = new List<ISwipeItem>();

			if (items is not null && context is not null)
			{
				foreach (var item in items)
				{
					if (item is ISwipeItemMenuItem { Visibility: Visibility.Collapsed })
						continue;
					if (item is ISwipeItemView { Visibility: Visibility.Collapsed })
						continue;

					if (item.ToPlatform(context) is global::Gtk.Widget widget)
					{
						widgets.Add(widget);
						invokable.Add(item);
					}
				}
			}

			panel.SetItems(side, widgets, items?.Mode ?? SwipeMode.Reveal, items?.SwipeBehaviorOnInvoked ?? SwipeBehaviorOnInvoked.Auto,
				index => invokable[index].OnInvoked());
		}

		internal static readonly IPropertyMapper<ISwipeView, ISwipeViewHandler> GtkMapper =
			new GtkMapperBuilder<ISwipeView, ISwipeViewHandler>(SwipeViewHandler.Mapper)
				.AddViewMappings()
				.MapGtk<MauiGtkSwipePanel>(nameof(IContentView.Content), (h, v, panel) =>
				{
					var context = h.MauiContext;
					panel.SetContent(context is not null && v.PresentedContent is { } content ? (global::Gtk.Widget)content.ToPlatform(context) : null);
				})
				.MapGtk<MauiGtkSwipePanel>(nameof(ISwipeView.SwipeTransitionMode), (h, v, panel) =>
				{
					panel.TransitionMode = v.SwipeTransitionMode;
					panel.QueueAllocate();
				})
				.MapGtk<MauiGtkSwipePanel>(nameof(ISwipeView.LeftItems), (h, v, panel) => UpdateItems(h, v, panel, OpenSwipeItem.LeftItems))
				.MapGtk<MauiGtkSwipePanel>(nameof(ISwipeView.TopItems), (h, v, panel) => UpdateItems(h, v, panel, OpenSwipeItem.TopItems))
				.MapGtk<MauiGtkSwipePanel>(nameof(ISwipeView.RightItems), (h, v, panel) => UpdateItems(h, v, panel, OpenSwipeItem.RightItems))
				.MapGtk<MauiGtkSwipePanel>(nameof(ISwipeView.BottomItems), (h, v, panel) => UpdateItems(h, v, panel, OpenSwipeItem.BottomItems))
				.Build();

		internal static readonly CommandMapper<ISwipeView, ISwipeViewHandler> GtkCommandMapper =
			new GtkCommandMapperBuilder<ISwipeView, ISwipeViewHandler>(SwipeViewHandler.CommandMapper)
				.AddViewCommands()
				.MapGtk<MauiGtkSwipePanel>(nameof(ISwipeView.RequestOpen), (h, v, args, panel) =>
				{
					if (args is SwipeViewOpenRequest request)
						panel.Open(request.OpenSwipeItem);
				})
				.MapGtk<MauiGtkSwipePanel>(nameof(ISwipeView.RequestClose), (h, v, args, panel) =>
				{
					if (args is SwipeViewCloseRequest)
						panel.Close();
				})
				.Build();
	}

	/// <summary>
	/// <see cref="SwipeItemMenuItemHandler"/> over a <see cref="MauiGtkButton"/> (text, optional image, background); the owning swipe panel invokes it.
	/// </summary>
	public class GtkSwipeItemMenuItemHandler : SwipeItemMenuItemHandler
	{
		readonly GtkImageLoader _loader = new();

		/// <summary>Creates the handler with the stock mappers.</summary>
		public GtkSwipeItemMenuItemHandler()
			: base(GtkMapper, GtkCommandMapper)
		{
		}

		/// <summary>Creates the handler with custom mappers.</summary>
		/// <param name="mapper">The property mapper, or <see langword="null"/> for the stock one.</param>
		/// <param name="commandMapper">The command mapper, or <see langword="null"/> for the stock one.</param>
		public GtkSwipeItemMenuItemHandler(IPropertyMapper? mapper, CommandMapper? commandMapper = null)
			: base(mapper ?? GtkMapper, commandMapper ?? GtkCommandMapper)
		{
		}

		/// <inheritdoc/>
		protected override object CreatePlatformElement()
		{
			var button = new MauiGtkButton();
			GtkStyle.For(button).Set("min-width", "72px");
			GtkStyle.For(button).Set("border-radius", "0");
			return button;
		}

		/// <inheritdoc/>
		protected override void DisconnectHandler(object platformView)
		{
			_loader.Reset();
			base.DisconnectHandler(platformView);
		}

		internal static readonly IPropertyMapper<ISwipeItemMenuItem, ISwipeItemMenuItemHandler> GtkMapper =
			new GtkMapperBuilder<ISwipeItemMenuItem, ISwipeItemMenuItemHandler>(SwipeItemMenuItemHandler.Mapper)
				.MapGtk<MauiGtkButton>(nameof(IMenuElement.Text), (h, v, w) => w.Text = v.Text ?? string.Empty)
				.MapGtk<MauiGtkButton>(nameof(ITextStyle.TextColor), (h, v, w) => GtkTextStyle.SetColor(w, (v as ITextStyle)?.TextColor))
				.MapGtk<MauiGtkButton>(nameof(ITextStyle.CharacterSpacing), (h, v, w) =>
					GtkTextStyle.SetCharacterSpacing(w, (v as ITextStyle)?.CharacterSpacing ?? 0))
				.MapGtk<MauiGtkButton>(nameof(ITextStyle.Font), (h, v, w) =>
				{
					if (v is ITextStyle style)
						GtkTextStyle.SetFont(w, style.Font, GtkTextStyle.GetFontManager(h));
				})
				.MapGtk<MauiGtkButton>(nameof(ISwipeItemMenuItem.Background), (h, v, w) => GtkBackgroundImage.Update(h, w, v.Background))
				.MapGtk<MauiGtkButton>(nameof(ISwipeItemMenuItem.Visibility), (h, v, w) => w.Visible = v.Visibility != Visibility.Collapsed)
				.MapGtk<MauiGtkButton>(nameof(IMenuElement.Source), (h, v, w) =>
				{
					if (h is GtkSwipeItemMenuItemHandler handler)
						_ = handler._loader.UpdateAsync(h, v, texture => w.SetImage(texture));
				})
				.Build();

		internal static readonly CommandMapper<ISwipeItemMenuItem, ISwipeItemMenuItemHandler> GtkCommandMapper =
			new(SwipeItemMenuItemHandler.CommandMapper);
	}

	/// <summary>
	/// <see cref="SwipeItemViewHandler"/> over a <see cref="MauiGtkPanel"/> hosting the item content; the owning swipe panel invokes it on a click.
	/// </summary>
	public class GtkSwipeItemViewHandler : SwipeItemViewHandler
	{
		/// <summary>Creates the handler with the stock mappers.</summary>
		public GtkSwipeItemViewHandler()
			: base(GtkMapper, GtkCommandMapper)
		{
		}

		/// <summary>Creates the handler with custom mappers.</summary>
		/// <param name="mapper">The property mapper, or <see langword="null"/> for the stock one.</param>
		/// <param name="commandMapper">The command mapper, or <see langword="null"/> for the stock one.</param>
		public GtkSwipeItemViewHandler(IPropertyMapper? mapper, CommandMapper? commandMapper = null)
			: base(mapper ?? GtkMapper, commandMapper ?? GtkCommandMapper)
		{
		}

		/// <inheritdoc/>
		protected override object CreatePlatformView() => new MauiGtkPanel { CrossPlatformLayout = VirtualView, IsMeasuredByMaui = true };

		/// <inheritdoc/>
		protected override void ConnectHandler(object platformView)
		{
			base.ConnectHandler(platformView);
			GtkContainerBehavior.Connect(platformView, VirtualView);
		}

		/// <inheritdoc/>
		protected override void DisconnectHandler(object platformView)
		{
			GtkContainerBehavior.Disconnect(platformView);
			base.DisconnectHandler(platformView);
		}

		internal static readonly IPropertyMapper<ISwipeItemView, ISwipeItemViewHandler> GtkMapper =
			new GtkMapperBuilder<ISwipeItemView, ISwipeItemViewHandler>(SwipeItemViewHandler.Mapper)
				.AddViewMappings()
				.MapGtk<MauiGtkPanel>(nameof(IContentView.Content), (h, v, panel) => GtkContainerBehavior.UpdateContent(h, v, panel))
				.Build();

		internal static readonly CommandMapper<ISwipeItemView, ISwipeItemViewHandler> GtkCommandMapper =
			new GtkCommandMapperBuilder<ISwipeItemView, ISwipeItemViewHandler>(SwipeItemViewHandler.CommandMapper).AddViewCommands().Build();
	}
}
