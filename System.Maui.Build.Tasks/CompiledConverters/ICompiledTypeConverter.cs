using System.Collections.Generic;
using Mono.Cecil.Cil;
using Mono.Cecil;
using System.Maui.Xaml;
using System;
using System.Maui.Build.Tasks;

namespace System.Maui.Xaml
{
	interface ICompiledTypeConverter
	{
		IEnumerable<Instruction> ConvertFromString(string value, ILContext context, BaseNode node);
	}
}

namespace System.Maui.Core.XamlC
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