// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Mono.Cecil;
using Mono.Cecil.Cil;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class DefinitionCollectionTests : ContentPage
{
	public DefinitionCollectionTests() => InitializeComponent();

	[TestFixture]
	class Tests
	{
		[Test]
		public void DefinitionCollectionsParsedFromMarkup([Values] XamlInflator inflator)
		{
			var layout = new DefinitionCollectionTests(inflator);
			var coldef = layout.grid.ColumnDefinitions;
			var rowdef = layout.grid.RowDefinitions;

			Assert.That(coldef.Count, Is.EqualTo(5));

			Assert.That(coldef[0].Width, Is.EqualTo(new GridLength(1, GridUnitType.Star)));
			Assert.That(coldef[1].Width, Is.EqualTo(new GridLength(2, GridUnitType.Star)));
			Assert.That(coldef[2].Width, Is.EqualTo(new GridLength(1, GridUnitType.Auto)));
			Assert.That(coldef[3].Width, Is.EqualTo(new GridLength(1, GridUnitType.Star)));
			Assert.That(coldef[4].Width, Is.EqualTo(new GridLength(300, GridUnitType.Absolute)));

			Assert.That(rowdef.Count, Is.EqualTo(5));
			Assert.That(rowdef[0].Height, Is.EqualTo(new GridLength(1, GridUnitType.Star)));
			Assert.That(rowdef[1].Height, Is.EqualTo(new GridLength(1, GridUnitType.Auto)));
			Assert.That(rowdef[2].Height, Is.EqualTo(new GridLength(25, GridUnitType.Absolute)));
			Assert.That(rowdef[3].Height, Is.EqualTo(new GridLength(14, GridUnitType.Absolute)));
			Assert.That(rowdef[4].Height, Is.EqualTo(new GridLength(20, GridUnitType.Absolute)));

		}

		[Test]
		public void DefinitionCollectionsReplacedAtCompilation()
		{
			MockCompiler.Compile(typeof(DefinitionCollectionTests), out var methodDef, out var hasLoggedErrors);
			Assert.That(!hasLoggedErrors);
			Assert.That(!methodDef.Body.Instructions.Any(instr => InstructionIsDefColConvCtor(methodDef, instr)), "This Xaml still generates [Row|Col]DefinitionCollectionTypeConverter ctor");
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
