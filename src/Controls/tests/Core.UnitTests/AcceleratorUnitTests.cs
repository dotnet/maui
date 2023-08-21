// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Controls.Internals;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class AcceleratorUnitTests : BaseTestFixture
	{

		[Fact]
		public void AcceleratorThrowsOnEmptyString()
		{
			Assert.Throws<ArgumentNullException>(() => Accelerator.FromString(""));
		}

		[Fact]
		public void AcceleratorThrowsOnNull()
		{
			Assert.Throws<ArgumentNullException>(() => Accelerator.FromString(null));
		}

		[Theory]
		[InlineData("ctrl+A")]
		[InlineData("cmd+A")]
		[InlineData("ctrl+1")]
		[InlineData("cmd+1")]
		[InlineData("ctrl+alt+A")]
		[InlineData("cmd+alt+A")]
		[InlineData("A")]
		public void AcceleratorFromString(string shortCutKeyBinding)
		{
			var accelerator = Accelerator.FromString(shortCutKeyBinding);

			Assert.Equal(shortCutKeyBinding, accelerator.ToString());
		}

		[Theory]
		[InlineData("A")]
		[InlineData("B")]
		[InlineData("1")]
		[InlineData("2")]
		public void AcceleratorFromOnlyLetter(string shortCutKeyBinding)
		{
			var accelerator = Accelerator.FromString(shortCutKeyBinding);

			Assert.Single(accelerator.Keys);
			Assert.Equal(accelerator.Keys.ElementAt(0), shortCutKeyBinding);
		}

		[Theory, MemberData(nameof(ShortcutTestData))]
		public void AcceleratorFromLetterAndModifier(TestShortcut shortcut)
		{
			string modifier = shortcut.Modifier;
			Assert.NotNull(modifier);

			string key = shortcut.Key;
			Assert.NotNull(key);

			var accelerator = Accelerator.FromString(shortcut.ToString());
			Assert.Single(accelerator.Keys);
			Assert.Single(accelerator.Modifiers);
			Assert.Equal(accelerator.Keys.ElementAt(0), shortcut.Key);
			Assert.Equal(accelerator.Modifiers.ElementAt(0), shortcut.Modifier);
		}

		[Fact]
		public void AcceleratorFromLetterAnd2Modifier()
		{
			string modifier = "ctrl";
			string modifier1Alt = "alt";
			string key = "A";
			string shortCutKeyBinding = $"{modifier}+{modifier1Alt}+{key}";
			var accelerator = Accelerator.FromString(shortCutKeyBinding);

			Assert.Single(accelerator.Keys);
			Assert.Equal(2, accelerator.Modifiers.Count());
			Assert.Equal(accelerator.Keys.ElementAt(0), key);
			Assert.Equal(accelerator.Modifiers.ElementAt(0), modifier);
			Assert.Equal(accelerator.Modifiers.ElementAt(1), modifier1Alt);
		}


		[Preserve(AllMembers = true)]
#pragma warning disable CA1815 // Override equals and operator equals on value types
		public struct TestShortcut
#pragma warning restore CA1815 // Override equals and operator equals on value types
		{
			internal TestShortcut(string modifier)
			{
				Modifier = modifier;
				Key = modifier[0].ToString();
			}

			internal string Modifier { get; set; }
			internal string Key { get; set; }

			public override string ToString()
			{
				return $"{Modifier}+{Key}";
			}
		}

		static IEnumerable<TestShortcut> GenerateTests
		{
			get { return new string[] { "ctrl", "cmd", "alt", "shift", "fn", "win" }.Select(str => new TestShortcut(str)); }
		}

		public static IEnumerable<object[]> ShortcutTestData()
		{
			foreach (var sc in GenerateTests)
			{
				yield return new object[] { sc };
			}
		}
	}
}
