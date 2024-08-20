
namespace Microsoft.Maui.Controls.PlatformConfiguration
{
	/// <summary>
	/// Marker class that identifies the Android platform.
	/// </summary>
	/// <remarks>
	/// Developers specifiy the type name of this marker class to the <see cref="M:Microsoft.Maui.Controls.IElementConfiguration`1.On``1" /> method to specify the underlying Android control on which to run a platform-specific effect.
	/// </remarks>
	public sealed class Android : IConfigPlatform { }
	/// <include file="../../../docs/Microsoft.Maui.Controls.PlatformConfiguration/iOS.xml" path="Type[@FullName='Microsoft.Maui.Controls.PlatformConfiguration.iOS']/Docs/*" />
	public sealed class iOS : IConfigPlatform { }
	/// <include file="../../../docs/Microsoft.Maui.Controls.PlatformConfiguration/Windows.xml" path="Type[@FullName='Microsoft.Maui.Controls.PlatformConfiguration.Windows']/Docs/*" />
	public sealed class Windows : IConfigPlatform { }
	/// <include file="../../../docs/Microsoft.Maui.Controls.PlatformConfiguration/Tizen.xml" path="Type[@FullName='Microsoft.Maui.Controls.PlatformConfiguration.Tizen']/Docs/*" />
	public sealed class Tizen : IConfigPlatform { }
	/// <include file="../../../docs/Microsoft.Maui.Controls.PlatformConfiguration/macOS.xml" path="Type[@FullName='Microsoft.Maui.Controls.PlatformConfiguration.macOS']/Docs/*" />
	public sealed class macOS : IConfigPlatform { }
	/// <include file="../../../docs/Microsoft.Maui.Controls.PlatformConfiguration/GTK.xml" path="Type[@FullName='Microsoft.Maui.Controls.PlatformConfiguration.GTK']/Docs/*" />
	public sealed class GTK : IConfigPlatform { }
}
