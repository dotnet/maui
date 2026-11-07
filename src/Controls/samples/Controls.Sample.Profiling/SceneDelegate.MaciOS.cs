#if IOS || MACCATALYST
using Foundation;
using Microsoft.Maui;

namespace Maui.Controls.Sample.Profiling;

[Register("SceneDelegate")]
public class SceneDelegate : MauiUISceneDelegate
{
}
#endif
