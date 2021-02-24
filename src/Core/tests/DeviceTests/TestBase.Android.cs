using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Maui.DeviceTests
{
	public partial class TestBase
	{
		public global::Android.Content.Context DefaultContext =>
			Platform.DefaultContext;
	}
}
