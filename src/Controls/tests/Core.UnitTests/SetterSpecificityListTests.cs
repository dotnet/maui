using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class SetterSpecificityListTests
	{
		[Fact]
		public void OneValue()
		{
			var list = new SetterSpecificityList();
			list.SetValue(SetterSpecificity.ManualValueSetter, nameof(SetterSpecificity.ManualValueSetter));

			var pair = list.GetSpecificityAndValue();
			Assert.Equal(nameof(SetterSpecificity.ManualValueSetter), pair.Value);
			Assert.Equal(SetterSpecificity.ManualValueSetter, pair.Key);

			// Add a "default" value
			list.SetValue(SetterSpecificity.DefaultValue, nameof(SetterSpecificity.DefaultValue));
			pair = list.GetSpecificityAndValue();
			Assert.Equal(nameof(SetterSpecificity.ManualValueSetter), pair.Value);
			Assert.Equal(SetterSpecificity.ManualValueSetter, pair.Key);
		}

		[Fact]
		public void TwoValues()
		{
			var list = new SetterSpecificityList();
			list.SetValue(SetterSpecificity.DefaultValue, nameof(SetterSpecificity.DefaultValue));
			list.SetValue(SetterSpecificity.ManualValueSetter, nameof(SetterSpecificity.ManualValueSetter));

			var pair = list.GetSpecificityAndValue();
			Assert.Equal(nameof(SetterSpecificity.ManualValueSetter), pair.Value);
			Assert.Equal(SetterSpecificity.ManualValueSetter, pair.Key);

			// Remove a value
			list.Remove(SetterSpecificity.ManualValueSetter);
			pair = list.GetSpecificityAndValue();
			Assert.Equal(nameof(SetterSpecificity.DefaultValue), pair.Value);
			Assert.Equal(SetterSpecificity.DefaultValue, pair.Key);
		}

		[Fact]
		public void ThreeValues()
		{
			var list = new SetterSpecificityList();
			list.SetValue(SetterSpecificity.DefaultValue, nameof(SetterSpecificity.DefaultValue));
			list.SetValue(SetterSpecificity.FromBinding, nameof(SetterSpecificity.FromBinding));
			list.SetValue(SetterSpecificity.ManualValueSetter, nameof(SetterSpecificity.ManualValueSetter));

			var pair = list.GetSpecificityAndValue();
			Assert.Equal(nameof(SetterSpecificity.ManualValueSetter), pair.Value);
			Assert.Equal(SetterSpecificity.ManualValueSetter, pair.Key);

			// Remove a value
			list.Remove(SetterSpecificity.ManualValueSetter);
			pair = list.GetSpecificityAndValue();
			Assert.Equal(nameof(SetterSpecificity.FromBinding), pair.Value);
			Assert.Equal(SetterSpecificity.FromBinding, pair.Key);
		}

		[Fact]
		public void GetClearedValue()
		{
			var list = new SetterSpecificityList();
			list.SetValue(SetterSpecificity.DefaultValue, nameof(SetterSpecificity.DefaultValue));
			Assert.Equal(nameof(SetterSpecificity.DefaultValue), list.GetClearedValue(SetterSpecificity.DefaultValue));
			list.SetValue(SetterSpecificity.ManualValueSetter, nameof(SetterSpecificity.ManualValueSetter));
			Assert.Equal(nameof(SetterSpecificity.DefaultValue), list.GetClearedValue(SetterSpecificity.ManualValueSetter));
		}
	}
}
