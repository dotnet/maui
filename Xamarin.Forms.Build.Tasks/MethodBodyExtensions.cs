using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

namespace Xamarin.Forms.Build.Tasks
{
	static class MethodBodyExtensions
	{
		public static void Optimize(this MethodBody self)
		{
			if (self == null)
				throw new ArgumentNullException(nameof(self));

			self.OptimizeLongs();
			self.OptimizeStLdLoc();
			self.RemoveUnusedLocals();
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
			for (var i = 0; i < self.Instructions.Count; i++)
			{
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

		static void OptimizeStLdLoc(this MethodBody self)
		{
			var method = self.Method;
			for (var i = 0; i < self.Instructions.Count; i++)
			{
				var instruction = self.Instructions[i];
				if (instruction.OpCode.Code != Code.Stloc)
					continue;
				if (i + 1 >= self.Instructions.Count)
					continue;
				var next = self.Instructions[i + 1];
				int num = ((VariableDefinition)instruction.Operand).Index;
				var vardef = instruction.Operand;
				if (next.OpCode.Code != Code.Ldloc || num != ((VariableDefinition)next.Operand).Index)
					continue;
				ExpandMacro(instruction, OpCodes.Dup, null);
				ExpandMacro(next, OpCodes.Stloc, vardef);
			}
		}

		static void RemoveUnusedLocals(this MethodBody self)
		{
			//Count ldloc for each variable
			var ldlocUsed = new List<VariableDefinition>();
			foreach (var instruction in self.Instructions)
			{
				if (instruction.OpCode.Code != Code.Ldloc)
					continue;
				var varDef = (VariableDefinition)instruction.Operand;
				if (!ldlocUsed.Contains(varDef))
					ldlocUsed.Add(varDef);
			}

			foreach (var varDef in self.Variables.ToArray())
			{
				if (ldlocUsed.Contains(varDef))
					continue;

				//find the Stloc instruction
				var instruction = (from instr in self.Instructions where instr.OpCode.Code == Code.Stloc && instr.Operand == varDef select instr).First();

				//remove dup/stloc
				if (instruction.Previous.OpCode.Code != Code.Dup)
					break;

				self.Instructions.Remove(instruction.Previous);
				self.Instructions.Remove(instruction);

				//and remove the variable
				self.Variables.Remove(varDef);
			}
		}
	}
}