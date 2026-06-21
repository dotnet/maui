using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class ItemDisplayBindingTests
	{

		class TestViewModel
		{
			public string Name { get; set; }
			public string Description { get; set; }
		}

		class TestView : View
		{
			internal Picker picker = new Picker();
			public static readonly BindableProperty ItemDisplayBindingProperty = BindableProperty.Create(nameof(ItemDisplayBinding), typeof(BindingBase), typeof(TestView), propertyChanged: (bindable, _, newValue) =>
			{
				if (bindable is TestView view)
				{
					view.picker.ItemDisplayBinding = (BindingBase)newValue;
				}
			});


			public BindingBase ItemDisplayBinding
			{
				get => (BindingBase)GetValue(ItemDisplayBindingProperty);
				set => SetValue(ItemDisplayBindingProperty, value);
			}
		}

		[Fact]
		public void TestItemDisplayBinding()
		{
			var testView = new TestView();


			var binding = new Binding("Name");
			testView.ItemDisplayBinding = binding;

			Assert.Equal(binding, testView.ItemDisplayBinding);
			Assert.Equal(binding, testView.picker.ItemDisplayBinding);
		}


	}
}