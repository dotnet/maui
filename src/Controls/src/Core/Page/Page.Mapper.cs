using System;
namespace Microsoft.Maui.Controls;

public partial class Page
{
	internal static new void RemapForControls()
	{
#if IOS
		PageHandler.Mapper.ReplaceMapping<IContentView, IPageHandler>(nameof(ISafeAreaView2.SafeAreaInsets), MapUpdateSafeAreaInsets);
#endif
	}
}
