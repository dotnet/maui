# API Change Tracking

.NET MAUI makes use of the `Microsoft.CodeAnalysis.PublicApiAnalyzers` NuGet to generate and track changes to public APIs to ensure that existing assemblies continue working without accidental breaking changes.

To learn more about breaking change guidelines in .NET, including what is considered a "breaking change", check out these resources:

* [.NET Runtime Breaking Change Rules](https://github.com/dotnet/runtime/blob/main/docs/coding-guidelines/breaking-change-rules.md)
* [.NET Runtime Breaking Change Definitions](https://github.com/dotnet/runtime/blob/main/docs/coding-guidelines/breaking-change-definitions.md)
* [.NET Library Guidance Breaking changes](https://learn.microsoft.com/dotnet/standard/library-guidance/breaking-changes)


## Changing APIs

In most cases, you are adding new APIs, so this is pretty simple and intellisense can be used. For changes and removes, these need to be properly tracked.

### Adding New APIs

If you are adding a new API, just go ahead and create the type/member and then use intellisense `ctrl + .` to fix the warning messages. You will have to do this for each TFM in the dropdown.

If this does not work, you can do it manually by adding the fully qualified type/member name to the `PublicAPI.Unshipped.txt` file in each TFM where it is relevant.

For example, we we are adding a new constructor to `PointerEventArgs`, we would add this line to unshipped file:

```
Microsoft.Maui.Controls.PointerEventArgs.PointerEventArgs(int clicks) -> void
```

### Changing / Removing APIs

When an API is changed, the old API needs to be "removed" and then the new API can be added. Removing an API is as easy as copying the type/member from the `PublicAPI.Shipped.txt` file and added to the `PublicAPI.Unshipped.txt` with a prefixof `*REMOVED*`.

For example, if we are removing a default constructor for some reason on the `PointerEventArgs` type:

```
Microsoft.Maui.Controls.PointerEventArgs.PointerEventArgs() -> void
```

All we would do is copy this line from the shipped file and add it to the unshipped file with the prefix:

```
*REMOVED*Microsoft.Maui.Controls.PointerEventArgs.PointerEventArgs() -> void
```

## Preparing API Releases

Once everything is ready to be released and the branch is created, we can merge all the unshipped files into the shipped files. Then we can run a git diff on the files and see all the changes made between releases.

### Merging the API Files

To merge the API files, we just need to run a script - and then commit the changes to git:

```
.\eng\scripts\mark-shipped.ps1
```

Taking the examples above and assuming we had these files:

`PublicAPI.Shipped.txt`:
```
#nullable enable
Microsoft.Maui.Controls.PointerEventArgs.PointerEventArgs() -> void
```

`PublicAPI.Unshipped.txt`:
```
#nullable enable
Microsoft.Maui.Controls.PointerEventArgs.PointerEventArgs(int clicks) -> void
*REMOVED*Microsoft.Maui.Controls.PointerEventArgs.PointerEventArgs() -> void
```

After running the script, we will end up with this:

`PublicAPI.Shipped.txt`:
```
#nullable enable
Microsoft.Maui.Controls.PointerEventArgs.PointerEventArgs(int clicks) -> void
```

`PublicAPI.Unshipped.txt`:
```
#nullable enable
```

Note the "replaced" constructor after the API has been removed and the new one added.

### Creating an API Diff

Once we have committed the API files, we can use git to generate a diff:

```
git diff <previous-branch> <new-branch> --output=api-changes.diff **\PublicAPI.*.txt
```

And again the example above, we will have this result:

```diff
diff --git a/src/PublicAPI/net/PublicAPI.Shipped.txt b/src/Core/src/PublicAPI/net/PublicAPI.Shipped.txt
index shashasha..shashasha
--- a/src/Core/src/PublicAPI/net/PublicAPI.Shipped.txt
+++ b/src/Core/src/PublicAPI/net/PublicAPI.Shipped.txt
@@ -1,2 +1,4 @@
 #nullable enable
-Microsoft.Maui.Controls.PointerEventArgs.PointerEventArgs() -> void
+Microsoft.Maui.Controls.PointerEventArgs.PointerEventArgs(int clicks) -> void
```

This diff can then be used to create a correct breaking API doc.

## Enabling Tracking

In order to enable tracking of a particular project, just add the following to the bottom of the .csproj:

```xml
<Import Project="$(MauiSrcDirectory)PublicAPI.targets" />
```

This will automatically start your project failing to build about missing things. This is just because the API tracker files are missing. We can use the `ctrl + .` or intellisense to automatically create them. Select each TFM from the dropdown in turn and pick any warning to use intellisense to generate the file for the entire project.

For each TFM, there will be a set of files that are created in a `PublicAPI` folder:

```
PublicAPI
 ├─ net
 │   ├─ PublicAPI.Shipped.txt
 │   └─ PublicAPI.Unshipped.txt
 ├─ net-android
 │   ├─ PublicAPI.Shipped.txt
 │   └─ PublicAPI.Unshipped.txt
 ├─ net-ios
 │   ├─ PublicAPI.Shipped.txt
 │   └─ PublicAPI.Unshipped.txt
 ├─ net-maccatalyst
 │   ├─ PublicAPI.Shipped.txt
 │   └─ PublicAPI.Unshipped.txt
 ├─ net-tizen
 │   ├─ PublicAPI.Shipped.txt
 │   └─ PublicAPI.Unshipped.txt
 ├─ net-windows
 │   ├─ PublicAPI.Shipped.txt
 │   └─ PublicAPI.Unshipped.txt
 └─ netstandard
     ├─ PublicAPI.Shipped.txt
     └─ PublicAPI.Unshipped.txt
```

If for some reason a file is not generated, they can be created manually with the contents of:

```
#nullable enable

```
