using System;
using Microsoft.Maui;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Core.ExternalBackend.TestSupport
{
	/// <summary>
	/// A view handler that models what a platform backend shipping outside of .NET MAUI has to do in order to
	/// manage a container view. It intentionally uses nothing but the public and protected surface of
	/// <see cref="ViewHandler{TVirtualView, TPlatformView}"/>; the fact that this assembly compiles is the proof
	/// that <see cref="ViewHandler.SetContainerView"/> is reachable from a non-friend assembly.
	/// </summary>
	public class ExternalBackendViewHandler<TVirtualView, TPlatformView> : ViewHandler<TVirtualView, TPlatformView>
		where TVirtualView : class, IView
		where TPlatformView : ExternalPlatformView, new()
	{
		/// <summary>
		/// The mapper used when no explicit mapper is supplied.
		/// </summary>
		public static IPropertyMapper<IView, IViewHandler> ExternalBackendMapper { get; } =
			new PropertyMapper<IView, IViewHandler>(ViewMapper);

		/// <summary>
		/// Initializes a new instance using <see cref="ExternalBackendMapper"/>.
		/// </summary>
		public ExternalBackendViewHandler()
			: base(ExternalBackendMapper)
		{
		}

		/// <summary>
		/// Initializes a new instance using the supplied mapper.
		/// </summary>
		public ExternalBackendViewHandler(IPropertyMapper mapper)
			: base(mapper)
		{
		}

		/// <summary>
		/// Gets the number of times <see cref="SetupContainer"/> has completed.
		/// </summary>
		public int SetupContainerCount { get; private set; }

		/// <summary>
		/// Gets the number of times <see cref="RemoveContainer"/> has completed.
		/// </summary>
		public int RemoveContainerCount { get; private set; }

		/// <summary>
		/// Gets the container view installed by this handler, strongly typed to the backend's wrapper type.
		/// </summary>
		public new ExternalWrapperView? ContainerView => (ExternalWrapperView?)base.ContainerView;

		/// <inheritdoc/>
		public override bool NeedsContainer
		{
			get
			{
				var view = ((IViewHandler)this).VirtualView;

				return view?.Background is not null ||
					view?.Clip is not null ||
					view?.Shadow is not null ||
					base.NeedsContainer;
			}
		}

		/// <inheritdoc/>
		protected override TPlatformView CreatePlatformView() => new();

		/// <inheritdoc/>
		protected override void SetupContainer()
		{
			if (ContainerView is not null)
			{
				return;
			}

			var platformView = PlatformView;
			var parent = platformView.Parent;
			var index = parent?.IndexOf(platformView) ?? -1;
			parent?.Remove(platformView);

			var wrapper = new ExternalWrapperView
			{
				Content = platformView
			};

			if (index >= 0)
			{
				parent!.Insert(index, wrapper);
			}
			else
			{
				parent?.Add(wrapper);
			}

			SetContainerView(wrapper);

			SetupContainerCount++;
		}

		/// <inheritdoc/>
		protected override void RemoveContainer()
		{
			RemoveContainerCount++;

			if (ContainerView is not ExternalWrapperView wrapper)
			{
				SetContainerView(null);
				return;
			}

			var parent = wrapper.Parent;
			var index = parent?.IndexOf(wrapper) ?? -1;
			parent?.Remove(wrapper);

			var platformView = wrapper.Content;
			wrapper.Dispose();

			SetContainerView(null);

			if (platformView is null)
			{
				return;
			}

			if (index >= 0)
			{
				parent!.Insert(index, platformView);
			}
			else
			{
				parent?.Add(platformView);
			}
		}

		/// <summary>
		/// Installs <paramref name="containerView"/> without going through <see cref="SetupContainer"/>, so tests can
		/// verify the guard rails of <see cref="ViewHandler.SetContainerView"/> directly.
		/// </summary>
		public void SetContainerViewDirectly(ExternalPlatformView? containerView) => SetContainerView(containerView);
	}

	/// <summary>
	/// A concrete external backend handler for an arbitrary <see cref="IView"/>.
	/// </summary>
	public class ExternalBackendViewHandler : ExternalBackendViewHandler<IView, ExternalPlatformView>
	{
		/// <summary>
		/// Initializes a new instance using the default external backend mapper.
		/// </summary>
		public ExternalBackendViewHandler()
		{
		}

		/// <summary>
		/// Initializes a new instance using the supplied mapper.
		/// </summary>
		public ExternalBackendViewHandler(IPropertyMapper mapper)
			: base(mapper)
		{
		}
	}
}
