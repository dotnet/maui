namespace Microsoft.Maui.Controls.ControlGallery.Tests
{
	public abstract class CrossPlatformTestFixture
	{
		ITestingPlatformService _testingPlatformService;
		protected ITestingPlatformService TestingPlatform
		{
			get
			{
				return _testingPlatformService = _testingPlatformService
					?? DependencyService.Resolve<ITestingPlatformService>();
			}
		}

	}
}
