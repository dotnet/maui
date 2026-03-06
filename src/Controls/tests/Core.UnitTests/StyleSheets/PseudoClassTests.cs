// Pseudo-class pseudo-selector test file (do not compile — demonstrates VSM mapping)
// When :hover/:focus/:disabled selectors are used, they should create VisualStateManager states

[Test]
public void PseudoClassSelectorParsing()
{
	// Test that pseudo-class selectors parse correctly
	var hoverSelector = Selector.Parse("button:hover");
	Assert.That(hoverSelector, Is.Not.EqualTo(Selector.Invalid));

	var focusSelector = Selector.Parse("input:focus");
	Assert.That(focusSelector, Is.Not.EqualTo(Selector.Invalid));

	var disabledSelector = Selector.Parse(".btn:disabled");
	Assert.That(disabledSelector, Is.Not.EqualTo(Selector.Invalid));
}

[Test]
public void PseudoClassVsmMapping()
{
	// Test that pseudo-class styles create VSM states
	var button = new Button { Text = "Click Me" };
	var app = new MockApplication();
	
	var css = @"
		.primary { background-color: blue; }
		.primary:hover { background-color: darkblue; }
		.primary:focus { border-width: 2; }
		.primary:disabled { opacity: 0.5; }
	";
	
	var stylesheet = StyleSheet.FromString(css);
	button.Classes.Add("primary");
	
	app.Resources.Add(stylesheet);
	app.LoadPage(button);
	
	// Verify VisualStateManager was set up
	var vsg = VisualStateManager.GetVisualStateGroups(button);
	Assert.That(vsg, Is.Not.Null);
	
	// Verify CommonStates group exists
	var commonStates = vsg.FirstOrDefault(g => g.Name == "CommonStates");
	Assert.That(commonStates, Is.Not.Null);
	
	// Verify pseudo-class states exist
	var pointerOverState = commonStates.States.FirstOrDefault(s => s.Name == VisualStateManager.CommonStates.PointerOver);
	Assert.That(pointerOverState, Is.Not.Null);
	Assert.That(pointerOverState.Setters, Has.Count.GreaterThan(0));
	
	var focusedState = commonStates.States.FirstOrDefault(s => s.Name == VisualStateManager.CommonStates.Focused);
	Assert.That(focusedState, Is.Not.Null);
	Assert.That(focusedState.Setters, Has.Count.GreaterThan(0));
	
	var disabledState = commonStates.States.FirstOrDefault(s => s.Name == VisualStateManager.CommonStates.Disabled);
	Assert.That(disabledState, Is.Not.Null);
	Assert.That(disabledState.Setters, Has.Count.GreaterThan(0));
}
