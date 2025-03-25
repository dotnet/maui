#pragma warning disable CS0618 // Type or member is obsolete
#if WINDOWS || ANDROID
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.DeviceTests.Stubs;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Frame)]
	public partial class FrameHandlerTest : HandlerTestBase<FrameHandlerTest.FrameRendererWithEmptyCtor, FrameStub>
	{
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
#pragma warning restore CS0618 // Type or member is obsolete