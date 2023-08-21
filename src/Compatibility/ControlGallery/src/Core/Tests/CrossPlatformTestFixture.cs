//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Android.App;
//using Android.Content;
//using Android.Content.PM;
//using Android.OS;
//using Android.Renderscripts;
//using Android.Runtime;
//using Android.Views;
//using Android.Widget;
//using Microsoft.Maui.Controls.ControlGallery.Issues;

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
