using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Internals;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Windows.Foundation;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
{
	[System.Obsolete(Compatibility.Hosting.MauiAppBuilderExtensions.UseMapperInstead)]
	public partial class CollectionViewRenderer : GroupableItemsViewRenderer<GroupableItemsView>
	{
	}
}