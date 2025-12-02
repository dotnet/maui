using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Maui.Controls.BindingSourceGen;
using Xunit;

namespace BindingSourceGen.UnitTests;

public class RelayCommandDebugTests
{
	[Fact]
	public void DebugRelayCommandDetection()
	{
		var source = """
			namespace System.Windows.Input
			{
				public interface ICommand
				{
					event System.EventHandler CanExecuteChanged;
					bool CanExecute(object parameter);
					void Execute(object parameter);
				}
			}

			namespace CommunityToolkit.Mvvm.Input
			{
				[System.AttributeUsage(System.AttributeTargets.Method)]
				public class RelayCommandAttribute : System.Attribute { }
			}

			namespace TestApp
			{
				public class MyViewModel
				{
					[CommunityToolkit.Mvvm.Input.RelayCommand]
					private void Save()
					{
					}
				}
			}
			""";

		var compilation = SourceGenHelpers.CreateCompilation(source);
		
		// Get the MyViewModel type
		var myViewModelType = compilation.GetTypeByMetadataName("TestApp.MyViewModel");
		Assert.NotNull(myViewModelType);
		
		// Try to find the Save method
		var saveMethods = myViewModelType!.GetMembers("Save").OfType<IMethodSymbol>();
		Assert.NotEmpty(saveMethods);
		
		var saveMethod = saveMethods.First();
		Assert.NotNull(saveMethod);
		
		// Check for RelayCommand attribute
		var attributes = saveMethod.GetAttributes();
		Assert.NotEmpty(attributes);
		
		var hasRelayCommand = attributes.Any(attr =>
			attr.AttributeClass?.Name == "RelayCommandAttribute" ||
			attr.AttributeClass?.ToDisplayString() == "CommunityToolkit.Mvvm.Input.RelayCommandAttribute");
		
		Assert.True(hasRelayCommand, "Save method should have RelayCommand attribute");
		
		// Try the TryGetRelayCommandPropertyType method
		var result = myViewModelType.TryGetRelayCommandPropertyType("SaveCommand", compilation, out var commandType);
		Assert.True(result, "Should find SaveCommand property type");
		Assert.NotNull(commandType);
		Assert.Equal("System.Windows.Input.ICommand", commandType!.ToDisplayString());
	}
}
