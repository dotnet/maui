using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Hosting;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public abstract partial class FocusHandlerTests<THandler, TStub, TLayoutStub> : HandlerTestBasement<THandler, TStub>
		where THandler : class, IViewHandler, new()
		where TStub : IStubBase, new()
		where TLayoutStub : IStubBase, ILayout, new()
	{
		[Fact]
		public async Task FocusAndIsFocusedIsWorking()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handler =>
				{
					handler.AddHandler<TLayoutStub, LayoutHandler>();
					handler.AddHandler<TStub, THandler>();
				});
			});

			// create layout with 2 elements
			TStub inputControl1;
			TStub inputControl2;
			var layout = new TLayoutStub
			{
				(inputControl1 = new TStub { Width = 100, Height = 50 }),
				(inputControl2 = new TStub { Width = 100, Height = 50 })
			};
			layout.Width = 100;
			layout.Height = 150;

			await InvokeOnMainThreadAsync(async () =>
			{
				var contentViewHandler = CreateHandler<LayoutHandler>(layout);
				await contentViewHandler.PlatformView.AttachAndRun(async () =>
				{
					var platform1 = inputControl1.ToPlatform();
					var platform2 = inputControl2.ToPlatform();

					// focus the first control
					inputControl1.Handler.Invoke(nameof(IView.Focus), new FocusRequest(false));

					// assert
					await inputControl1.WaitForFocused();
					Assert.True(inputControl1.IsFocused);
					Assert.False(inputControl2.IsFocused);

					// focus the second control
					inputControl2.Handler.Invoke(nameof(IView.Focus), new FocusRequest(false));

					// assert
					await inputControl2.WaitForFocused();
					Assert.False(inputControl1.IsFocused);
					Assert.True(inputControl2.IsFocused);
				});
			});
		}

		[Fact]
		public async Task UnfocusAndIsFocusedIsWorking()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handler =>
				{
					handler.AddHandler<TLayoutStub, LayoutHandler>();
					handler.AddHandler<TStub, THandler>();
				});
			});

			// create layout with 2 elements
			TStub inputControl1;
			TStub inputControl2;
			var layout = new TLayoutStub
			{
				(inputControl1 = new TStub { Width = 100, Height = 50 }),
				(inputControl2 = new TStub { Width = 100, Height = 50 })
			};
			layout.Width = 100;
			layout.Height = 150;

			await InvokeOnMainThreadAsync(async () =>
			{
				var contentViewHandler = CreateHandler<LayoutHandler>(layout);
				await contentViewHandler.PlatformView.AttachAndRun(async () =>
				{
					var platform1 = inputControl1.ToPlatform();
					var platform2 = inputControl2.ToPlatform();

					// focus the first control
					inputControl1.Handler.Invoke(nameof(IView.Focus), new FocusRequest(false));

					// assert
					await inputControl1.WaitForFocused();
					Assert.True(inputControl1.IsFocused);
					Assert.False(inputControl2.IsFocused);

					// UNfocus the first control (revert the focus)
					inputControl1.Handler.Invoke(nameof(IView.Unfocus));

					// assert
					await inputControl1.WaitForUnFocused();
					Assert.False(inputControl1.IsFocused);
					Assert.False(inputControl2.IsFocused);
				});
			});
		}
	}
}