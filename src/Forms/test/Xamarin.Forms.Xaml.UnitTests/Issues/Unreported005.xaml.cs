using System;
using NUnit.Framework;

namespace Xamarin.Forms.Xaml.UnitTests
{
	public abstract class Unreported005RelativeToView : IMarkupExtension
	{
		protected Unreported005RelativeToView()
		{
			Factor = 1;
		}

		public string ElementName { get; set; }

		public double Factor { get; set; }

		public double Constant { get; set; }

		public object ProvideValue(IServiceProvider serviceProvider)
		{
			var element = new ReferenceExtension { Name = ElementName }.ProvideValue(serviceProvider) as View;
			if (element != null)
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
		protected override double DetermineExtent(VisualElement view)
		{
			return view.Width;
		}

		protected override double DetermineStart(VisualElement view)
		{
			return view.X;
		}
	}

	//[XamlCompilation(XamlCompilationOptions.Skip)]
	public partial class Unreported005 : ContentPage
	{
		public Unreported005()
		{
			InitializeComponent();
		}

		public Unreported005(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		class Tests
		{
			[TestCase(true), TestCase(false)]
			public void CustomMarkupExtensionWorks(bool useCompiledXaml)
			{
				var page = new Unreported005(useCompiledXaml);
				Assert.That(RelativeLayout.GetXConstraint(page.after), Is.TypeOf<Constraint>());
				Assert.NotNull(RelativeLayout.GetXConstraint(page.after));
			}
		}
	}
}