using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Devices;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class WarnOnObsolete : ContentPage
	{
		public WarnOnObsolete() => InitializeComponent();
		public WarnOnObsolete(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		class Test
		{
			[SetUp] public void Setup() => AppInfo.SetCurrent(new MockAppInfo());
			[TearDown] public void TearDown() => AppInfo.SetCurrent(null);
		}
	}

	public class ViewWithObsolete : View
	{
		[Obsolete("Can't touch this")]
		public static BindableProperty ObsoleteBPProperty = BindableProperty.Create("ObsoleteBP", typeof(string), typeof(ViewWithObsolete), null);

		public string ObsoleteBP { get; set; }

		[Obsolete("Can't touch this")]
		public string ObsoleteProp { get; set; }

		public string ObsoletePropSetter { get; [Obsolete("Can't touch this")] set; }
	}
}
