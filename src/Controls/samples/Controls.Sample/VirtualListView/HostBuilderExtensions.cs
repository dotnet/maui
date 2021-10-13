using Microsoft.Extensions.DependencyInjection;
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
        public static MauiAppBuilder UseVirtualListView(this MauiAppBuilder builder)
            => builder.ConfigureMauiHandlers(handlers =>
                handlers.AddHandler(typeof(IVirtualListView), typeof(VirtualListViewHandler)));
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
