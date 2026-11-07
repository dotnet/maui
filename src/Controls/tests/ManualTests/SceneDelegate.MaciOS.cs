#if IOS || MACCATALYST
using Foundation;
using Microsoft.Maui;

namespace Microsoft.Maui.ManualTests;

[Register("SceneDelegate")]
public class SceneDelegate : MauiUISceneDelegate
{
}
#endif
