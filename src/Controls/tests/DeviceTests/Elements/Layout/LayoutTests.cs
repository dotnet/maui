using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Layout)]
	public partial class LayoutTests : HandlerTestBase
	{
		[Theory]
		[InlineData(true, true, true)]
		[InlineData(true, false, false)]
		[InlineData(false, true, false)]
		[InlineData(false, false, false)]
		public async Task CascadeInputTransparentAppliesOnAdd(bool inputTransparent, bool cascadeInputTransparent, bool expected)
		{
			var control = new StackLayout() { InputTransparent = inputTransparent, CascadeInputTransparent = cascadeInputTransparent };
			_ = await CreateHandlerAsync<LayoutHandler>(control);

			var child = new Button() { InputTransparent = false };
			_ = await CreateHandlerAsync<ButtonHandler>(child);

			await InvokeOnMainThreadAsync(() => control.Add(child));

			Assert.Equal(expected, child.InputTransparent);
		}

		[Theory]
		[InlineData(true, true, true)]
		[InlineData(true, false, false)]
		[InlineData(false, true, false)]
		[InlineData(false, false, false)]
		public async Task CascadeInputTransparentAppliesOnInsert(bool inputTransparent, bool cascadeInputTransparent, bool expected)
		{
			var control = new StackLayout() 
			{ 
				InputTransparent = inputTransparent, 
				CascadeInputTransparent = cascadeInputTransparent 
			};

			_ = await CreateHandlerAsync<LayoutHandler>(control);

			var child = new Button() { InputTransparent = false };
			_ = await CreateHandlerAsync<ButtonHandler>(child);

			await InvokeOnMainThreadAsync(() => control.Insert(0, child));

			Assert.Equal(expected, child.InputTransparent);
		}

		[Theory]
		[InlineData(true, true, true)]
		[InlineData(true, false, false)]
		[InlineData(false, true, false)]
		[InlineData(false, false, false)]
		public async Task CascadeInputTransparentAppliesOnUpdate(bool inputTransparent, bool cascadeInputTransparent, bool expected)
		{
			var control = new StackLayout() { InputTransparent = inputTransparent, CascadeInputTransparent = cascadeInputTransparent };
			_ = await CreateHandlerAsync<LayoutHandler>(control);

			var child0 = new Button() { InputTransparent = false };
			_ = await CreateHandlerAsync<ButtonHandler>(child0);

			await InvokeOnMainThreadAsync(() => control.Add(child0));

			var child1 = new Button() { InputTransparent = false };
			_ = await CreateHandlerAsync<ButtonHandler>(child1);

			await InvokeOnMainThreadAsync(() => control[0] = child1);

			Assert.Equal(expected, child1.InputTransparent);
		}

		[Theory]
		[InlineData(true, true, true)]
		[InlineData(true, false, false)]
		[InlineData(false, true, false)]
		[InlineData(false, false, false)]
		public async Task CascadeInputTransparentAppliesOnInit(bool inputTransparent, bool cascadeInputTransparent, bool expected)
		{
			var child = new Button() { InputTransparent = false };
			_ = await CreateHandlerAsync<ButtonHandler>(child);

			var control = new StackLayout() { InputTransparent = inputTransparent, CascadeInputTransparent = cascadeInputTransparent };
			await InvokeOnMainThreadAsync(() => control.Add(child));
			_ = await CreateHandlerAsync<LayoutHandler>(control);

			Assert.Equal(expected, child.InputTransparent);
		}
	}
}
