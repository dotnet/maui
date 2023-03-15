using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.StyleSheets;
using CFlexLayout = Microsoft.Maui.Controls.Compatibility.FlexLayout;
using CGrid = Microsoft.Maui.Controls.Compatibility.Grid;
using CStackLayout = Microsoft.Maui.Controls.Compatibility.StackLayout;

[assembly: InternalsVisibleTo("Compatibility.Windows.UnitTests")]
[assembly: InternalsVisibleTo("Compatibility.Android.UnitTests")]
[assembly: InternalsVisibleTo("Microsoft.Maui.Controls.Core.UnitTests")]
[assembly: InternalsVisibleTo("Microsoft.Maui.Compatibility.Core.UnitTests")]
[assembly: InternalsVisibleTo("Microsoft.Maui.Controls.Xaml.UnitTests")]
[assembly: StyleProperty("column-gap", typeof(CGrid), nameof(CGrid.ColumnSpacingProperty))]
[assembly: StyleProperty("row-gap", typeof(CGrid), nameof(CGrid.RowSpacingProperty))]

// compatibility flex
[assembly: StyleProperty("align-content", typeof(CFlexLayout), nameof(CFlexLayout.AlignContentProperty))]
[assembly: StyleProperty("align-items", typeof(CFlexLayout), nameof(CFlexLayout.AlignItemsProperty))]
[assembly: StyleProperty("align-self", typeof(VisualElement), nameof(CFlexLayout.AlignSelfProperty), PropertyOwnerType = typeof(CFlexLayout))]
[assembly: StyleProperty("flex-direction", typeof(CFlexLayout), nameof(CFlexLayout.DirectionProperty))]
[assembly: StyleProperty("flex-basis", typeof(VisualElement), nameof(CFlexLayout.BasisProperty), PropertyOwnerType = typeof(CFlexLayout))]
[assembly: StyleProperty("flex-grow", typeof(VisualElement), nameof(CFlexLayout.GrowProperty), PropertyOwnerType = typeof(CFlexLayout))]
[assembly: StyleProperty("flex-shrink", typeof(VisualElement), nameof(CFlexLayout.ShrinkProperty), PropertyOwnerType = typeof(CFlexLayout))]
[assembly: StyleProperty("flex-wrap", typeof(VisualElement), nameof(CFlexLayout.WrapProperty), PropertyOwnerType = typeof(CFlexLayout))]
[assembly: StyleProperty("justify-content", typeof(CFlexLayout), nameof(CFlexLayout.JustifyContentProperty))]
[assembly: StyleProperty("order", typeof(VisualElement), nameof(CFlexLayout.OrderProperty), PropertyOwnerType = typeof(CFlexLayout))]
[assembly: StyleProperty("position", typeof(CFlexLayout), nameof(CFlexLayout.PositionProperty))]

//xf specific
[assembly: StyleProperty("-maui-spacing", typeof(CStackLayout), nameof(CStackLayout.SpacingProperty))]
[assembly: StyleProperty("-maui-orientation", typeof(CStackLayout), nameof(CStackLayout.OrientationProperty))]