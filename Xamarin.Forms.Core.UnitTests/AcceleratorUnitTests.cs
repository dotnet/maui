using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
	public class AcceleratorUnitTests : BaseTestFixture
	{

		[Test]
		public void AcceleratorThrowsOnEmptyString()
		{
			Assert.Throws<ArgumentNullException>(() => Accelerator.FromString(""));
		}

		[Test]
		public void AcceleratorThrowsOnNull()
		{
			Assert.Throws<ArgumentNullException>(() => Accelerator.FromString(null));
		}

		[Test]
		public void AcceleratorFromString()
		{
			string shourtCutKeyBinding = "ctrl+A";
			var accelerator = Accelerator.FromString(shourtCutKeyBinding);

			Assert.AreEqual(shourtCutKeyBinding, accelerator.ToString());
		}

		[Test]
		public void AcceleratorFromOnlyLetter()
		{
			string shourtCutKeyBinding = "A";
			var accelerator = Accelerator.FromString(shourtCutKeyBinding);

			Assert.AreEqual(accelerator.Keys.Count(), 1);
			Assert.AreEqual(accelerator.Keys.ElementAt(0), shourtCutKeyBinding);
		}

		[Test, TestCaseSource(nameof(GenerateTests))]
		public void AcceleratorFromLetterAndModifier(TestShortcut shourtcut)
		{
			string modifier = shourtcut.Modifier;
			string key = shourtcut.Key;
			var accelerator = Accelerator.FromString(shourtcut.ToString());

			Assert.AreEqual(accelerator.Keys.Count(), 1);
			Assert.AreEqual(accelerator.Modifiers.Count(), 1);
			Assert.AreEqual(accelerator.Keys.ElementAt(0), shourtcut.Key);
			Assert.AreEqual(accelerator.Modifiers.ElementAt(0), shourtcut.Modifier);
		}


		[Test]
		public void AcceleratorFromLetterAnd2Modifier()
		{
			string modifier = "ctrl";
			string modifier1Alt = "alt";
			string key = "A";
			string shourtCutKeyBinding = $"{modifier}+{modifier1Alt}+{key}";
			var accelerator = Accelerator.FromString(shourtCutKeyBinding);

			Assert.AreEqual(accelerator.Keys.Count(), 1);
			Assert.AreEqual(accelerator.Modifiers.Count(), 2);
			Assert.AreEqual(accelerator.Keys.ElementAt(0), key);
			Assert.AreEqual(accelerator.Modifiers.ElementAt(0), modifier);
			Assert.AreEqual(accelerator.Modifiers.ElementAt(1), modifier1Alt);
		}


		[Preserve(AllMembers = true)]
		public struct TestShortcut
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
	}
}