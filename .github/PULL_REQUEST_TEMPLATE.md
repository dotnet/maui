### Description of Change ###

Describe your changes here. 

### Bugs Fixed ###

- Related to issue #

Provide links to issues here. Ensure that a GitHub issue was created for your feature or bug fix before sending PR.

### API Changes ###

List all API changes here (or just put None), example:

Added: 
 
- `string Class.Property { get; set; }`
- `void Class.Method();`

Changed:

 - `object Cell.OldPropertyName` => `object Cell.NewPropertyName`
 
If there is an entirely new API, then you can use a more verbose style:

```csharp
public static class NewClass {
    public static int SomeProperty { get; set; }
    public static void SomeMethod(string value);
}
```


### Behavioral Changes ###

Describe any non-bug related behavioral changes that may change how users app behaves when upgrading to this version of the codebase.

### PR Checklist ###

- [ ] Has tests (if omitted, state reason in description)
- [ ] Has samples (if omitted, state reason in description)
- [ ] Rebased on top of `main` at time of PR
- [ ] Changes adhere to coding standard
- [ ] Updated documentation ([see walkthrough](https://github.com/xamarin/Essentials/wiki/Documenting-your-code-with-mdoc))
