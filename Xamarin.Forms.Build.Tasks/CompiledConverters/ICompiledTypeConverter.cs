using System.Collections.Generic;
using Mono.Cecil.Cil;
using Mono.Cecil;
using System.Xml;

namespace Xamarin.Forms.Xaml
{
	interface ICompiledTypeConverter
	{
		IEnumerable<Instruction> ConvertFromString(string value, ModuleDefinition module, BaseNode node);
	}
}