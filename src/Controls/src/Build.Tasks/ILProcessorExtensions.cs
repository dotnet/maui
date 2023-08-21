// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using Mono.Cecil.Cil;

namespace Microsoft.Maui.Controls.Build.Tasks
{
	static class ILProcessorExtensions
	{
		public static void Append(this ILProcessor processor, IEnumerable<Instruction> instructions)
		{
			foreach (var instruction in instructions)
				processor.Append(instruction);
		}
	}
}