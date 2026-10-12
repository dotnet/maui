# Package development, validation and release preparation

## Prepared artifact, not an already released package

`@maui/api` / `0.1.0-preview.1` is the working identity for this experimental
.NET-project SDK. Registry name availability, scope ownership and publishing
authorization are unverified. Do not advertise a registry release until one
actually exists. There is no `@microsoft` branding, companion NuGet release or
template release implicit in this npm artifact.

The versioned deliverable is the allowlisted ESM/declaration/documentation/MIT
tarball. Generated `dist`, npm caches, `node_modules`, tests, fixtures, package
lockfile, build-input stamp and build scripts are not shipped. Source and
declarations use conventional `.js` specifiers; consuming the tarball requires
neither compiling original TypeScript nor Vite.

## Build and validate locally

From the repository root, with Node 22.18+ (validated on Node 24.4.1/npm 11.5.2):

```sh
npm --prefix src/HybridWebView/JavaScript ci
npm --prefix src/HybridWebView/JavaScript run build
npm --prefix src/HybridWebView/JavaScript test
npm --prefix src/HybridWebView/JavaScript run pack-check
```

`ci` restores the locked TypeScript 7.0.2 development compiler; the SDK has no
runtime dependencies. When all locked tarballs are cached, `ci --offline`
avoids registry traffic. A missing proxy-registry metadata cache can prevent
creating a new lockfile even with installed tools; do not change global registry
or credentials. The checked-in lockfile enables normal clean restores.

Build cleans `dist`, compiles ESM and declarations and writes an ignored
deterministic input/output hash stamp. Node built-in contract tests import the
**built public exports**. `prepack` runs build/tests, so preparing a tarball
cannot silently ship missing or untested output.

`pack-check` executes local `npm pack` (not publication), checks every tar entry
against the exact allowlist, checks JS/declarations/metadata for private paths
or source references, then creates an ignored fresh consumer under `artifacts/`.
It generates a lockfile and uses offline `npm ci` to install **the tarball**,
not a symlink; compiles a TypeScript consumer against its published declarations
without `skipLibCheck`, then runs production ESM root/subpath imports. No original
SDK source tree is present in that installation. The checked tarball remains
under `src/HybridWebView/JavaScript/artifacts/` for local inspection.
No registry upload or publish command is run by these scripts.

For file-list-only inspection:

```sh
npm --prefix src/HybridWebView/JavaScript pack --dry-run
```

## Playground dependency and staging

The playground consumes a normal
`file:../../../../HybridWebView/JavaScript` npm dependency, not a TS alias or
source import. From a clean checkout restore both development environments:

```sh
npm --prefix src/HybridWebView/JavaScript ci
npm --prefix src/Controls/samples/Controls.Sample.HybridWebApp/Web ci
npm --prefix src/Controls/samples/Controls.Sample.HybridWebApp/Web test
npm --prefix src/Controls/samples/Controls.Sample.HybridWebApp/Web run stage
npm --prefix src/Controls/samples/Controls.Sample.HybridWebApp/Web run dev
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

See the [consumer README](https://github.com/dotnet/maui/blob/net11.0/src/Controls/samples/Controls.Sample.HybridWebApp/README.md)
for the cross-platform launcher and native prerequisites.

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
   preparation does not perform it.
6. Only after successful publication, document the actual registry URL,
   distribution tag, verified install command and matching native-host version.
