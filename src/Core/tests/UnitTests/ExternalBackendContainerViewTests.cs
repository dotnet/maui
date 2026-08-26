using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Core.ExternalBackend.TestSupport;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.UnitTests
{
	/// <summary>
	/// Verifies that a handler living in an assembly that is not a friend of Microsoft.Maui can manage
	/// <see cref="ViewHandler.ContainerView"/> through the public/protected surface only.
	/// </summary>
	[Category(TestCategory.Core, TestCategory.View)]
	public class ExternalBackendContainerViewTests
	{
		static readonly IPropertyMapper<IView, IViewHandler> ContainerOnlyMapper =
			new PropertyMapper<IView, IViewHandler>
			{
				[nameof(IViewHandler.ContainerView)] = ViewHandler.MapContainerView
			};

		static ExternalBackendViewHandler CreateHandler(IView view, IPropertyMapper mapper = null)
		{
			var handler = new ExternalBackendViewHandler(mapper ?? ContainerOnlyMapper);
			handler.SetVirtualView(view);
			return handler;
		}

		[Fact]
		public void ExternalBackendAssemblyIsNotAFriendOfMauiCore()
		{
			var mauiCore = typeof(ViewHandler).Assembly;
			var externalBackend = typeof(ExternalBackendViewHandler).Assembly;

			Assert.NotSame(mauiCore, externalBackend);

			var friends = mauiCore
				.GetCustomAttributes<InternalsVisibleToAttribute>()
				.Select(a => a.AssemblyName)
				.ToList();

			Assert.DoesNotContain(externalBackend.GetName().Name, friends);
		}

		[Fact]
		public void SetupContainerInstallsContainerViewAndReparentsPlatformView()
		{
			var handler = CreateHandler(new ViewStub());
			var platformView = handler.PlatformView;

			var parent = new ExternalViewGroup();
			var before = new ExternalPlatformView();
			var after = new ExternalPlatformView();
			parent.Add(before);
			parent.Add(platformView);
			parent.Add(after);

			handler.HasContainer = true;

			var container = handler.ContainerView;

			Assert.NotNull(container);
			Assert.Equal(1, handler.SetupContainerCount);
			Assert.Same(platformView, container.Content);
			Assert.Same(container, platformView.Parent);

			// The container takes the exact slot the platform view used to occupy.
			Assert.Equal(1, parent.IndexOf(container));
			Assert.Equal(-1, parent.IndexOf(platformView));
			Assert.Equal(new ExternalPlatformView[] { before, container, after }, parent.Children);
		}

		[Fact]
		public void ContainerViewIsVisibleThroughIViewHandler()
		{
			var handler = CreateHandler(new ViewStub());

			Assert.Null(((IViewHandler)handler).ContainerView);

			handler.HasContainer = true;

			Assert.Same(handler.ContainerView, ((IViewHandler)handler).ContainerView);
			Assert.IsType<ExternalWrapperView>(((IViewHandler)handler).ContainerView);
		}

		[Fact]
		public void RemoveContainerRestoresPlatformViewAndClearsContainerView()
		{
			var handler = CreateHandler(new ViewStub());
			var platformView = handler.PlatformView;

			var parent = new ExternalViewGroup();
			var before = new ExternalPlatformView();
			parent.Add(before);
			parent.Add(platformView);

			handler.HasContainer = true;
			var container = handler.ContainerView;

			handler.HasContainer = false;

			Assert.Equal(1, handler.RemoveContainerCount);
			Assert.Null(handler.ContainerView);
			Assert.Null(((IViewHandler)handler).ContainerView);
			Assert.True(container.IsDisposed);
			Assert.Same(parent, platformView.Parent);
			Assert.Equal(new ExternalPlatformView[] { before, platformView }, parent.Children);
		}

		[Fact]
		public void NeedsContainerTransitionsDriveSetupAndRemoval()
		{
			var view = new ViewStub();
			var handler = CreateHandler(view);

			// Nothing about the view requires a container yet.
			Assert.False(handler.NeedsContainer);
			Assert.False(handler.HasContainer);
			Assert.Null(handler.ContainerView);

			// A gradient/image style background is exactly the scenario an external backend loses
			// when it cannot install a container.
			view.Background = new SolidPaint(Colors.Red);
			handler.UpdateValue(nameof(IViewHandler.ContainerView));

			Assert.True(handler.NeedsContainer);
			Assert.True(handler.HasContainer);
			Assert.NotNull(handler.ContainerView);
			Assert.Equal(1, handler.SetupContainerCount);
			Assert.Equal(0, handler.RemoveContainerCount);

			// A shadow keeps the container alive without setting it up a second time.
			view.Shadow = new ShadowStub();
			handler.UpdateValue(nameof(IViewHandler.ContainerView));

			Assert.True(handler.HasContainer);
			Assert.Equal(1, handler.SetupContainerCount);
			Assert.Equal(0, handler.RemoveContainerCount);

			view.Background = null;
			view.Shadow = null;
			handler.UpdateValue(nameof(IViewHandler.ContainerView));

			Assert.False(handler.NeedsContainer);
			Assert.False(handler.HasContainer);
			Assert.Null(handler.ContainerView);
			Assert.Equal(1, handler.SetupContainerCount);
			Assert.Equal(1, handler.RemoveContainerCount);
		}

		[Fact]
		public void SetupAndRemoveCanRoundTripRepeatedly()
		{
			var handler = CreateHandler(new ViewStub());
			var platformView = handler.PlatformView;

			var parent = new ExternalViewGroup();
			parent.Add(platformView);

			for (var i = 1; i <= 3; i++)
			{
				handler.HasContainer = true;
				Assert.Equal(i, handler.SetupContainerCount);
				Assert.NotNull(handler.ContainerView);

				handler.HasContainer = false;
				Assert.Equal(i, handler.RemoveContainerCount);
				Assert.Null(handler.ContainerView);
				Assert.Same(parent, platformView.Parent);
			}

			Assert.Equal(new ExternalPlatformView[] { platformView }, parent.Children);
		}

		[Fact]
		public void SettingHasContainerToTheSameValueIsANoOp()
		{
			var handler = CreateHandler(new ViewStub());

			handler.HasContainer = true;
			handler.HasContainer = true;

			Assert.Equal(1, handler.SetupContainerCount);

			handler.HasContainer = false;
			handler.HasContainer = false;

			Assert.Equal(1, handler.RemoveContainerCount);
		}

		[Fact]
		public void SetContainerViewInstallsAndClearsTheContainerDirectly()
		{
			var handler = CreateHandler(new ViewStub());
			var wrapper = new ExternalWrapperView();

			handler.SetContainerViewDirectly(wrapper);

			Assert.Same(wrapper, handler.ContainerView);
			Assert.Same(wrapper, ((IViewHandler)handler).ContainerView);

			// SetContainerView only records the container; it never flips HasContainer on its own.
			Assert.False(handler.HasContainer);

			handler.SetContainerViewDirectly(null);

			Assert.Null(handler.ContainerView);
			Assert.Null(((IViewHandler)handler).ContainerView);
		}

		[Fact]
		public void DisconnectingTheHandlerAfterContainerRemovalIsClean()
		{
			var view = new ViewStub();
			var handler = CreateHandler(view);
			var platformView = handler.PlatformView;

			handler.HasContainer = true;
			var container = handler.ContainerView;
			handler.HasContainer = false;

			((IElementHandler)handler).DisconnectHandler();

			Assert.True(container.IsDisposed);
			Assert.Null(handler.ContainerView);
			Assert.Null(((IElementHandler)handler).PlatformView);
			Assert.Null(view.Handler);
			Assert.NotNull(platformView);
		}

		[Fact]
		public void FullViewMapperStillDrivesTheExternalContainer()
		{
			var view = new ViewStub
			{
				Background = new SolidPaint(Colors.Blue)
			};

			var handler = CreateHandler(view, ExternalBackendViewHandler.ExternalBackendMapper);

			Assert.True(handler.HasContainer);
			Assert.NotNull(handler.ContainerView);
			Assert.Same(handler.PlatformView, handler.ContainerView.Content);
		}
	}
}
