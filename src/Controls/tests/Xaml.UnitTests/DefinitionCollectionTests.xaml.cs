// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class DefinitionCollectionTests : ContentPage
	{
		public DefinitionCollectionTests() => InitializeComponent();
		public DefinitionCollectionTests(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}
		public class Tests
		{
			[Theory]
			[InlineData(true)]
			[InlineData(false)]
			public void Method(bool useCompiledXaml)
			{
				var layout = new DefinitionCollectionTests(useCompiledXaml);
				var coldef = layout.grid.ColumnDefinitions;
				var rowdef = layout.grid.RowDefinitions;

				Assert.Equal(5, coldef.Count);

				Assert.Equal(coldef[0].Width, new GridLength(1, GridUnitType.Star));
				Assert.Equal(coldef[1].Width, new GridLength(2, GridUnitType.Star));
				Assert.Equal(coldef[2].Width, new GridLength(1, GridUnitType.Auto));
				Assert.Equal(coldef[3].Width, new GridLength(1, GridUnitType.Star));
				Assert.Equal(coldef[4].Width, new GridLength(300, GridUnitType.Absolute));

				Assert.Equal(5, rowdef.Count);
				Assert.Equal(rowdef[0].Height, new GridLength(1, GridUnitType.Star));
				Assert.Equal(rowdef[1].Height, new GridLength(1, GridUnitType.Auto));
				Assert.Equal(rowdef[2].Height, new GridLength(25, GridUnitType.Absolute));
				Assert.Equal(rowdef[3].Height, new GridLength(14, GridUnitType.Absolute));
				Assert.Equal(rowdef[4].Height, new GridLength(20, GridUnitType.Absolute));
			}

			[Fact]
			public void DefinitionCollectionsReplacedAtCompilation()
			{
				MockCompiler.Compile(typeof(DefinitionCollectionTests), out var methodDef, out var hasLoggedErrors);
				Assert.True(!hasLoggedErrors);
				Assert.True(!methodDef.Body.Instructions.Any(instr => InstructionIsDefColConvCtor(methodDef, instr)), "This Xaml still generates [Row|Col]DefinitionCollectionTypeConverter ctor");
			}

			bool InstructionIsDefColConvCtor(MethodDefinition methodDef, Mono.Cecil.Cil.Instruction instruction)
			{
				if (instruction.OpCode != OpCodes.Newobj)
					return false;
				if (!(instruction.Operand is MethodReference methodRef))
					return false;
				if (Build.Tasks.TypeRefComparer.Default.Equals(methodRef.DeclaringType, methodDef.Module.ImportReference(typeof(Microsoft.Maui.Controls.RowDefinitionCollectionTypeConverter))))
					return true;
				if (Build.Tasks.TypeRefComparer.Default.Equals(methodRef.DeclaringType, methodDef.Module.ImportReference(typeof(Microsoft.Maui.Controls.ColumnDefinitionCollectionTypeConverter))))
					return true;
				return false;
			}
		}
	}
}
