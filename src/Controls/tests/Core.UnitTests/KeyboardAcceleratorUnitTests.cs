using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Controls.Internals;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class KeyboardAcceleratorUnitTests : BaseTestFixture
	{
		[Theory]
		[InlineData("A")]
		[InlineData("B")]
		[InlineData("1")]
		[InlineData("2")]
		public void KeyboardAcceleratorFromKeyOnly(string key)
		{
			var accelerator = new KeyboardAccelerator() { Key = key };

			Assert.Single(accelerator.Key);
			Assert.Equal(accelerator.Key, key);
		}

		[Theory]
		[InlineData(KeyboardAcceleratorModifiers.None, "A")]
		[InlineData(KeyboardAcceleratorModifiers.Shift, "A")]
		[InlineData(KeyboardAcceleratorModifiers.Ctrl, "A")]
		[InlineData(KeyboardAcceleratorModifiers.Alt, "A")]
		[InlineData(KeyboardAcceleratorModifiers.Cmd, "A")]
		[InlineData(KeyboardAcceleratorModifiers.Windows, "A")]
		[InlineData(KeyboardAcceleratorModifiers.Shift | KeyboardAcceleratorModifiers.Ctrl, "A")]
		[InlineData(KeyboardAcceleratorModifiers.Shift | KeyboardAcceleratorModifiers.Alt, "A")]
		[InlineData(KeyboardAcceleratorModifiers.Shift | KeyboardAcceleratorModifiers.Cmd, "A")]
		[InlineData(KeyboardAcceleratorModifiers.Shift | KeyboardAcceleratorModifiers.Windows, "A")]
		[InlineData(KeyboardAcceleratorModifiers.Ctrl | KeyboardAcceleratorModifiers.Alt, "A")]
		[InlineData(KeyboardAcceleratorModifiers.Ctrl | KeyboardAcceleratorModifiers.Cmd, "A")]
		[InlineData(KeyboardAcceleratorModifiers.Ctrl | KeyboardAcceleratorModifiers.Windows, "A")]
		[InlineData(KeyboardAcceleratorModifiers.Alt | KeyboardAcceleratorModifiers.Cmd, "A")]
		[InlineData(KeyboardAcceleratorModifiers.Alt | KeyboardAcceleratorModifiers.Windows, "A")]
		[InlineData(KeyboardAcceleratorModifiers.Shift | KeyboardAcceleratorModifiers.Ctrl | KeyboardAcceleratorModifiers.Alt, "A")]
		[InlineData(KeyboardAcceleratorModifiers.Shift | KeyboardAcceleratorModifiers.Ctrl | KeyboardAcceleratorModifiers.Cmd, "A")]
		[InlineData(KeyboardAcceleratorModifiers.Shift | KeyboardAcceleratorModifiers.Ctrl | KeyboardAcceleratorModifiers.Windows, "A")]
		[InlineData(KeyboardAcceleratorModifiers.Ctrl | KeyboardAcceleratorModifiers.Alt | KeyboardAcceleratorModifiers.Cmd, "A")]
		[InlineData(KeyboardAcceleratorModifiers.Ctrl | KeyboardAcceleratorModifiers.Alt | KeyboardAcceleratorModifiers.Windows, "A")]
		[InlineData(KeyboardAcceleratorModifiers.Shift | KeyboardAcceleratorModifiers.Alt | KeyboardAcceleratorModifiers.Cmd, "A")]
		[InlineData(KeyboardAcceleratorModifiers.Shift | KeyboardAcceleratorModifiers.Alt | KeyboardAcceleratorModifiers.Windows, "A")]
		[InlineData(KeyboardAcceleratorModifiers.Shift | KeyboardAcceleratorModifiers.Ctrl
			| KeyboardAcceleratorModifiers.Alt | KeyboardAcceleratorModifiers.Cmd, "A")]
		[InlineData(KeyboardAcceleratorModifiers.Shift | KeyboardAcceleratorModifiers.Ctrl
			| KeyboardAcceleratorModifiers.Alt | KeyboardAcceleratorModifiers.Windows, "A")]
		public void KeyboardAcceleratorFromKeyAndModifiers(KeyboardAcceleratorModifiers modifiers, string key)
		{
			var accelerator = new KeyboardAccelerator() { Modifiers = modifiers, Key = key };

			Assert.Single(accelerator.Key);
			Assert.Equal(accelerator.Key, key);
			Assert.Equal(accelerator.Modifiers, modifiers);
		}
	}
}
