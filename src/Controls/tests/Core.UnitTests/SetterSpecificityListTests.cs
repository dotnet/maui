using System;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class SetterSpecificityListTests
	{
		[Fact]
		public void NoValues()
		{
			var list = new SetterSpecificityList<object>();

			var pair = list.GetSpecificityAndValue();
			Assert.Null(pair.Value);
			Assert.Equal(default, pair.Key);
		}

		[Fact]
		public void OverridesValueWithSameSpecificity()
		{
			var list = new SetterSpecificityList<object>();
			list[SetterSpecificity.ManualValueSetter] = "initial";

			list[SetterSpecificity.ManualValueSetter] = "new";
			Assert.Equal(1, list.Count);

			var pair = list.GetSpecificityAndValue();
			Assert.Equal("new", pair.Value);
			Assert.Equal(SetterSpecificity.ManualValueSetter, pair.Key);
		}

		[Fact]
		public async Task RemovingValueDoesNotLeak()
		{
			var list = new SetterSpecificityList<object>();
			list[SetterSpecificity.DefaultValue] = nameof(SetterSpecificity.DefaultValue);
			list[SetterSpecificity.FromHandler] = nameof(SetterSpecificity.FromHandler);
			WeakReference<object> weakReference;

			{
				var o = new object();
				weakReference = new WeakReference<object>(o);
				list[SetterSpecificity.FromBinding] = o;
			}

			list.Remove(SetterSpecificity.FromBinding);

			await Task.Yield();
			GC.Collect();
			GC.WaitForPendingFinalizers();

			Assert.False(weakReference.TryGetTarget(out _));
		}

		[Fact]
		public async Task RemovingLastValueDoesNotLeak()
		{
			var list = new SetterSpecificityList<object>();
			WeakReference<object> weakReference;

			{
				var o = new object();
				weakReference = new WeakReference<object>(o);
				list[SetterSpecificity.ManualValueSetter] = o;
			}

			list.Remove(SetterSpecificity.ManualValueSetter);

			await Task.Yield();
			GC.Collect();
			GC.WaitForPendingFinalizers();

			Assert.False(weakReference.TryGetTarget(out _));
		}

		[Fact]
		public void GetValueForSpecificity()
		{
			var list = new SetterSpecificityList<object>();
			list[SetterSpecificity.DefaultValue] = nameof(SetterSpecificity.DefaultValue);
			list[SetterSpecificity.ManualValueSetter] = nameof(SetterSpecificity.ManualValueSetter);

			var foundValue = list[SetterSpecificity.DefaultValue];
			Assert.Equal(nameof(SetterSpecificity.DefaultValue), foundValue);
		}

		[Fact]
		public void NullWhenNoValuesMatchSpecificity()
		{
			var list = new SetterSpecificityList<object>();
			list[SetterSpecificity.DefaultValue] = nameof(SetterSpecificity.DefaultValue);
			list[SetterSpecificity.ManualValueSetter] = nameof(SetterSpecificity.ManualValueSetter);

			var foundValue = list[SetterSpecificity.FromHandler];
			Assert.Null(foundValue);
		}

		[Fact]
		public void OneValue()
		{
			var list = new SetterSpecificityList<object>();
			list[SetterSpecificity.ManualValueSetter] = nameof(SetterSpecificity.ManualValueSetter);

			var pair = list.GetSpecificityAndValue();
			Assert.Equal(nameof(SetterSpecificity.ManualValueSetter), pair.Value);
			Assert.Equal(SetterSpecificity.ManualValueSetter, pair.Key);

			// Add a "default" value
			list[SetterSpecificity.DefaultValue] = nameof(SetterSpecificity.DefaultValue);
			pair = list.GetSpecificityAndValue();
			Assert.Equal(nameof(SetterSpecificity.ManualValueSetter), pair.Value);
			Assert.Equal(SetterSpecificity.ManualValueSetter, pair.Key);
			Assert.Equal(nameof(SetterSpecificity.ManualValueSetter), list.GetValue());
			Assert.Equal(SetterSpecificity.ManualValueSetter, list.GetSpecificity());
		}

		[Fact]
		public void TwoValues()
		{
			var list = new SetterSpecificityList<object>();
			list[SetterSpecificity.DefaultValue] = nameof(SetterSpecificity.DefaultValue);
			list[SetterSpecificity.ManualValueSetter] = nameof(SetterSpecificity.ManualValueSetter);

			var pair = list.GetSpecificityAndValue();
			Assert.Equal(nameof(SetterSpecificity.ManualValueSetter), pair.Value);
			Assert.Equal(SetterSpecificity.ManualValueSetter, pair.Key);

			// Remove a value
			list.Remove(SetterSpecificity.ManualValueSetter);
			pair = list.GetSpecificityAndValue();
			Assert.Equal(nameof(SetterSpecificity.DefaultValue), pair.Value);
			Assert.Equal(SetterSpecificity.DefaultValue, pair.Key);
			Assert.Equal(nameof(SetterSpecificity.DefaultValue), list.GetValue());
			Assert.Equal(SetterSpecificity.DefaultValue, list.GetSpecificity());
		}

		[Fact]
		public void ThreeValues()
		{
			var list = new SetterSpecificityList<object>();
			list[SetterSpecificity.DefaultValue] = nameof(SetterSpecificity.DefaultValue);
			list[SetterSpecificity.FromBinding] = nameof(SetterSpecificity.FromBinding);
			list[SetterSpecificity.ManualValueSetter] = nameof(SetterSpecificity.ManualValueSetter);

			var pair = list.GetSpecificityAndValue();
			Assert.Equal(nameof(SetterSpecificity.ManualValueSetter), pair.Value);
			Assert.Equal(SetterSpecificity.ManualValueSetter, pair.Key);

			// Remove a value
			list.Remove(SetterSpecificity.ManualValueSetter);
			pair = list.GetSpecificityAndValue();
			Assert.Equal(nameof(SetterSpecificity.FromBinding), pair.Value);
			Assert.Equal(SetterSpecificity.FromBinding, pair.Key);
			Assert.Equal(nameof(SetterSpecificity.FromBinding), list.GetValue());
			Assert.Equal(SetterSpecificity.FromBinding, list.GetSpecificity());
		}

		[Fact]
		public void ManyValues()
		{
			var list = new SetterSpecificityList<object>();
			list[SetterSpecificity.DefaultValue] = nameof(SetterSpecificity.DefaultValue);
			list[SetterSpecificity.FromBinding] = nameof(SetterSpecificity.FromBinding);
			list[SetterSpecificity.DynamicResourceSetter] = nameof(SetterSpecificity.DynamicResourceSetter);
			list[SetterSpecificity.ManualValueSetter] = nameof(SetterSpecificity.ManualValueSetter);
			list[SetterSpecificity.Trigger] = nameof(SetterSpecificity.Trigger);

			var pair = list.GetSpecificityAndValue();
			Assert.Equal(nameof(SetterSpecificity.Trigger), pair.Value);
			Assert.Equal(SetterSpecificity.Trigger, pair.Key);

			// Remove a value
			list.Remove(SetterSpecificity.ManualValueSetter);
			pair = list.GetSpecificityAndValue();
			Assert.Equal(nameof(SetterSpecificity.Trigger), pair.Value);
			Assert.Equal(SetterSpecificity.Trigger, pair.Key);
			Assert.Equal(nameof(SetterSpecificity.Trigger), list.GetValue());
			Assert.Equal(SetterSpecificity.Trigger, list.GetSpecificity());
		}

		[Fact]
		public void GetClearedValue()
		{
			var list = new SetterSpecificityList<object>();
			list[SetterSpecificity.DefaultValue] = nameof(SetterSpecificity.DefaultValue);
			Assert.Equal(default, list.GetClearedValue());
			Assert.Equal(default, list.GetClearedSpecificity());
			list[SetterSpecificity.ManualValueSetter] = nameof(SetterSpecificity.ManualValueSetter);
			Assert.Equal(nameof(SetterSpecificity.DefaultValue), list.GetClearedValue());
			Assert.Equal(SetterSpecificity.DefaultValue, list.GetClearedSpecificity());
		}

		[Fact]
		public void GetClearedValueForSpecificity()
		{
			var list = new SetterSpecificityList<object>();
			list[SetterSpecificity.DefaultValue] = nameof(SetterSpecificity.DefaultValue);
			Assert.Equal(default, list.GetClearedValue(SetterSpecificity.DefaultValue));
			list[SetterSpecificity.ManualValueSetter] = nameof(SetterSpecificity.ManualValueSetter);
			Assert.Equal(nameof(SetterSpecificity.DefaultValue), list.GetClearedValue(SetterSpecificity.ManualValueSetter));
			Assert.Equal(nameof(SetterSpecificity.ManualValueSetter), list.GetClearedValue(SetterSpecificity.FromHandler));
		}
	}
}
