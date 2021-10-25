using Microsoft.UI.Xaml.Markup;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using WDataTemplate = Microsoft.UI.Xaml.DataTemplate;
using WDataTemplateSelector = Microsoft.UI.Xaml.Controls.DataTemplateSelector;

namespace Microsoft.Maui.Controls
{
	public partial class IrDataTemplateSelector : WDataTemplateSelector
	{
		internal readonly PositionalViewSelector PositionalViewSelector;
		public readonly IVirtualListView VirtualListView;

		internal record RecycledDataTemplate(string ReuseId, WDataTemplate DataTemplate);

		List<RecycledDataTemplate> recycledDataTemplates = new();

		public IrDataTemplateSelector(IVirtualListView virtualListView)
		{
			VirtualListView = virtualListView;
			PositionalViewSelector = new PositionalViewSelector(virtualListView);
		}

		WDataTemplate CreateDataTemplateInstance()
		{
			Assembly.Load("Microsoft.Maui.Controls");
			Assembly.Load("Maui.Controls.Sample");

			var dtstr =
				@"<DataTemplate
					xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
					xmlns:maui=""using:Microsoft.Maui.Controls"">
					<maui:IrItemContentControl>
					</maui:IrItemContentControl>
				 </DataTemplate>";

			return XamlReader.Load(dtstr) as WDataTemplate;
		}

		protected override WDataTemplate SelectTemplateCore(object item)
		{
			if (item is IrDataWrapper dataWrapper)
			{
				var info = dataWrapper.position;
				if (info == null)
					return null;

				var data = PositionalViewSelector.Adapter.DataFor(info.Kind, info.SectionIndex, info.ItemIndex);
				var reuseId = PositionalViewSelector?.ViewSelector?.GetReuseId(info, data);

				RecycledDataTemplate container;

				foreach (var rt in recycledDataTemplates)
				{
					if (rt.ReuseId == reuseId)
					{
						container = rt;
						break;
					}
				}

				container = recycledDataTemplates?.FirstOrDefault(re => re.ReuseId == reuseId);

				if (container == null)
				{
					container = new RecycledDataTemplate(reuseId, CreateDataTemplateInstance());
					recycledDataTemplates.Add(container);
				}

				return container.DataTemplate;
			}

			return null;
		}
	}
}
