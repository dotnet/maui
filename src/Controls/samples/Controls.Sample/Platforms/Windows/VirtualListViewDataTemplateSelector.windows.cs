using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Maui
{
	public partial class VirtualListViewDataTemplate : DataTemplate
	{
		public IView View { get; private set; }
		public PositionInfo Position { get; private set; }

		public void Update(PositionInfo position, IView view)
		{
			View = view;
			Position = position;
		}
	}

	public partial class VirtualListViewDataTemplateSelector : DataTemplateSelector
	{
		readonly object lockObj = new object();

		internal readonly PositionalViewSelector PositionalViewSelector;
		public readonly IVirtualListView VirtualListView;

		public readonly VirtualListViewDataTemplate ContainerTemplate;


		internal record RecycledDataTemplate(string ReuseId, VirtualListViewDataTemplate DataTemplate);

		List<RecycledDataTemplate> recycledDataTemplates = new();

		public VirtualListViewDataTemplateSelector(IVirtualListView virtualListView, VirtualListViewDataTemplate dataTemplate)
		{
			VirtualListView = virtualListView;
			ContainerTemplate = dataTemplate;

			PositionalViewSelector = new PositionalViewSelector(virtualListView);
		}

		protected override DataTemplate SelectTemplateCore(object item)
		{
			if (item is IrDataWrapper dataWrapper)
			{
				var info = dataWrapper.position;
				if (info == null)
					return null;

				var data = PositionalViewSelector.Adapter.DataFor(info.Kind, info.SectionIndex, info.ItemIndex);

				var reuseId = PositionalViewSelector?.ViewSelector?.GetReuseId(info.Kind, data, info.SectionIndex, info.ItemIndex);

				RecycledDataTemplate container;

				lock (lockObj)
				{
					container = recycledDataTemplates?.FirstOrDefault(re => re.ReuseId == reuseId);

					if (container == null)
					{
						container = new RecycledDataTemplate(reuseId, ContainerTemplate);
						recycledDataTemplates.Add(container);
					}
				}

				var view = PositionalViewSelector?.ViewSelector?.CreateView(info.Kind, data, info.SectionIndex, info.ItemIndex);

				if (view is IPositionInfo viewWithPositionInfo)
					viewWithPositionInfo.SetPositionInfo(info);

				
				container.DataTemplate.Update(info, view);

				return container.DataTemplate;
			}

			return null;
		}
	}
}
