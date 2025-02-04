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
using NUnit.Framework;

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
	}

	[TestFixture]
	class Test
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
			AppInfo.SetCurrent(null);
		}

		[Test]
		public void CompilationDoesNotFail([Values] bool useCompiledXaml)
		{
			if (useCompiledXaml)
			{
				MockCompiler.Compile(typeof(Maui26369), out var methodDef, out var hasLoggedErrors);
				Assert.IsFalse(hasLoggedErrors);
				Assert.False(ContainsBoxToNullable(methodDef));
			}

			var page = new Maui26369(useCompiledXaml);
			Assert.That(page.NullableGridLength, Is.EqualTo(new GridLength(30)));
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

