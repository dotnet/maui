# ShellContent query parameters

`ShellContent.QueryParameters` supplies instance-specific data to the page created by a
`ShellContent`, including when users select tabs or flyout items without calling
`Shell.GoToAsync`.

```xaml
<Tab Title="Reminders">
    <ShellContent Title="Yesterday"
                  ContentTemplate="{DataTemplate pages:ReminderPage}">
        <ShellContent.QueryParameters>
            <ShellContentQueryParameter Name="date"
                                        Value="{Binding Yesterday}" />
        </ShellContent.QueryParameters>
    </ShellContent>
    <ShellContent Title="Today"
                  ContentTemplate="{DataTemplate pages:ReminderPage}">
        <ShellContent.QueryParameters>
            <ShellContentQueryParameter Name="date"
                                        Value="{Binding Today}" />
        </ShellContent.QueryParameters>
    </ShellContent>
</Tab>
```

The page receives these values through the existing `IQueryAttributable` and
`QueryPropertyAttribute` mechanisms. The route continues to identify the navigation
destination; query parameters are instance data and don't participate in route registration
or uniqueness.

## Behavior

- Names are case-sensitive. Parameters with null or empty names are ignored.
- Null values are delivered as values rather than treated as removal.
- When names are duplicated, the last parameter in collection order wins. Removing or
  replacing it reveals the preceding value, if one exists.
- Parameters are applied on initial selection and whenever the user switches back to the
  content. Created pages are reused according to normal `ShellContent` lifecycle behavior.
- Changes to the collection, a parameter name, or a parameter value update the selected
  content immediately. Changes to inactive content are applied when it is next selected.
- Parameter objects inherit the `ShellContent` binding context, so `Name` and `Value` can use
  bindings. Removed and replaced objects are detached from that inherited context and no
  longer observed.
- Parameters passed explicitly to `Shell.GoToAsync` take precedence over content parameters
  with the same name. Content parameters supply values for keys not present in the navigation
  data.
- Exceptions raised by the target's query handling are propagated unchanged.
