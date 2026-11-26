using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class PlatformBehaviorTests : BaseTestFixture
	{
		[Fact]
		// https://github.com/dotnet/maui/issues/39747
		public void AttachesOnceTheHandlerIsSetWhenLoadedFiresFirst()
		{
			var page = new ContentPage();
			var behavior = new TestPlatformBehavior();
			page.Behaviors.Add(behavior);

			// Without a platform, Loaded fires as soon as the page gets a Window, before it has a Handler
			_ = new Window(page);

			Assert.Equal(0, behavior.AttachedCount);

			page.Handler = new PageHandlerStub();

			Assert.Equal(1, behavior.AttachedCount);

			page.Handler = null;

			Assert.Equal(1, behavior.DetachedCount);
		}

		[Fact]
		public void AttachesOnceWhenLoadedFiresAfterTheHandlerIsSet()
		{
			var page = new ContentPage();
			var behavior = new TestPlatformBehavior();
			page.Behaviors.Add(behavior);
			page.Handler = new PageHandlerStub();

			Assert.Equal(0, behavior.AttachedCount);

			_ = new Window(page);

			Assert.Equal(1, behavior.AttachedCount);
		}

		[Fact]
		public void AttachesWhenAddedToALoadedElement()
		{
			var page = new ContentPage { Handler = new PageHandlerStub() };
			_ = new Window(page);
			var behavior = new TestPlatformBehavior();

			page.Behaviors.Add(behavior);

			Assert.Equal(1, behavior.AttachedCount);
		}

		class TestPlatformBehavior : PlatformBehavior<ContentPage>
		{
			public int AttachedCount { get; private set; }
			public int DetachedCount { get; private set; }

			protected override void OnAttachedTo(ContentPage bindable, object platformView) => AttachedCount++;

			protected override void OnDetachedFrom(ContentPage bindable, object platformView) => DetachedCount++;
		}

		class PageHandlerStub : ViewHandler<ContentPage, object>
		{
			public PageHandlerStub() : base(new PropertyMapper<IView>())
			{
			}

			protected override object CreatePlatformView() => new object();
		}
	}
}
