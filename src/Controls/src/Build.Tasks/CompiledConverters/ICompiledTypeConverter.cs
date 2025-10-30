using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls.Build.Tasks;
using Microsoft.Maui.Controls.Xaml;
using Mono.Cecil.Cil;

namespace Microsoft.Maui.Controls.Xaml
{
	interface ICompiledTypeConverter
	{
		IEnumerable<Instruction> ConvertFromString(string value, ILContext context, BaseNode node);
	}
}

namespace Microsoft.Maui.Controls.XamlC
{
	//only used in unit tests to make sure the compiled InitializeComponent is invoked
	class IsCompiledTypeConverter : ICompiledTypeConverter
	{
		public IEnumerable<Instruction> ConvertFromString(string value, ILContext context, BaseNode node)
		{
			if (value != "IsCompiled?")
				throw new Exception();
			yield return Instruction.Create(OpCodes.Ldc_I4_1);
		}
	}
}