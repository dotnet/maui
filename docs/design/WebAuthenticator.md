# WebAuthenticator request lifecycle

`WebAuthenticator` opens the platform authentication surface and completes when the app receives
the configured callback route. OAuth and OIDC protocol responsibilities such as PKCE, state, nonce,
token exchange, and token validation remain in the application.

## Shared lifecycle

- A process can have one active built-in request. A second valid call throws
  `InvalidOperationException` without canceling the first request.
- Options are snapshotted before validation and are not retained as mutable platform state.
- Callback, cancellation, and failure race through one process-wide completion reservation.
- Decoder and platform callbacks run outside the manager lock and at most once.
- Cleanup completes before cancellation-registration disposal; request removal is the final step.
- A request cannot resume after process termination.

Route-only callbacks match the configured scheme and, when present, host, effective port, and
non-root path. Scheme and host comparisons ignore case; constrained paths compare exactly. Query
and fragment do not participate in route matching, and an expected root path does not constrain the
callback path. A lifecycle callback with a route mismatch is not consumed and leaves the request
pending. An identity-bound native result with a missing or mismatched URI is terminal for that request.

## Native callback routing

Native lifecycle callbacks attempt the process-wide built-in request before consulting the current
`WebAuthenticator.Default`. This prevents replacement of the static facade from orphaning an
active built-in request. Callbacks not handled by the built-in route can still flow to a custom
implementation or another lifecycle handler, and the built-in implementation is never invoked twice.

## Platform integration

### Windows

Packaged apps declare the callback protocol on the current manifest application. Unpackaged apps
use a protocol command owned by the current executable. A framework-owned `AppInstance` route key
identifies the callback owner; transient activation processes redirect to that owner and remain
alive until redirection completes. The route remains registered after the request because explicit
unregistration prevents reliable re-registration ([Windows App SDK #4420][windows-appsdk-4420]). A
later `FindOrRegisterForKey` can replace a previous framework route when the callback scheme changes,
as implemented by [Windows App SDK 2.3.1][windows-appsdk-appinstance]. If the app already owns the
current instance key, the framework preserves it and the app must cooperatively route protocol
activations. The originating window is brought to the foreground on a best-effort basis before
callback decoding and observable completion.

### iOS and Mac Catalyst

Both targets use `ASWebAuthenticationSession`. HTTPS callbacks require version 17.4 or later, the
default HTTPS port, and matching Associated Domains configuration. Native completion is posted to
the main queue before completing the shared request. The framework does not clear shared cookies.

### Android

Android prefers Auth Tab when a verified Custom Tabs provider supports it. Results are correlated
by request ID. HTTPS callbacks with a non-default port are not eligible for Auth Tab because that
transport cannot preserve the port. If Auth Tab is unavailable or ineligible, the implementation
falls back to a Custom Tab and then the system browser. Custom-scheme fallbacks require a matching
exported callback activity; HTTPS callbacks require matching Digital Asset Links.

## Diagnostics and limitations

Native and operating-system failures can be written to Debug output with their exception details.
Logs at URI, callback, manager, and application-decoder boundaries remain redacted and must not
include authorization URLs, callback values, query strings, codes, or tokens.

Native cancellation is reported when the selected transport exposes it. Closing an external
fallback browser is not always observable. `PrefersEphemeralWebBrowserSession` is best-effort and
is not guaranteed on Windows.

See the [Essentials samples README](../../src/Essentials/samples/README.md#webauthenticator) for the
zero-configuration sample and its production-security boundary.

[windows-appsdk-4420]: https://github.com/microsoft/WindowsAppSDK/issues/4420
[windows-appsdk-appinstance]: https://github.com/microsoft/WindowsAppSDK/blob/v2.3.1/dev/AppLifecycle/AppInstance.cpp
