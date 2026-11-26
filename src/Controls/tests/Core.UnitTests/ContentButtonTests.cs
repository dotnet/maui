using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using NSubstitute;
using Xunit;
using static Microsoft.Maui.Controls.Core.UnitTests.VisualStateTestHelpers;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class ContentButtonTests : VisualElementCommandSourceTests<ContentButton>
	{
		protected override ContentButton CreateSource() => new ContentButton();
		protected override void Activate(ContentButton source) => ((IButton)source).Clicked();
		protected override BindableProperty IsEnabledProperty => VisualElement.IsEnabledProperty;
		protected override BindableProperty CommandProperty => ContentButton.CommandProperty;
		protected override BindableProperty CommandParameterProperty => ContentButton.CommandParameterProperty;

		[Fact]
		public void DefaultsMatchButtonBorderSurface()
		{
			var button = new ContentButton();
			var textButton = new Button();
			Assert.Equal(textButton.BorderColor, button.BorderColor);
			Assert.Equal(textButton.BorderWidth, button.BorderWidth);
			Assert.Equal(textButton.CornerRadius, button.CornerRadius);
			Assert.Null(button.Content);
			Assert.False(button.IsPressed);
		}

		[Fact]
		public void CommandExecutesBeforeClickedEvenWhenItDisablesButton()
		{
			var calls = new List<string>();
			var button = new ContentButton();
			button.Command = new Command(() =>
			{
				calls.Add("Command");
				button.IsEnabled = false;
			});
			button.Clicked += (_, _) => calls.Add("Clicked");
			((IButton)button).Clicked();
			Assert.Equal(new[] { "Command", "Clicked" }, calls);
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public void PressAndReleaseMatchButtonVisualStates(bool enabled)
		{
			var button = new ContentButton { IsEnabled = enabled };
			var groups = CreateTestStateGroups();
			VisualStateManager.SetVisualStateGroups(button, groups);
			int pressed = 0, released = 0;
			button.Pressed += (_, _) => pressed++;
			button.Released += (_, _) => released++;

			((IButton)button).Pressed();
			Assert.Equal(enabled, button.IsPressed);
			Assert.Equal(enabled ? 1 : 0, pressed);
			if (enabled)
				Assert.Equal(PressedStateName, groups[0].CurrentState.Name);

			button.IsEnabled = false;
			((IButton)button).Released();
			Assert.False(button.IsPressed);
			Assert.Equal(1, released);
			Assert.Equal(DisabledStateName, groups[0].CurrentState.Name);
		}

		[Fact]
		public void DisablingInPressedHandlerStillAllowsRelease()
		{
			var button = new ContentButton();
			button.Pressed += (_, _) => button.IsEnabled = false;
			((IButton)button).Pressed();
			((IButton)button).Released();
			Assert.False(button.IsPressed);
		}

		[Fact]
		public void IsPressedCannotBeAssigned()
		{
			var button = new ContentButton();
			Assert.True(ContentButton.IsPressedProperty.IsReadOnly);
			button.SetValue(ContentButton.IsPressedProperty, true);
			Assert.False(button.IsPressed);
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public void CommandAndParameterBindingOrder(bool commandFirst)
		{
			var parameter = new object();
			var seen = new List<object>();
			var context = new
			{
				Action = new Command(p => { }, p =>
				{
					seen.Add(p);
					return ReferenceEquals(p, parameter);
				}),
				Parameter = parameter
			};
			var button = new ContentButton();
			if (commandFirst)
				button.SetBinding(ContentButton.CommandProperty, "Action");
			button.SetBinding(ContentButton.CommandParameterProperty, "Parameter");
			if (!commandFirst)
				button.SetBinding(ContentButton.CommandProperty, "Action");

			button.BindingContext = context;
			Assert.True(button.IsEnabled);
			Assert.NotEmpty(seen);
			Assert.All(seen, p => Assert.Same(parameter, p));

			var parent = new Grid { BindingContext = context };
			button.ClearValue(BindableObject.BindingContextProperty);
			parent.Add(button);
			Assert.True(button.IsEnabled);
			Assert.Same(parameter, button.CommandParameter);
		}

		[Fact]
		public void ReplacingCommandUnsubscribesOldCommand()
		{
			var oldCommand = new TrackingCommand();
			var command = new TrackingCommand();
			var button = new ContentButton { Command = oldCommand };
			Assert.Equal(1, oldCommand.Subscribers);
			button.Command = command;
			Assert.Equal(0, oldCommand.Subscribers);
			Assert.Equal(1, command.Subscribers);
			button.Command = null;
			Assert.Equal(0, command.Subscribers);
		}

		[Fact]
		public async Task CommandDoesNotRetainButton()
		{
			var command = new TrackingCommand();
			var reference = CreateWeakButton(command);
			await TestHelpers.Collect();
			Assert.False(reference.IsAlive);
			command.RaiseCanExecuteChanged();
			GC.KeepAlive(command);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		static WeakReference CreateWeakButton(ICommand command) => new WeakReference(new ContentButton { Command = command });

		[Fact]
		public void ContentReplacementMaintainsLogicalTreeAndBindingContext()
		{
			var context = new object();
			var first = new Label();
			var second = new Label();
			var button = new ContentButton { BindingContext = context, Content = first };
			Assert.Same(button, first.Parent);
			Assert.Same(context, first.BindingContext);

			button.Content = second;
			Assert.Null(first.Parent);
			Assert.Same(button, second.Parent);
			Assert.Same(context, second.BindingContext);
			Assert.Single(((IElementController)button).LogicalChildren);

			button.Content = null;
			Assert.Null(second.Parent);
			Assert.Empty(((IElementController)button).LogicalChildren);
		}

		[Fact]
		public void ExplicitChildBindingContextSurvivesParentChanges()
		{
			var childContext = new object();
			var child = new Label { BindingContext = childContext };
			var button = new ContentButton { BindingContext = new object(), Content = child };
			button.BindingContext = new object();
			Assert.Same(childContext, child.BindingContext);
		}

		[Fact]
		public void ContentInheritsDynamicResources()
		{
			var child = new Label();
			child.SetDynamicResource(Label.TextColorProperty, "Foreground");
			var button = new ContentButton { Content = child };
			button.Resources["Foreground"] = Colors.Red;
			Assert.Equal(Colors.Red, child.TextColor);
			button.Resources["Foreground"] = Colors.Blue;
			Assert.Equal(Colors.Blue, child.TextColor);
		}

		[Fact]
		public void ControlTemplatePresentsContentAndCanBeRemoved()
		{
			var child = new Label { Text = "Content" };
			var button = new ContentButton { Content = child };
			button.ControlTemplate = new ControlTemplate(() => new ContentPresenter());
			var presenter = Assert.IsType<ContentPresenter>(((IContentView)button).PresentedContent);
			Assert.Same(child, presenter.Content);
			button.ControlTemplate = null;
			Assert.Same(child, ((IContentView)button).PresentedContent);
		}

		[Theory]
		[InlineData(0)]
		[InlineData(5)]
		public void BorderWidthInvalidatesMeasure(double width)
		{
			var button = new ContentButton();
			bool invalidated = false;
			button.MeasureInvalidated += (_, _) => invalidated = true;
			button.BorderWidth = width;
			Assert.True(invalidated);
		}

		[Theory]
		[InlineData(FlowDirection.LeftToRight, 10)]
		[InlineData(FlowDirection.RightToLeft, 10)]
		public void ContentLayoutHonorsMarginsAlignmentAndFlowDirection(FlowDirection direction, double expectedX)
		{
			var handler = Substitute.For<IViewHandler>();
			handler.GetDesiredSize(Arg.Any<double>(), Arg.Any<double>()).Returns(new Size(20, 10));
			var child = new View
			{
				Handler = handler,
				IsPlatformEnabled = true,
				Margin = new Thickness(2, 3, 4, 5),
				HorizontalOptions = LayoutOptions.Start,
				VerticalOptions = LayoutOptions.Center,
			};
			var button = new ContentButton
			{
				IsPlatformEnabled = true,
				Content = child,
				FlowDirection = direction,
				Padding = 5,
				BorderWidth = 3,
			};
			var layout = (ICrossPlatformLayout)button;
			layout.CrossPlatformMeasure(100, 80);
			layout.CrossPlatformArrange(new Rect(0, 0, 100, 80));
			Assert.Equal(new Rect(expectedX, 34, 20, 10), child.Bounds);
			Assert.Equal(direction == FlowDirection.RightToLeft,
				((IFlowDirectionController)child).EffectiveFlowDirection.IsRightToLeft());
		}

		[Fact]
		public void BuiltInRegistrationDoesNotFallBackToContentView()
		{
			var builder = MauiApp.CreateBuilder().UseMauiApp<ApplicationStub>();
			using var app = builder.Build();
			var handlers = app.Services.GetRequiredService<IMauiHandlersFactory>();
			Assert.IsType<ContentButtonHandler>(handlers.GetHandler(typeof(IContentButton)));
			Assert.IsType<ContentButtonHandler>(handlers.GetHandler(typeof(ContentButton)));
		}

		sealed class TrackingCommand : ICommand
		{
			EventHandler _changed;
			public int Subscribers { get; private set; }
			public event EventHandler CanExecuteChanged
			{
				add { _changed += value; Subscribers++; }
				remove { _changed -= value; Subscribers--; }
			}
			public bool CanExecute(object parameter) => true;
			public void Execute(object parameter) { }
			public void RaiseCanExecuteChanged() => _changed?.Invoke(this, EventArgs.Empty);
		}
	}
}
