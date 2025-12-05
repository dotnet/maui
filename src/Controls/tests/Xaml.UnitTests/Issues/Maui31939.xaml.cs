// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using System;
using System.Collections.Generic;
using System.Windows.Input;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

// Regression test for https://github.com/dotnet/maui/issues/31939
// CommandParameter TemplateBinding is lost during ControlTemplate reparenting,
// causing CanExecute to be called with null parameter before CommandParameter is resolved.
public partial class Maui31939 : ContentPage
{
	public Maui31939() => InitializeComponent();

	[TestFixture]
	class Tests
	{
		[SetUp]
		public void Setup()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		[TearDown]
		public void TearDown()
		{
			DispatcherProvider.SetCurrent(null);
		}

		[Test]
		public void CommandParameterTemplateBindingShouldNotBeNullWhenCanExecuteIsCalled([Values] XamlInflator inflator)
		{
			// Verify initial template binding works correctly: CommandParameter should be resolved
			// before CanExecute is called when template is first applied.
			var viewModel = new Maui31939ViewModel();
			var page = new Maui31939(inflator);
			page.BindingContext = viewModel;

			Assert.That(viewModel.CanExecuteCalledWithNullParameter, Is.False, 
				"CanExecute was called with null parameter during template binding application");

			var button = (Button)page.TestControl.GetTemplateChild("TestButton");
			Assert.That(button, Is.Not.Null);
			Assert.That(button.CommandParameter, Is.EqualTo("TestValue"));
			Assert.That(button.Command, Is.Not.Null);
		}

		[Test]
		public void CommandParameterTemplateBindingWorksAfterReparenting([Values] XamlInflator inflator)
		{
			// Regression test: when elements are reparented within a ControlTemplate, bindings
			// are re-applied. Due to the async void ApplyRelativeSourceBinding path, Command may
			// be applied before CommandParameter, causing CanExecute(null) to be called.
			var viewModel = new Maui31939ViewModel();
			var page = new Maui31939(inflator);
			page.BindingContext = viewModel;

			var grid = (Grid)page.TestControl.GetTemplateChild("MainLayout");
			var button = (Button)page.TestControl.GetTemplateChild("TestButton");

			Assert.That(button, Is.Not.Null);
			Assert.That(button.CommandParameter, Is.EqualTo("TestValue"));

			// Simulate reparenting operation (like the issue describes)
			viewModel.ResetCanExecuteTracking();
			grid.Children.Clear();
			grid.Children.Add(button);

			// After reparenting, CommandParameter should still be bound correctly
			// and CanExecute should not have been called with null
			Assert.That(viewModel.CanExecuteCalledWithNullParameter, Is.False,
				"CanExecute was called with null parameter after reparenting");
			Assert.That(button.CommandParameter, Is.EqualTo("TestValue"));
		}
	}
}

/// <summary>
/// Custom control with Command and CommandParameter bindable properties
/// for testing TemplateBinding scenarios.
/// </summary>
public class Maui31939Control : ContentView
{
	public static readonly BindableProperty TestCommandProperty =
		BindableProperty.Create(nameof(TestCommand), typeof(ICommand), typeof(Maui31939Control), null);

	public static readonly BindableProperty TestCommandParameterProperty =
		BindableProperty.Create(nameof(TestCommandParameter), typeof(object), typeof(Maui31939Control), null);

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

/// <summary>
/// ViewModel with a Command that tracks whether CanExecute was called with null parameter.
/// This simulates the real-world scenario where apps have commands that expect non-null parameters.
/// </summary>
public class Maui31939ViewModel
{
	public bool CanExecuteCalledWithNullParameter { get; private set; }
	private bool _isEnabled = true;

	public Maui31939ViewModel()
	{
		TestCommand = new Maui31939Command(
			execute: parameter => { /* Do nothing */ },
			canExecute: parameter =>
			{
				if (parameter is null)
				{
					CanExecuteCalledWithNullParameter = true;
				}
				return _isEnabled;
			});
	}

	public ICommand TestCommand { get; }

	public void ResetCanExecuteTracking()
	{
		CanExecuteCalledWithNullParameter = false;
	}
}

/// <summary>
/// Custom command implementation that allows tracking CanExecute calls.
/// </summary>
public class Maui31939Command : ICommand
{
	private readonly Action<object> _execute;
	private readonly Func<object, bool> _canExecute;

	public event EventHandler CanExecuteChanged;

	public Maui31939Command(Action<object> execute, Func<object, bool> canExecute = null)
	{
		_execute = execute ?? throw new ArgumentNullException(nameof(execute));
		_canExecute = canExecute;
	}

	public bool CanExecute(object parameter) => _canExecute?.Invoke(parameter) ?? true;

	public void Execute(object parameter) => _execute(parameter);

	public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}
