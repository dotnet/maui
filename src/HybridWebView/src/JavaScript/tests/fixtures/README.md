# Framework regression snapshot

`hybridwebview.js` is a byte-for-byte, test-only snapshot of
`src/Core/src/Handlers/HybridWebView/HybridWebView.js` in dotnet/maui.
Its last source change was commit
[`6af46b209c67b8ac122ab5abe1edaf02b2eefbef`](https://github.com/dotnet/maui/blob/6af46b209c67b8ac122ab5abe1edaf02b2eefbef/src/Core/src/Handlers/HybridWebView/HybridWebView.js).
SHA-256: `759c0ebaac292e4d75dee625665c032e63dab3bf979881b52a8b2005a67c5673`.
The repository's MIT license applies.

The disconnect-during-bootstrap regression executes this actual framework
script in a VM, including the native WebView2 listener and real `InvokeDotNet`
serialization. It checks one initialization/listener and one delivered event
after cancellation/replacement; it is not a substitute bridge implementation.
The fixed snapshot makes the test independent of a MAUI source checkout.
When updating the supported framework, explicitly refresh the snapshot, source
commit and hash, then rerun SDK tests and native acceptance.

This fixture is not SDK runtime code, not loaded by the app and not included in
the npm tarball. Production always loads the consumed MAUI version's
framework-owned `/_framework/hybridwebview.js`.
