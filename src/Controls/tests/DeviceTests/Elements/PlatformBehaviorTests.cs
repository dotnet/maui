using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Xunit;
#if IOS || MACCATALYST
using PlatformView = UIKit.UIView;
#elif MACOS
using PlatformView = AppKit.NSView;
#elif ANDROID
using PlatformView = Android.Views.View;
#elif WINDOWS
using PlatformView = Microsoft.UI.Xaml.FrameworkElement;
#elif NET6_0_OR_GREATER || (NETSTANDARD || !PLATFORM)
using PlatformView = System.Object;
#endif

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Behavior)]
#if ANDROID || IOS || MACCATALYST
	[Collection(ControlsHandlerTestBase.RunInNewWindowCollection)]
#endif
	public partial class PlatformBehaviorTests : ControlsHandlerTestBase
	{
		[Fact]
		public async Task BasicLoadUnloadingWorks()
		{
			var behavior = new PlatformBehaviorTest();
			var editor = new Editor();
			editor.Behaviors.Add(behavior);

			IView layout = new VerticalStackLayout()
			{
				Children =
				{
					editor
				}
			};

			Assert.False(behavior.IsAttached);
			Assert.Equal(0, behavior.AttachedCount);
			Assert.Equal(0, behavior.DetachedCount);

			await CreateHandlerAndAddToWindow<LayoutHandler>(layout, (_) =>
			{
				Assert.True(behavior.IsAttached);
				Assert.Equal(1, behavior.AttachedCount);
				Assert.Equal(0, behavior.DetachedCount);
				return Task.CompletedTask;
			});

			Assert.False(behavior.IsAttached);
			Assert.Equal(1, behavior.AttachedCount);
			Assert.Equal(1, behavior.DetachedCount);
		}


		class PlatformBehaviorTest : PlatformBehavior<Editor>
		{
			public int AttachedCount { get; private set; }
			public int DetachedCount { get; private set; }
			public bool IsAttached { get; private set; } = false;

			protected override void OnAttachedTo(Editor bindable, PlatformView platformView)
			{
				base.OnAttachedTo(bindable, platformView);
				IsAttached = true;
				AttachedCount++;
			}

			protected override void OnDetachedFrom(Editor bindable, PlatformView platformView)
			{
				base.OnDetachedFrom(bindable, platformView);
				IsAttached = false;
				DetachedCount++;
			}
		}

	}
}
