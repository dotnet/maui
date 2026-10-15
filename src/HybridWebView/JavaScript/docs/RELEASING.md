# Package development, validation and release preparation

## Prepared artifact, not an already released package

`@maui/api` / `0.1.0-preview.1` is the working identity for this experimental
.NET-project SDK. Registry name availability, scope ownership and publishing
authorization are unverified. Do not advertise a registry release until one
actually exists. There is no `@microsoft` branding, companion NuGet release or
template release implicit in this npm artifact. The sibling managed WIP product
`Microsoft.Maui.Controls.HybridWebApps` / `0.1.0-preview.1` is now independently packable, with its own
README/license/XML documentation. Its NuGet identity is likewise unverified;
NuGet publication requires separate authorization and is not npm publication.
The npm artifact never includes managed/sample binaries or source.

The versioned deliverable is the allowlisted ESM/declaration/documentation/MIT
tarball. Generated `dist`, npm caches, `node_modules`, tests, fixtures, package
lockfile, build-input stamp and build scripts are not shipped. Source and
declarations use conventional `.js` specifiers; consuming the tarball requires
neither compiling original TypeScript nor Vite.

## Build and validate locally

From the product tree root (`src/HybridWebView/`), with Node 22.18+
(validated on Node 24.4.1/npm 11.5.2). These same paths work after copying the
whole bundle to another repository:

```sh
npm --prefix JavaScript ci
npm --prefix JavaScript run build
npm --prefix JavaScript test
npm --prefix JavaScript run pack-check
```

`ci` restores the locked TypeScript 7.0.2 development compiler and documentation
development tools; the SDK has no
runtime dependencies. When all locked tarballs are cached, `ci --offline`
avoids registry traffic. A missing proxy-registry metadata cache can prevent
creating a new lockfile even with installed tools; do not change global registry
or credentials. The checked-in lockfile enables normal clean restores.

Build cleans `dist`, compiles ESM and declarations with comments preserved, and
writes an ignored deterministic input/output hash stamp. Node built-in contract tests import the
**built public exports**. The double-bootstrap regression executes the real
framework's attributed test-only snapshot (`tests/fixtures/README.md`), not a
MAUI checkout or a production SDK bootstrap. Tests also generate the reference and validate
source documentation from emitted declarations. `prepack` runs build/docs/tests,
so preparing a tarball cannot silently ship missing or untested output.

`pack-check` executes local `npm pack` (not publication), checks every tar entry
against the exact allowlist, checks JS/declarations/metadata for private paths
or source references, then creates an ignored fresh consumer under `artifacts/`.
It generates a lockfile and uses offline `npm ci` to install **the tarball**,
not a symlink; compiles a TypeScript consumer against its published declarations
without `skipLibCheck`, then runs production ESM root/subpath imports. No original
SDK source tree is present in that installation. It additionally runs API
Extractor/API Documenter against the installed tarball's declarations to prove
that every exported symbol/public member retains its source TSDoc. Packed-reference
output goes to ignored `artifacts/api-reference-packed/`.
Each entrypoint's exact exported symbols, nested public members, signatures and
TSDoc are compared with its local model; counts alone are not acceptance evidence.
The checked tarball remains under `JavaScript/artifacts/`
for local inspection.
No registry upload or publish command is run by these scripts.

For file-list-only inspection:

```sh
npm --prefix JavaScript pack --dry-run
```

## Source reference and DocFX/Microsoft Learn

```sh
npm --prefix JavaScript run docs
```

The comments in `JavaScript/src/**/*.ts` are the public-symbol documentation source.
The command builds with **TypeScript 7.0.2**, then runs **API Extractor 7.59.4**
on each actual exported `.d.ts` entrypoint and **API Documenter 7.30.18**
on each isolated `.api.json` model. It writes ignored
`artifacts/api-reference/{root,core,essentials/*}/{input,markdown}/`, plus
an import-path `index.md` and `toc.yml`. It fails for extraction/compiler/TSDoc
warnings or missing summaries/parameter documentation on exported declarations
and public members. No fake packages or feature re-exports are created for docs.
Cross-subpath core-type references intentionally remain references; they do not
become feature exports.

The extractor includes the package's own declarations when following re-exports:
TypeScript classifies relative imports under an installed `node_modules` package
as external, even though they are still the same package. The standard
`bundledPackages: ["@maui/api"]` setting handles that classification. Without it,
the installed root's ten core exports (32 documented symbols/members in total)
were omitted by extraction, producing 65 rather than 97 modeled items; no `.d.ts`
files or comments had been lost. Pack-check now compares exact per-entrypoint
API shapes/comments and requires nonempty extraction, so this cannot silently pass.
This setting does not add feature exports to the root or change the package.

API Extractor bundles its own TypeScript **5.9.3** to inspect these simple
declarations, independently of the SDK build compiler; neither the SDK nor the
packed-consumer compiler is downgraded. These commands verify compatibility with
the currently emitted declaration syntax. TypeDoc 0.28.20's peer range ends at
TypeScript 6.0, so installing it directly against this SDK's TypeScript 7 is not
the chosen route. Recheck generator/declaration compatibility when introducing
new TypeScript syntax. Tools are dev dependencies only, never runtime/shipping files.

For a **local DocFX conceptual site**, copy the generated `api-reference/`
directory to the documentation site's content tree. Add its Markdown and
`toc.yml` to that site's `docfx.json` build content, for example:

```json
{
  "build": {
    "content": [
      { "files": ["api-reference/index.md", "api-reference/**/markdown/*.md", "api-reference/toc.yml"] }
    ],
    "dest": "_site"
  }
}
```

Link the generated `api-reference/toc.yml` from the site's main TOC, then run
the site's existing `docfx build` and validate links/navigation. DocFX site
rendering is a separate integration step, not executed by this npm package.
The generated per-entrypoint Markdown uses directory-local links and labels the
exact import path on every page.

[API Extractor's official generation guide](https://api-extractor.com/pages/setup/generating_docs/)
also documents `api-documenter yaml` for DocFX-compatible reference input.
Do not merge these isolated models as though they were different packages:
all represent the same real `@maui/api` package, so a single merged YAML/reference
site would need a reviewed subpath-aware UID/link conversion to avoid collisions.
The working command above uses Markdown, not a claimed turnkey Learn YAML pipeline.
[Modern DocFX language support](https://github.com/dotnet/docfx/blob/main/docs/docs/additional-languages.md)
does not directly extract this TypeScript SDK like .NET metadata.

**Microsoft Learn publication requires separate docs-repository/site onboarding**:
agree ownership and preview support policy, select a docs repository and route,
add its required front matter/metadata and TOC entries, integrate this generation
command into that repository's build, and validate its content/link checks.
Generated `.api.json`/Markdown is an input artifact, not an automatic Learn page
or a publication authorization. No live Learn reference is claimed here.

## Playground dependency and staging

The playground consumes a normal
`file:../../../JavaScript` npm dependency, not a TS alias or
source import. From the product tree root restore both development environments:

```sh
npm --prefix JavaScript ci
npm --prefix samples/HybridWebApp/Web ci
npm --prefix samples/HybridWebApp/Web test
npm --prefix samples/HybridWebApp/Web run stage
npm --prefix samples/HybridWebApp/Web run dev
```

Playground `dev`, `test`, `type-check` and `build` rebuild the SDK first; the
local file link still resolves emitted package exports. Source-only edits to the
SDK are not live TypeScript inputs to Vite: run `Web`'s `sdk:build`, and restart
Vite after changing dependency exports. App UI/HMR behavior is unchanged.

`stage` rebuilds both SDK and app and emits the native asset manifest explicitly,
before native MSBuild. It verifies the SDK hash stamp and includes SDK source,
build scripts/config/lockfile and emitted output in native freshness validation.
Its manifest adds current SDK input globs, so newly added/deleted SDK files also
invalidate staging. Only manifest-listed Vite assets become native `wwwroot`.
Direct `build` removes the staging manifest; native MSBuild never runs npm.
After SDK edits/builds, rerun `stage` before a packaged native build.

The current identity wire mapping uses separate `app.info`, `device.info` and
sample-owned `sample.diagnostics` commands, with unchanged public JS return types.
Relaunch the matching native host before acceptance; the old combined response
is not the current unpublished preview contract.

The current source bundle's README documents its copyable layout and native
migration gates. The [matching consumer README](https://github.com/dotnet/maui/blob/f0c36030b40a35daaae7256df2b79e1058774e82/src/HybridWebView/samples/HybridWebApp/README.md)
pins the complete portable source layout for tarball readers. Pin source links
to the selected release commit/tag before publication. A copied native
app and managed library require an explicit MAUI 11 package version/feed and SDK/workloads;
packaged Vite forwarding remains guarded until the consumed Core fix is
validated. Managed-only tests use the actual library, Essentials and Controls/Core
assemblies. The current single WIP managed product deliberately includes MAUI UI
providers; a copied bundle needs real compatible Essentials, Controls.Core,
Core and transitive dependencies. It no longer has an Essentials-only boundary.

## Authorized release checklist

1. Verify the scope/name and the release operator's authorization. Confirm the
   .NET MAUI maintainers' naming/support policy; the working name alone grants
   no rights. Scoped public-package access must be selected deliberately.
2. Select an explicit preview version and intended prerelease distribution tag;
   do not unintentionally place an experimental build on `latest`. Update
   package/lockfile, compatibility documentation and matching host contracts.
3. Run the commands above and inspect the actual tarball. Preserve repository
   MIT attribution. Confirm no sample providers/UI, tokens, private paths,
   native files or unintended dependencies entered the artifact.
4. Validate the matching native host on the intended platform/runtime matrix,
   including permissions, cancellation, Unicode, sessions, HMR/disposal and
   native asset freshness. Package tests alone are insufficient release evidence.
5. Review release notes/support status and any authentication/provenance
   requirements with the authorized maintainer. Keep credentials outside source
   and logs. Publication is a separate, explicit maintainer operation; this
   preparation does not perform it. Pin source/reference links to the matching
   release tag or commit; never link to unmerged files on a default branch.
6. Only after successful publication, document the actual registry URL,
   distribution tag, verified install command and matching native-host version.
