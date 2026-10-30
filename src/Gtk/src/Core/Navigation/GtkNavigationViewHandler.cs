using System;
using System.Collections.Generic;
using Microsoft.Maui.Gtk.Hosting;
using Microsoft.Maui.Gtk.Platform;
using Microsoft.Maui.Handlers;
using GtkStack = Gtk.Stack;

namespace Microsoft.Maui.Gtk.Handlers
{
	/// <summary>
	/// <see cref="NavigationViewHandler"/> over a <c>Gtk.Stack</c>: every navigation stack entry is a page shown in a
	/// <see cref="MauiGtkFillHost"/>; pushes and pops slide the new page in.
	/// </summary>
	/// <remarks>
	/// <para>Only the top page is realised. Pages that left the stack are detached once the transition ended (a short timer, because a
	/// headless display never ticks the frame clock); their platform views are not destroyed, so a page can be pushed again.</para>
	/// <para>The back button and the title belong to the toolbar (<see cref="GtkToolbarHandler"/>), not to the stack.</para>
	/// </remarks>
	public class GtkNavigationViewHandler : NavigationViewHandler
	{
		/// <summary>Duration, in milliseconds, of the slide transition of an animated navigation (0 turns animations off).</summary>
		public static uint TransitionDuration { get; set; } = 250;

		readonly List<MauiGtkFillHost> _retiring = new();
		MauiGtkFillHost? _currentHost;
		IView? _currentTop;
		IReadOnlyList<IView> _stack = Array.Empty<IView>();
		uint _cleanupSource;
		int _counter;

		/// <summary>Creates the handler with the stock mappers.</summary>
		public GtkNavigationViewHandler()
			: base(GtkMapper, GtkCommandMapper)
		{
		}

		/// <summary>Creates the handler with custom mappers.</summary>
		/// <param name="mapper">The property mapper, or <see langword="null"/> for the stock one.</param>
		/// <param name="commandMapper">The command mapper, or <see langword="null"/> for the stock one.</param>
		public GtkNavigationViewHandler(IPropertyMapper? mapper, CommandMapper? commandMapper = null)
			: base(mapper ?? GtkMapper, commandMapper ?? GtkCommandMapper)
		{
		}

		/// <summary>The navigation stack as last applied (bottom first).</summary>
		public IReadOnlyList<IView> NavigationStack => _stack;

		/// <summary>The stack widget (the platform view).</summary>
		public GtkStack Stack => (GtkStack)((IElementHandler)this).PlatformView!;

		/// <inheritdoc/>
		protected override object CreatePlatformView() => new GtkStack
		{
			TransitionType = global::Gtk.StackTransitionType.None,
			Hhomogeneous = true,
			Vhomogeneous = true,
			Hexpand = true,
			Vexpand = true,
		};

		/// <inheritdoc/>
		protected override void DisconnectHandler(object platformView)
		{
			CancelCleanup();
			RemoveRetired();

			if (_currentHost is { } host && platformView is GtkStack stack)
			{
				stack.Remove(host);
				host.Clear();
			}

			_currentHost = null;
			_currentTop = null;
			_stack = Array.Empty<IView>();

			base.DisconnectHandler(platformView);
		}

		/// <summary>Applies a navigation request: shows the top page of the requested stack and reports back to the virtual view.</summary>
		/// <param name="request">The request.</param>
		public void Navigate(NavigationRequest request)
		{
			if (request is null)
				throw new ArgumentNullException(nameof(request));

			var virtualView = ((IElementHandler)this).VirtualView as IStackNavigation ?? throw new InvalidOperationException("The handler has no virtual view.");
			var context = MauiContext ?? throw new InvalidOperationException("MauiContext should have been set by the base class.");
			var stack = Stack;

			var requested = request.NavigationStack;
			var top = requested.Count > 0 ? requested[requested.Count - 1] : null;

			// A new navigation finishes the previous one's clean-up first, so hosts never pile up.
			CancelCleanup();
			RemoveRetired();

			if (!ReferenceEquals(top, _currentTop))
			{
				bool pop = _currentTop is not null && Contains(_stack, top);
				bool animate = request.Animated && _currentHost is not null && top is not null && TransitionDuration > 0;

				MauiGtkFillHost? host = null;
				if (top is not null)
				{
					host = new MauiGtkFillHost();
					host.SetView(top, context);
					stack.AddNamed(host, "page" + (_counter++));
				}

				stack.TransitionDuration = TransitionDuration;
				stack.TransitionType = !animate
					? global::Gtk.StackTransitionType.None
					: pop ? global::Gtk.StackTransitionType.SlideRight : global::Gtk.StackTransitionType.SlideLeft;

				if (host is not null)
					stack.VisibleChild = host;

				if (_currentHost is not null)
					_retiring.Add(_currentHost);

				_currentHost = host;
				_currentTop = top;
			}

			_stack = requested;

			if (_retiring.Count > 0)
			{
				if (request.Animated && TransitionDuration > 0 && _currentHost is not null)
					_cleanupSource = GLib.Timeout.Add(TransitionDuration + 50, OnCleanup);
				else
					RemoveRetired();
			}

			virtualView.NavigationFinished(requested);
		}

		static bool Contains(IReadOnlyList<IView> stack, IView? view)
		{
			for (int i = 0; i < stack.Count; i++)
			{
				if (ReferenceEquals(stack[i], view))
					return true;
			}

			return false;
		}

		bool OnCleanup()
		{
			_cleanupSource = 0;
			RemoveRetired();
			return false;
		}

		void CancelCleanup()
		{
			if (_cleanupSource != 0)
			{
				GLib.Timeout.Remove(_cleanupSource);
				_cleanupSource = 0;
			}
		}

		void RemoveRetired()
		{
			if (_retiring.Count == 0)
				return;

			var stack = ((IElementHandler)this).PlatformView as GtkStack;
			foreach (var host in _retiring)
			{
				stack?.Remove(host);
				host.Clear();
			}

			_retiring.Clear();
		}

		internal static readonly IPropertyMapper<IStackNavigationView, INavigationViewHandler> GtkMapper =
			new GtkMapperBuilder<IStackNavigationView, INavigationViewHandler>(NavigationViewHandler.Mapper)
				.AddViewMappings()
				// A navigation page that is the root of a modal page owns the toolbar of that modal window.
				.MapGtk<GtkStack>(nameof(IToolbarElement.Toolbar), (h, v, w) => GtkToolbarSupport.UpdateWindowToolbar(h, v as IToolbarElement))
				.Build();

		internal static readonly CommandMapper<IStackNavigationView, INavigationViewHandler> GtkCommandMapper =
			new GtkCommandMapperBuilder<IStackNavigationView, INavigationViewHandler>(NavigationViewHandler.CommandMapper)
				.AddViewCommands()
				// The stock command throws NotImplementedException on the platform-less assemblies, so it is not chained.
				.MapGtk<GtkStack>(nameof(IStackNavigation.RequestNavigation), (h, v, args, w) =>
				{
					if (h is GtkNavigationViewHandler handler && args is NavigationRequest request)
						handler.Navigate(request);
				}, callStock: false)
				.Build();
	}
}
