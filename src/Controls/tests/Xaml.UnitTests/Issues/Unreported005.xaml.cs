using System;
using Xunit;
using Xunit.Sdk;

using Constraint = Microsoft.Maui.Controls.Compatibility.Constraint;
using RelativeLayout = Microsoft.Maui.Controls.Compatibility.RelativeLayout;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[RequireService([typeof(IReferenceProvider), typeof(IProvideValueTarget)])]
public abstract class Unreported005RelativeToView : IMarkupExtension
{
	protected Unreported005RelativeToView() => Factor = 1;

	public string ElementName { get; set; }

	public double Factor { get; set; }

	public double Constant { get; set; }

	public object ProvideValue(IServiceProvider serviceProvider)
	{
		if (new ReferenceExtension { Name = ElementName }.ProvideValue(serviceProvider) is View element)
		{
			var result = Constraint.RelativeToView(element, (layout, view) => DeterminePosition(view) + Constant);
			return result;
		}
		return null;
	}

	protected virtual double DeterminePosition(VisualElement view)
	{
		var result = DetermineStart(view) + DetermineExtent(view) * Factor;
		return result;
	}

	protected abstract double DetermineExtent(VisualElement view);

	protected abstract double DetermineStart(VisualElement view);
}

public class Unreported005RelativeToViewHorizontal : Unreported005RelativeToView
{
	protected override double DetermineExtent(VisualElement view) => view.Width;

	protected override double DetermineStart(VisualElement view) => view.X;
}

public partial class Unreported005 : ContentPage
{
	public Unreported005() => InitializeComponent();

	[Collection("Issue")]
	public class Tests
	{
		[Theory]
		[XamlInflatorData]
		internal void CustomMarkupExtensionWorks(XamlInflator inflator)
		{
			var page = new Unreported005(inflator);
			Assert.IsType<Constraint>(RelativeLayout.GetXConstraint(page.after));
			Assert.NotNull(RelativeLayout.GetXConstraint(page.after));
		}
	}
}