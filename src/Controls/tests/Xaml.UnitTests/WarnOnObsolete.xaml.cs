using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class WarnOnObsolete : ContentPage
{
	public WarnOnObsolete() => InitializeComponent();


	class Test
	{
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
