using System;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../../docs/Microsoft.Maui.Controls/CollectionView.xml" path="Type[@FullName='Microsoft.Maui.Controls.CollectionView']/Docs/*" />
	public partial class CollectionView : ReorderableItemsView, IElementConfiguration<CollectionView>
	{
		readonly Lazy<PlatformConfigurationRegistry<CollectionView>> _platformConfigurationRegistry;

		public CollectionView()
		{
			_platformConfigurationRegistry = new Lazy<PlatformConfigurationRegistry<CollectionView>>(() => new PlatformConfigurationRegistry<CollectionView>(this));
		}

		public IPlatformElementConfiguration<T, CollectionView> On<T>() where T : IConfigPlatform
		{
			return _platformConfigurationRegistry.Value.On<T>();
		}
	}
}
