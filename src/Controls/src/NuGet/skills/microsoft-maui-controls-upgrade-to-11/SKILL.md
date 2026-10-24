---
name: microsoft-maui-controls-upgrade-to-11
description: Upgrade a .NET MAUI Controls application to .NET MAUI 11, including SDK/workload and package alignment, version-specific handler customization, and behavior-preserving validation.
---

# Upgrade a Controls application to MAUI 11

Use this skill when the user asks to upgrade a .NET MAUI application to MAUI 11.
It supplies application guidance, not permission to install tools, change global
settings, publish packages, or modify files outside the application.

## Establish the source and requested target

Read the application's `global.json`, project files, `Directory.Build.props`,
`Directory.Packages.props` when present, and its platform customizations.
Identify how the app sets `MauiVersion` and direct package versions. Do not add
redundant transitive package references or replace central package management
with per-project versions.

Establish a working MAUI 10 build and runnable baseline for the platforms being
upgraded. For MAUI 8/9, stage the MAUI 10 baseline first; do not promise a tested
direct upgrade. MAUI 6/7 and Xamarin.Forms migrations are outside this skill.

Select the user's requested MAUI 11 release, with a compatible .NET 11 SDK,
workload set and platform toolchain. A NuGet version alone does not install the
SDK or workload. Check `dotnet --info` and `dotnet workload list`; report missing
prerequisites instead of automatically provisioning or disabling toolchain checks.
See the [upgrade checklist](references/upgrade-checklist.md).

## Reconcile changes with the exact package

Prefer the restored package's reference assemblies, XML documentation and
`.nuspec` repository commit when checking an API. A release branch, `main`, or a
what's-new headline may describe a different implementation from the selected
preview/RC package. Follow the package's exact source revision when the two
disagree; do not guess undocumented switches or declare an opt-in to be a default.

Update SDK selection, framework targets, supported platform versions, and
existing direct package/MauiVersion declarations together. Preserve explicit
platform versions when compatible with the selected SDK; a recognized suffixed
TFM is not inherently an error. Verify the evaluated project and fresh restore
graph rather than inferring the selected API from the project text alone.

## Review native customization, not just compilation

Find handler registrations, renderer subclasses, mapper changes, event
subscriptions and assumptions about native view ancestry. A compatibility
renderer can still compile and preserve a screenshot without using the new
default architecture. Do not register an old renderer solely to conceal an
incomplete migration; discuss any intentionally requested fallback explicitly.

For Android Shell renderer customizations, read the
[Android Shell reference](references/android-shell.md). Other platforms must be
checked against their own release-specific APIs; Android findings are not proof
of an iOS, Mac Catalyst or Windows port.

Preserve routes/query delivery, commands, binding, navigation/back behavior,
toolbar appearance and lifecycle cleanup. Keep customizations scoped to the
intended control; avoid a global mapper mutation unless the app needs it.

## Verify the delivered application

Restore and build Debug and Release for every claimed platform. Exercise the
existing behavior on a device/simulator: open a route, perform its actions,
navigate back using both app chrome and system navigation, and reopen the page.
Check that event handlers are not duplicated and page state has the intended
lifetime. When the upgrade requires the target default implementation, inspect
the actual handler/native hierarchy as well as visible behavior.

Use evaluated build output paths for installation, not a guessed TFM directory.
Classify SDK/Xcode mismatch, device ANR, restore/network and disk failures as
infrastructure unless evidence establishes an application defect. Compile-only
or screenshots alone are not runnable-app validation.

Report the exact versions changed, relevant behavior checks, and any platform
that remains unverified. Do not claim a successful whole-app upgrade from a
single passing platform or silently weaken a check to obtain a green result.
