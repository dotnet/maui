#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls.SourceGen.UnitTests.HotReload;
using Xunit;

namespace Microsoft.Maui.Controls.SourceGen.UnitTests.HotReload.AiAssisted;

public partial class TemplateAndSelectorHotReloadTests
{
	// Wave-2 · Templates · TS-04 (GREEN anchor)
	// Provenance: MAUI §hot-reload ControlTemplate construction (namescope / x:Reference / VSM TargetName);
	//             SetNamescopesAndRegisterNamesVisitor via LoadTemplate (#36482).
	// Faithfulness: reaches ControlTemplate.CreateContent() and drives GoToState headlessly (pure BindableObject
	//               logic, per VisualStateManagerTests.TargetedVisualElementGoesToCorrectState); asserts each
	//               realization owns its namescope, x:Reference resolves to that realization's own Target, and a
	//               VSM TargetName setter mutates only its own realization; fails-for-bug: realizations share a
	//               namescope, x:Reference leaks across realizations, or a VSM setter escapes its root.
	// Expected: GREEN — verified empirically; x:Reference is asserted at construction only (mutating a bound
	//                   source needs a dispatcher headlessly), and the VSM setter targets an UNBOUND label so
	//                   GoToState never triggers a binding proxy.
	// Issue: https://github.com/dotnet/maui/issues/36482
	[MetadataUpdateFact]
	public void ControlTemplate_Construction_NamescopeXReferenceAndVsmAreIsolated()
	{
		using var harness = CreateHarness();
		var generation = harness.Generate(ControlTemplateXaml("V1", "V1-Active"), ControlTemplateXaml("V2", "V2-Active"));

		harness.RunLive(generation, live =>
		{
			var page = live.GetInstance<ContentPage>();
			var template = Assert.IsType<ControlTemplate>(page.Resources["Ct"]);

			// Two realizations from the SAME (V1) template.
			var rootA = Assert.IsType<VerticalStackLayout>(template.CreateContent());
			var rootB = Assert.IsType<VerticalStackLayout>(template.CreateContent());
			Assert.NotSame(rootA, rootB);

			var targetA = rootA.FindByName<Label>("Target");
			var targetB = rootB.FindByName<Label>("Target");
			var mirrorA = rootA.FindByName<Label>("Mirror");
			var mirrorB = rootB.FindByName<Label>("Mirror");
			var vsmTargetA = rootA.FindByName<Label>("VsmTarget");
			var vsmTargetB = rootB.FindByName<Label>("VsmTarget");
			Assert.NotNull(targetA);
			Assert.NotNull(targetB);
			Assert.NotNull(mirrorA);
			Assert.NotNull(mirrorB);
			Assert.NotNull(vsmTargetA);
			Assert.NotNull(vsmTargetB);

			// Per-realization namescope: the same name resolves to distinct elements.
			Assert.NotSame(targetA, targetB);
			Assert.NotSame(vsmTargetA, vsmTargetB);
			Assert.Equal("V1", targetA!.Text);

			// x:Reference resolved (at construction) to each realization's OWN Target.
			Assert.Equal("V1", mirrorA!.Text);
			Assert.Equal("V1", mirrorB!.Text);

			// VSM TargetName isolation: activating A mutates only A's (unbound) VsmTarget.
			Assert.Equal("idle", vsmTargetA!.Text);
			Assert.True(VisualStateManager.GoToState(rootA, "Active"));
			Assert.Equal("V1-Active", vsmTargetA.Text);
			Assert.Equal("idle", vsmTargetB!.Text);
			Assert.True(VisualStateManager.GoToState(rootA, "Normal"));
			Assert.Equal("idle", vsmTargetA.Text); // Active setters unapplied
		});
	}

	// Wave-2 · Templates · TS-04 (RED-PROBE — reclassified future realization)
	// Provenance: the ControlTemplate is a keyed resource replaced by a factory-less template on update (dumped);
	//             a held template keeps its V1 factory, so a future realization stays V1.
	// Faithfulness: reaches the post-update CreateContent() and FindByName; fails-for-bug: the next realization's
	//               Target is still "V1" instead of "V2".
	// Expected: RED-PROBE(#36482) — green anchor: ControlTemplate_Construction_NamescopeXReferenceAndVsmAreIsolated
	// Issue: https://github.com/dotnet/maui/issues/36482
	[MetadataUpdateFact(Skip = "ControlTemplate future realization does not reflect the edit under IHR (keyed-resource replacement) — see #36482; green anchor: ControlTemplate_Construction_NamescopeXReferenceAndVsmAreIsolated")]
	public void ControlTemplate_FutureRealizationReflectsNewVersion()
	{
		using var harness = CreateHarness();
		var generation = harness.Generate(ControlTemplateXaml("V1", "V1-Active"), ControlTemplateXaml("V2", "V2-Active"));

		harness.RunLive(generation, live =>
		{
			var page = live.GetInstance<ContentPage>();
			var template = Assert.IsType<ControlTemplate>(page.Resources["Ct"]);

			var rootA = Assert.IsType<VerticalStackLayout>(template.CreateContent());
			Assert.Equal("V1", rootA.FindByName<Label>("Target")!.Text);

			Assert.Same(page, live.ApplyUpdate<ContentPage>(1));

			var rootC = Assert.IsType<VerticalStackLayout>(template.CreateContent());
			Assert.Equal("V2", rootC.FindByName<Label>("Target")!.Text); // future realization should be V2
		});
	}
}
