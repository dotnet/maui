---
name: maui-accessibility
description: >
  Guide for making .NET MAUI apps accessible — screen reader support via SemanticProperties,
  heading levels, AutomationProperties visibility control, programmatic focus and announcements,
  and platform-specific gotchas for TalkBack, VoiceOver, and Narrator.
  USE FOR: "add accessibility", "screen reader support", "SemanticProperties", "AutomationProperties",
  "TalkBack", "VoiceOver", "Narrator", "accessible MAUI", "heading levels", "semantic description",
  "announce to screen reader", "accessibility audit".
  DO NOT USE FOR: general UI layout (use maui-collectionview or maui-shell-navigation),
  animations (use maui-animations), or gestures (use maui-gestures).
---

# .NET MAUI Accessibility Skill

Use this skill when adding or auditing accessibility in .NET MAUI apps. It covers
SemanticProperties, AutomationProperties, screen reader announcements, and
platform-specific pitfalls.

---

## SemanticProperties (Attached Properties)

Set on any `VisualElement` to provide screen reader context.

| Property | Purpose |
|---|---|
| `SemanticProperties.Description` | Accessible name read by screen reader |
| `SemanticProperties.Hint` | Extra context (e.g. "Double tap to activate") |
| `SemanticProperties.HeadingLevel` | Heading landmark: `None`, `Level1`–`Level9` |

### XAML

```xml
<Button Text="Save"
        SemanticProperties.Description="Save your changes"
        SemanticProperties.Hint="Double tap to save the current document" />

<Label Text="Settings"
       SemanticProperties.HeadingLevel="Level1" />
```

### C#

```csharp
var btn = new Button { Text = "Save" };
SemanticProperties.SetDescription(btn, "Save your changes");
SemanticProperties.SetHint(btn, "Double tap to save the current document");

var heading = new Label { Text = "Settings" };
SemanticProperties.SetHeadingLevel(heading, SemanticHeadingLevel.Level1);
```

---

## Programmatic Focus & Announcements

### SetSemanticFocus

Move screen reader focus to an element after a UI change:

```csharp
errorLabel.Text = "Username is required";
errorLabel.SetSemanticFocus();
```

### SemanticScreenReader.Announce

Push a live announcement without moving focus:

```csharp
SemanticScreenReader.Announce("File uploaded successfully");
```

Use `Announce` for transient status updates. Use `SetSemanticFocus` when the user
must interact with the target element.

---

## AutomationProperties

Control whether an element appears in the accessibility tree.

| Property | Effect |
|---|---|
| `AutomationProperties.IsInAccessibleTree` | `false` hides the element from screen readers |
| `AutomationProperties.ExcludedWithChildren` | `true` hides the element **and all descendants** |

```xml
<!-- Decorative image — hide from screen reader -->
<Image Source="bg.png"
       AutomationProperties.IsInAccessibleTree="false" />

<!-- Container with purely decorative content -->
<Grid AutomationProperties.ExcludedWithChildren="true">
    <Image Source="pattern.png" />
</Grid>
```

### Deprecated AutomationProperties → SemanticProperties

| Deprecated | Replacement |
|---|---|
| `AutomationProperties.Name` | `SemanticProperties.Description` |
| `AutomationProperties.HelpText` | `SemanticProperties.Hint` |

Avoid the deprecated properties in new code. They may not work consistently
across platforms and will be removed in a future release.

---

## Critical Platform Gotchas

### 1. Don't set Description on Label

Setting `SemanticProperties.Description` on a `Label` **overrides** the `Text`
property for screen readers. If Description matches Text, the label may be read
twice or behave unexpectedly. Omit Description and let the screen reader read
`Text` directly.

```xml
<!-- BAD — stops Text from being read naturally -->
<Label Text="Welcome"
       SemanticProperties.Description="Welcome" />

<!-- GOOD — screen reader reads Text automatically -->
<Label Text="Welcome" />
```

### 2. Android Entry/Editor: Description breaks TalkBack actions

On Android, setting `SemanticProperties.Description` on `Entry` or `Editor`
causes TalkBack to lose "double tap to edit" action hints. Use `Placeholder` or
`SemanticProperties.Hint` instead.

```xml
<!-- BAD on Android -->
<Entry SemanticProperties.Description="Email address" />

<!-- GOOD — use Placeholder for context -->
<Entry Placeholder="Email address" />
```

### 3. iOS: Description on parent hides children

On iOS/VoiceOver, setting `SemanticProperties.Description` on a layout (e.g.
`StackLayout`, `Grid`) makes the entire container a single accessible element,
making child elements **unreachable**.

```xml
<!-- BAD — children invisible to VoiceOver -->
<HorizontalStackLayout SemanticProperties.Description="User info">
    <Label Text="Name:" />
    <Label Text="Alice" />
</HorizontalStackLayout>

<!-- GOOD — let children be individually focusable -->
<HorizontalStackLayout>
    <Label Text="Name:" />
    <Label Text="Alice" />
</HorizontalStackLayout>
```

### 4. Hint conflicts with Entry.Placeholder on Android

On Android, `SemanticProperties.Hint` and `Entry.Placeholder` map to the same
Android attribute (`contentDescription` / `hint`). Setting both may cause one to
override the other. Choose one.

### 5. HeadingLevel platform differences

- **Windows (Narrator):** Supports all 9 heading levels (`Level1`–`Level9`).
- **Android (TalkBack) / iOS (VoiceOver):** Only distinguish "heading" vs
  "not heading". All levels (`Level1`–`Level9`) are treated identically.

Use `Level1`–`Level9` for semantic correctness; just know the hierarchy only
renders on Windows.

---

## Accessibility Checklist for Existing Pages

When auditing or retrofitting a page, work through this list:

1. **Images**: Add `SemanticProperties.Description` to meaningful images.
   Set `AutomationProperties.IsInAccessibleTree="false"` on decorative ones.
2. **Buttons/Controls**: Ensure icon-only buttons have `Description`.
   Text buttons generally don't need it.
3. **Entries/Editors**: Use `Placeholder` for context. Add `Hint` only if
   extra instruction is needed. Avoid `Description` (Android breakage).
4. **Labels**: Do **not** add `Description` — let `Text` speak for itself.
   Add `HeadingLevel` to section headers.
5. **Headings**: Mark page title as `Level1`, section titles as `Level2`, etc.
6. **Grouping**: Avoid `Description` on layout containers (iOS breakage).
   Use `ExcludedWithChildren` to hide decorative groups.
7. **Dynamic content**: Call `SemanticScreenReader.Announce` for status
   changes. Use `SetSemanticFocus` after navigation or error display.
8. **Tab order**: Set `TabIndex` on interactive controls for logical order.
9. **Test**: Run TalkBack (Android), VoiceOver (iOS/Mac), and Narrator
   (Windows) to verify the reading order and actions.
