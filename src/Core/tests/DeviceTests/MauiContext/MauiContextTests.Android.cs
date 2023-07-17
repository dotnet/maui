using Android.Content;
using Android.Views;
using AndroidX.Fragment.App;
using Microsoft.Maui.Hosting;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.MauiContext)]
	public class MauiContextTests
	{
		readonly MauiApp _mauiApp;
		readonly MauiContext _mauiContext;

		public MauiContextTests()
		{
			_mauiApp = MauiApp
				.CreateBuilder()
				.Build();

			_mauiContext = new MauiContext(_mauiApp.Services, MauiProgram.DefaultContext);
		}

		[Fact(DisplayName = "Correct Layout Inflater pulled from Activity")]
		public void ScopedMauiContextReturnsActivityInflator()
		{
			var globalInflator = LayoutInflater.FromContext(MauiProgram.DefaultContext);

			var context = _mauiContext.MakeScoped();

			Assert.Equal(globalInflator, context.GetLayoutInflater());
		}

		[Fact(DisplayName = "Correct Fragment Manager pulled from Activity")]
		public void ScopedMauiContextReturnsActivityFragmentManager()
		{
			var globalManager = MauiProgram.DefaultContext.GetFragmentManager();

			var context = _mauiContext.MakeScoped();

			Assert.Equal(globalManager, context.GetFragmentManager());
		}

		[Fact(DisplayName = "Scoped Fragment Manager Returned")]
		public void ScopedMauiContextReturnsChildFragmentManager()
		{
			var manager = new TestFragmentManager();
			var globalManager = MauiProgram.DefaultContext.GetFragmentManager();

			var context = _mauiContext.MakeScoped(fragmentManager: manager);

			Assert.NotEqual(globalManager, context.GetFragmentManager());
			Assert.Equal(manager, context.GetFragmentManager());
		}

		[Fact(DisplayName = "Scoped Layout Manager Returned")]
		public void ScopedMauiContextReturnsChildLayoutInflater()
		{
			var globalInflator = LayoutInflater.FromContext(MauiProgram.DefaultContext);
			var layoutInflater = new TestLayoutInflater(MauiProgram.DefaultContext);

			var context = _mauiContext.MakeScoped(layoutInflater: layoutInflater);

			Assert.NotEqual(globalInflator, context.GetLayoutInflater());
			Assert.Equal(layoutInflater, context.GetLayoutInflater());
		}

		[Fact(DisplayName = "Scoped Layout Returns Parent Animation Manager")]
		public void ScopedMauiContextReturnsParentAnimationManager()
		{
			var parentContext = _mauiContext;
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
