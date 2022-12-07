using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls.Build.Tasks;
using Microsoft.Maui.Controls.Xaml;
using Mono.Cecil.Cil;

namespace Microsoft.Maui.Controls.XamlC;

class ImageSourceTypeConverter : ICompiledTypeConverter
{
	public IEnumerable<Instruction> ConvertFromString(string value, ILContext context, BaseNode node)
	{
		var module = context.Body.Method.Module;
		if (!string.IsNullOrEmpty(value))
		{
			value = value.Trim();

			if (Uri.TryCreate(value, UriKind.Absolute, out Uri uri) && uri.Scheme != "file")
			{
				yield return Instruction.Create(OpCodes.Ldstr, value);
				yield return Instruction.Create(OpCodes.Ldc_I4_1); // (int)UriKind.Absolute is 1
				yield return Instruction.Create(OpCodes.Newobj, module.ImportCtorReference(context.Cache, ("System", "System", "Uri"), parameterTypes: new[] {
																							("mscorlib", "System", "String"),
																							("System", "System", "UriKind")}));
				yield return Instruction.Create(OpCodes.Call, module.ImportMethodReference(context.Cache, ("Microsoft.Maui.Controls", "Microsoft.Maui.Controls", "ImageSource"), "FromUri", parameterTypes: new[] {
																							("System", "System", "Uri")}, isStatic: true));
			}
			else
			{
				yield return Instruction.Create(OpCodes.Ldstr, value);
				yield return Instruction.Create(OpCodes.Call, module.ImportMethodReference(context.Cache, ("Microsoft.Maui.Controls", "Microsoft.Maui.Controls", "ImageSource"), "FromFile", parameterTypes: new[] {
																							("mscorlib", "System", "String")}, isStatic: true));
			}
			yield break;
		}
		throw new BuildException(BuildExceptionCode.Conversion, node, null, value, typeof(ImageSource));
	}
}
