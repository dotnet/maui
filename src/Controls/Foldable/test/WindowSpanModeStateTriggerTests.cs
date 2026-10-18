using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Foldable;
using Microsoft.Maui.Foldable;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Foldable.UnitTests
{
	public class WindowSpanModeStateTriggerTests
	{
		[Fact]
		public void UsesAttachedVisualElementInsteadOfGlobalCurrent()
		{
			var element = new Grid();
			var localService = new TestFoldableService
			{
				Hinge = new Rect(490, 0, 20, 1000),
			};
			var globalService = new TestFoldableService
			{
				Hinge = new Rect(0, 490, 1000, 20),
				Landscape = true,
			};
			element.Handler = new TestElementHandler(localService);
			DualScreenInfo.Current.SetFoldableService(globalService);

			var trigger = new WindowSpanModeStateTrigger
			{
				SpanMode = TwoPaneViewMode.Wide,
			};
			var state = new VisualState { Name = "Wide" };
			state.StateTriggers.Add(trigger);
			var group = new VisualStateGroup();
			group.States.Add(state);
			group.VisualElement = element;
			state.VisualStateGroup = group;
			trigger.VisualState = state;
			trigger.SendAttached();

			var infoField = typeof(WindowSpanModeStateTrigger).GetField("_info", BindingFlags.Instance | BindingFlags.NonPublic);
			Assert.NotNull(infoField);
			var info = Assert.IsType<DualScreenInfo>(infoField.GetValue(trigger));
			Assert.Same(element, info.Element);

			var guideField = typeof(DualScreenInfo).GetField("_twoPaneViewLayoutGuide", BindingFlags.Instance | BindingFlags.NonPublic);
			Assert.NotNull(guideField);
			var guide = Assert.IsType<TwoPaneViewLayoutGuide>(guideField.GetValue(info));
			guide.UpdateLayouts(1000, 1000);

			Assert.Equal(TwoPaneViewMode.Tall, DualScreenInfo.Current.SpanMode);
			Assert.Equal(TwoPaneViewMode.Wide, info.SpanMode);
			Assert.True(trigger.IsActive);

			localService.Hinge = new Rect(0, 490, 1000, 20);
			localService.Landscape = true;
			localService.RaiseLayoutChanged();

			Assert.Equal(TwoPaneViewMode.Tall, info.SpanMode);
			Assert.False(trigger.IsActive);

			globalService.Hinge = new Rect(490, 0, 20, 1000);
			globalService.Landscape = false;
			globalService.RaiseLayoutChanged();

			Assert.Equal(TwoPaneViewMode.Wide, DualScreenInfo.Current.SpanMode);
			Assert.False(trigger.IsActive);

			var propertyChangedField = typeof(DualScreenInfo).GetField("PropertyChanged", BindingFlags.Instance | BindingFlags.NonPublic);
			Assert.NotNull(propertyChangedField);
			Assert.Contains(
				((PropertyChangedEventHandler)propertyChangedField.GetValue(info)).GetInvocationList(),
				handler => ReferenceEquals(handler.Target, trigger));

			trigger.SendDetached();

			var detachedHandlers = propertyChangedField.GetValue(info) as PropertyChangedEventHandler;
			Assert.True(detachedHandlers == null ||
				!detachedHandlers.GetInvocationList().Any(handler => ReferenceEquals(handler.Target, trigger)));

			trigger.SendAttached();

			Assert.Single(
				((PropertyChangedEventHandler)propertyChangedField.GetValue(info)).GetInvocationList(),
				handler => ReferenceEquals(handler.Target, trigger));
		}

		sealed class TestFoldableService : IFoldableService
		{
			public event System.EventHandler OnScreenChanged
			{
				add { }
				remove { }
			}

			public event System.EventHandler<FoldableHingeAngleChangedEventArgs> HingeAngleChanged
			{
				add { }
				remove { }
			}

			public event System.EventHandler<FoldEventArgs> OnLayoutChanged;

			public bool IsSpanned => Hinge != Rect.Zero;
			public bool IsLandscape => Landscape;
			public bool Landscape { get; set; }
			public Rect Hinge { get; set; }
			public Size ScaledScreenSize => new Size(1000, 1000);

			public Rect GetHinge() => Hinge;
			public Rect GetHinge(VisualElement visualElement) => Hinge;
			public bool IsLandscapeFor(VisualElement visualElement) => Landscape;
			public Size GetScaledScreenSize(VisualElement visualElement) => ScaledScreenSize;
			public Point? GetLocationOnScreen(VisualElement visualElement) => Point.Zero;
			public Task<int> GetHingeAngleAsync() => Task.FromResult(0);
			public void StartMonitoring(VisualElement visualElement) { }
			public void StopMonitoring(VisualElement visualElement) { }

			public void RaiseLayoutChanged()
			{
				OnLayoutChanged?.Invoke(this, new FoldEventArgs());
			}
		}

		sealed class TestElementHandler : IViewHandler
		{
			public TestElementHandler(IFoldableService service)
			{
				var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection()
					.AddSingleton(service)
					.BuildServiceProvider();
				MauiContext = new MauiContext(services);
			}

			public object PlatformView => null;
			public object ContainerView => null;
			public IView VirtualView { get; private set; }
			IElement IElementHandler.VirtualView => VirtualView;
			public IMauiContext MauiContext { get; private set; }
			public bool HasContainer { get; set; }

			public void DisconnectHandler() { }
			public void Invoke(string command, object args = null) { }
			public void SetMauiContext(IMauiContext mauiContext) => MauiContext = mauiContext;
			public void SetVirtualView(IElement view) => VirtualView = (IView)view;
			public void UpdateValue(string property) { }
			public Size GetDesiredSize(double widthConstraint, double heightConstraint) => Size.Zero;
			public void PlatformArrange(Rect frame) { }
		}
	}
}
