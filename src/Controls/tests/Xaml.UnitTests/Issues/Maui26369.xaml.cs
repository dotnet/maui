using System;
using System.ComponentModel;
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

	public Maui26369() => InitializeComponent();

	public class Test
	{
		public Test()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		public void Dispose()
		{
			AppInfo.SetCurrent(null);
		}

		[Theory]
		[Values]
		public void CompilationDoesNotFail(XamlInflator inflator)
		{
			if (inflator == XamlInflator.XamlC)
			{
				MockCompiler.Compile(typeof(Maui26369), out var methodDef, out var hasLoggedErrors);
				Assert.False(hasLoggedErrors);
				Assert.False(ContainsBoxToNullable(methodDef));
			}

			var page = new Maui26369(inflator);
			Assert.Equal(new GridLength(30), page.NullableGridLength);
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

