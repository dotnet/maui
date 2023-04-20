using System.ComponentModel;
using Microsoft.Maui.Controls;

namespace Microsoft.Maui.Controls.Xaml.UnitTests.Issues.Maui14158;

[Description("Microsoft.Maui.Controls.Xaml.UnitTests.InternalsHiddenAssembly")]
internal class PublicInExternal : Button { }

[Description("Microsoft.Maui.Controls.Xaml.UnitTests.InternalsHiddenAssembly")]
public class PublicInHidden : Button { }

[Description("Microsoft.Maui.Controls.Xaml.UnitTests.InternalsHiddenAssembly")]
internal class PublicInVisible : Button { }
