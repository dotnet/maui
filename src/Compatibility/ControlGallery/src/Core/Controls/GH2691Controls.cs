using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery
{
	public static class GH2691
	{
		public static void Init()
		{
		}
	}
}

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.CustomNamespace1
{
	public class CustomButton : Button
	{
	}
}

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.CustomNamespace2
{
	public class CustomLabel : Label
	{
	}
}

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.CustomNamespace3
{
	public class CustomStackLayout : StackLayout
	{
	}

	public class CustomLabel : Label
	{
	}
}
