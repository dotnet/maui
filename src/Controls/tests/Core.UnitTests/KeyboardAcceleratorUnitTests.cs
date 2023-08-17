using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Controls.Internals;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class KeyboardAcceleratorUnitTests : BaseTestFixture
	{

		[Fact]
		public void KeyboardAcceleratorThrowsOnEmptyString()
		{
			Assert.Throws<ArgumentNullException>(() => KeyboardAccelerator.FromString(""));
		}

		[Fact]
		public void KeyboardAcceleratorThrowsOnNull()
		{
			Assert.Throws<ArgumentNullException>(() => KeyboardAccelerator.FromString(null));
		}

		[Theory]
		[InlineData("ctrl+A")]
		[InlineData("cmd+A")]
		[InlineData("ctrl+1")]
		[InlineData("cmd+1")]
		[InlineData("ctrl+alt+A")]
		[InlineData("cmd+alt+A")]
		[InlineData("A")]
		public void KeyboardAcceleratorFromString(string shortCutKeyBinding)
		{
			var accelerator = KeyboardAccelerator.FromString(shortCutKeyBinding);

			Assert.Equal(shortCutKeyBinding, accelerator.ToString());
		}

		[Theory]
		[InlineData("A")]
		[InlineData("B")]
		[InlineData("1")]
		[InlineData("2")]
		public void KeyboardAcceleratorFromOnlyLetter(string shortCutKeyBinding)
		{
			var accelerator = KeyboardAccelerator.FromString(shortCutKeyBinding);

			Assert.Single(accelerator.Key);
			Assert.Equal(accelerator.Key, shortCutKeyBinding);
		}

		[Theory, MemberData(nameof(ShortcutTestData))]
		public void KeyboardAcceleratorFromLetterAndModifier(TestShortcut shortcut)
		{
			string modifier = shortcut.Modifier;
			Assert.NotNull(modifier);

			string key = shortcut.Key;
			Assert.NotNull(key);

			var accelerator = KeyboardAccelerator.FromString(shortcut.ToString());
			Assert.Single(accelerator.Key);
			Assert.Single(accelerator.Modifiers);
			Assert.Equal(accelerator.Key, shortcut.Key);
			Assert.Equal(accelerator.Modifiers.ElementAt(0), shortcut.Modifier);
		}

		[Fact]
		public void KeyboardAcceleratorFromLetterAnd2Modifier()
		{
			string modifier = "ctrl";
			string modifier1Alt = "alt";
			string key = "A";
			string shortCutKeyBinding = $"{modifier}+{modifier1Alt}+{key}";
			var accelerator = KeyboardAccelerator.FromString(shortCutKeyBinding);

			Assert.Single(accelerator.Key);
			Assert.Equal(2, accelerator.Modifiers.Count());
			Assert.Equal(accelerator.Key, key);
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
