using Android.Content;
using Android.Views;
using AndroidX.Fragment.App;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Hosting;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public class MauiContextTests
	{
		IMauiContext GetContext()
		{
			return new ContextStub(MauiApp
				.CreateBuilder()
				.Build()
				.Services);
		}

		[Fact(DisplayName = "Correct Layout Inflater pulled from Activity")]
		public void ScopedMauiContextReturnsActivityInflator()
		{
			var context = GetContext().MakeScoped();
			Assert.Equal(context.GetLayoutInflater(), LayoutInflater.From(Platform.DefaultContext));
		}

		[Fact(DisplayName = "Correct Fragment Manager pulled from Activity")]
		public void ScopedMauiContextReturnsActivityFragmentManager()
		{
			var context = GetContext().MakeScoped();
			Assert.Equal(context.GetFragmentManager(), Platform.DefaultContext.GetFragmentManager());
		}

		[Fact(DisplayName = "Scoped Fragment Manager Returned")]
		public void ScopedMauiContextReturnsChildFragmentManager()
		{
			var manager = new TestFragmentManager();
			var context = GetContext().MakeScoped(fragmentManager: manager);
			Assert.NotEqual(context.GetFragmentManager(), Platform.DefaultContext.GetFragmentManager());
			Assert.Equal(context.GetFragmentManager(), manager);
		}

		[Fact(DisplayName = "Scoped Layout Manager Returned")]
		public void ScopedMauiContextReturnsChildLayoutInflater()
		{
			var layoutInflater = new TestLayoutInflater(Platform.DefaultContext);
			var context = GetContext().MakeScoped(layoutInflater: layoutInflater);
			Assert.NotEqual(context.GetLayoutInflater(), LayoutInflater.FromContext(Platform.DefaultContext));
			Assert.Equal(context.GetLayoutInflater(), layoutInflater);
		}

		[Fact(DisplayName = "Scoped Layout Returns Parent Animation Manager")]
		public void ScopedMauiContextReturnsParentAnimationManager()
		{
			var parentContext = GetContext();
			var context = parentContext.MakeScoped();
			Assert.Equal(parentContext.GetAnimationManager(), context.GetAnimationManager());
		}

		class TestLayoutInflater : LayoutInflater
		{
			public TestLayoutInflater(Context context) : base(context)
			{
			}

			public override LayoutInflater CloneInContext(Context newContext)
			{
				throw new System.NotImplementedException();
			}
		}

		class TestFragmentManager : FragmentManager
		{

		}
	}
}
