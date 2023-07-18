#if WINDOWS || ANDROID
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Hosting;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Frame)]
	public partial class FrameHandlerTest : HandlerTestBase<FrameHandlerTest.FrameRendererWithEmptyCtor, FrameStub>
	{
		protected override MauiAppBuilder ConfigureBuilder(MauiAppBuilder mauiAppBuilder) =>
			base.ConfigureBuilder(mauiAppBuilder)
				.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<Frame, FrameHandlerTest.FrameRendererWithEmptyCtor>();
					handlers.AddHandler<FrameStub, FrameHandlerTest.FrameRendererWithEmptyCtor>();
				});

		public FrameHandlerTest()
		{

		}

		public class FrameRendererWithEmptyCtor : FrameRenderer
		{
#if ANDROID

			public FrameRendererWithEmptyCtor() : base(MauiProgramDefaults.DefaultContext)
			{
			}
			public FrameRendererWithEmptyCtor(IPropertyMapper mapper)
				: base(MauiProgramDefaults.DefaultContext, mapper)
			{
			}

			public FrameRendererWithEmptyCtor(IPropertyMapper mapper, CommandMapper commandMapper)
				: base(MauiProgramDefaults.DefaultContext, mapper, commandMapper)
			{
			}

#else

			public FrameRendererWithEmptyCtor()
			{

			}

			public FrameRendererWithEmptyCtor(IPropertyMapper mapper)
				: base(mapper)
			{
			}

			public FrameRendererWithEmptyCtor(IPropertyMapper mapper, CommandMapper commandMapper)
				: base(mapper, commandMapper)
			{
			}
#endif
		}
	}
}
#endif