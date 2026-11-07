#if IOS || MACCATALYST
using Foundation;
using Microsoft.Maui;

namespace Maui.Controls.Sample.Platform;

[Register("SceneDelegate")]
public class SceneDelegate : MauiUISceneDelegate
{
}
#endif
