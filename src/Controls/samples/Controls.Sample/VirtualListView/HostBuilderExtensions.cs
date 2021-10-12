using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Maui
{
    public static class VirtualListViewHostBuilderExtensions
    {
		public static MauiAppBuilder UseVirtualListView(this MauiAppBuilder appHostBuilder)
		{
			return appHostBuilder.ConfigureMauiHandlers(handlers => {
				handlers.TryAddHandler(typeof(VirtualListView), typeof(VirtualListViewHandler));
				handlers.TryAddHandler(typeof(IVirtualListView), typeof(VirtualListViewHandler));
			});
		}
	}

    public static class ViewExtensions
    {
        public static int GetContentTypeHashCode(this IView view)
        {
            var hashCode = view.GetType().GetHashCode();

            if (view is IContainer container)
            {
                foreach (var c in container)
                {
                    hashCode = (hashCode, GetContentTypeHashCode(c)).GetHashCode();
                }
            }

            return hashCode;
        }
    }
}
