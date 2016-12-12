using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using System;

namespace Xamarin.Forms.Build.Tasks
{
	static class MethodBodyExtensions
	{
		public static void Optimize(this MethodBody self)
		{
			if (self == null)
				throw new ArgumentNullException(nameof(self));

			self.OptimizeLongs();
			self.OptimizeMacros();
		}

		static void ExpandMacro(Instruction instruction, OpCode opcode, object operand)
		{
			instruction.OpCode = opcode;
			instruction.Operand = operand;
		}

		//this can be removed if/when https://github.com/jbevain/cecil/pull/307 is released in a nuget we consume
		static void OptimizeLongs(this MethodBody self)
		{
			var method = self.Method;
			for (var i = 0; i < self.Instructions.Count; i++) {
				var instruction = self.Instructions[i];
				if (instruction.OpCode.Code != Code.Ldc_I8)
					continue;
				var l = (long)instruction.Operand;
				if (l < int.MinValue || l > int.MaxValue)
					continue;
				ExpandMacro(instruction, OpCodes.Ldc_I4, unchecked((int)l));
				self.Instructions.Insert(++i, Instruction.Create(OpCodes.Conv_I8));
			}
		}
	}
}