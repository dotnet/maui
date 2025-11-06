using System;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests.Issues;

public partial class CommandParameterReparenting : ContentPage
{
	public CommandParameterReparenting() => InitializeComponent();

	[TestFixture]
	class Tests
	{
		[SetUp] 
		public void Setup() => DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		
		[TearDown] 
		public void TearDown() => DispatcherProvider.SetCurrent(null);

		[Test]
		public async Task CommandParameterShouldNotBeNullDuringReparenting([Values] XamlInflator inflator)
		{
			var page = new CommandParameterReparenting(inflator);
			var footerControl = page.FooterControlInstance;

			// Create a command that tracks whether it was called with null parameter
			bool wasCalledWithNull = false;
			object lastParameter = null;
			int callCount = 0;
			
			var command = new Command<object>(
				execute: (param) => { },
				canExecute: (param) => 
				{
					callCount++;
					lastParameter = param;
					if (param == null)
					{
						wasCalledWithNull = true;
					}
					return param != null;
				}
			);

			// Set the command BEFORE reparenting to ensure it's bound when reparenting happens
			footerControl.TestCommand = command;

			// Get the button before reparenting
			var button = (Button)footerControl.GetTemplateChild("TestButton");
			
			// Verify initial state - the button should have the CommandParameter set
			Assert.AreEqual("TestValue", button.CommandParameter, 
				"CommandParameter should be 'TestValue' initially");

			// Now trigger the reparenting operation that causes the issue
			var grid = (Grid)footerControl.GetTemplateChild("MainLayout");

			// Track calls during reparenting
			int callCountBeforeReparent = callCount;
			
			// This operation triggers the timing issue: clearing and re-adding children
			// causes bindings to be re-applied, and the async nature of relative binding
			// can cause Command to be evaluated before CommandParameter is set
			grid.Children.Clear();
			grid.Children.Add(button);

			// Wait a bit for async bindings to complete
			await Task.Delay(100);

			int callCountDuringReparent = callCount - callCountBeforeReparent;

			// The test should fail if CommandParameter was null during CanExecute evaluation
			Assert.IsFalse(wasCalledWithNull, 
				$"Command.CanExecute was called with null parameter during reparenting. " +
				$"Last parameter value: {lastParameter}, Calls during reparent: {callCountDuringReparent}");

			// Also verify the parameter is correctly set after reparenting
			Assert.AreEqual("TestValue", button.CommandParameter, 
				"CommandParameter should be 'TestValue' after reparenting");
		}
	}
}

public class FooterControl : ContentView
{
	public static readonly BindableProperty TestCommandProperty =
		BindableProperty.Create(nameof(TestCommand), typeof(ICommand), typeof(FooterControl), null);

	public static readonly BindableProperty TestCommandParameterProperty =
		BindableProperty.Create(nameof(TestCommandParameter), typeof(object), typeof(FooterControl), null);

	public ICommand TestCommand
	{
		get => (ICommand)GetValue(TestCommandProperty);
		set => SetValue(TestCommandProperty, value);
	}

	public object TestCommandParameter
	{
		get => GetValue(TestCommandParameterProperty);
		set => SetValue(TestCommandParameterProperty, value);
	}
}
