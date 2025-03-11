#nullable disable
using Microsoft.Maui.Controls.Handlers.Items;

namespace Microsoft.Maui.Controls
{
	public partial class CollectionView
	{
		internal static new void RemapForControls()
		{
#if WINDOWS
			CollectionViewHandler.Mapper.ReplaceMapping<CollectionView, CollectionViewHandler>(PlatformConfiguration.WindowsSpecific.CollectionView.SingleSelectionFollowsFocusProperty.PropertyName, MapSingleSelectionFollowsFocus);
#endif
		}
	}
}
