using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui26369 : ContentPage
{
	public static readonly BindableProperty NullableGridLengthProperty =
		BindableProperty.Create(nameof(NullableGridLength), typeof(GridLength?), typeof(Maui26369), null);

	[TypeConverter(typeof(GridLengthTypeConverter))]
	public GridLength? NullableGridLength
	{
		get => (GridLength?)GetValue(NullableGridLengthProperty);
		set => SetValue(NullableGridLengthProperty, value);
	}

	public Maui26369()
	{
		InitializeComponent();
	}

	public Maui26369(bool useCompiledXaml)
	{
		//this stub will be replaced at compile time
	}	class Test
	{
		// NOTE: xUnit uses constructor for setup. This may need manual conversion.
		// [SetUp]
		public void Setup()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		// NOTE: xUnit uses IDisposable.Dispose() for teardown. This may need manual conversion.
		// [TearDown]
		public void TearDown()
		{
			AppInfo.SetCurrent(null);
		}

		[Fact]
		public void CompilationDoesNotFail([Theory]
		[InlineData(true)]
		[InlineData(false)] bool useCompiledXaml)
		{
			if (useCompiledXaml)
			{
				MockCompiler.Compile(typeof(Maui26369), out var methodDef, out var hasLoggedErrors);
				Assert.False(hasLoggedErrors);
				Assert.False(ContainsBoxToNullable(methodDef));
			}

			var page = new Maui26369(useCompiledXaml);
			Assert.Equal(new GridLength(30, page.NullableGridLength));
		}

		private bool ContainsBoxToNullable(MethodDefinition methodDef)
		{
			//{IL_0000: box System.Nullable`1<Microsoft.Maui.GridLength>}
			return methodDef.Body.Instructions.Any(instruction =>
				instruction.OpCode == OpCodes.Box
				&& instruction.Operand is GenericInstanceType git
				&& git.FullName == "System.Nullable`1<Microsoft.Maui.GridLength>");
		}
	}
}

