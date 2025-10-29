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
		}		class Tests
		{
			[Theory]
			public void Method(bool useCompiledXaml)
			{
				var layout = new DefinitionCollectionTests(useCompiledXaml);
				var coldef = layout.grid.ColumnDefinitions;
				var rowdef = layout.grid.RowDefinitions;

				Assert.Equal(5, coldef.Count);

				Assert.Equal(new GridLength(1, GridUnitType.Star, coldef[0].Width));
				Assert.Equal(new GridLength(2, GridUnitType.Star, coldef[1].Width));
				Assert.Equal(new GridLength(1, GridUnitType.Auto, coldef[2].Width));
				Assert.Equal(new GridLength(1, GridUnitType.Star, coldef[3].Width));
				Assert.Equal(new GridLength(300, GridUnitType.Absolute, coldef[4].Width));

				Assert.Equal(5, rowdef.Count);
				Assert.Equal(new GridLength(1, GridUnitType.Star, rowdef[0].Height));
				Assert.Equal(new GridLength(1, GridUnitType.Auto, rowdef[1].Height));
				Assert.Equal(new GridLength(25, GridUnitType.Absolute, rowdef[2].Height));
				Assert.Equal(new GridLength(14, GridUnitType.Absolute, rowdef[3].Height));
				Assert.Equal(new GridLength(20, GridUnitType.Absolute, rowdef[4].Height));

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
