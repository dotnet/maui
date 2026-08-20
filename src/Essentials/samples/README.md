# .NET MAUI Essentials — samples

This folder holds the **Essentials.Sample** app and the small reference **servers** that some of its
pages talk to. Each scenario has its own setup + testing guide — start there.

## Scenarios

### Passkeys (WebAuthn / FIDO2)

Creating and using passkeys with the platform authenticator (Face ID / Touch ID / Windows Hello /
Android), verified by a minimal ASP.NET Core Identity relying-party server.

- **Guide:** [README-Passkeys.md](README-Passkeys.md)
- **Reference server:** [`Samples.Server.Passkeys/`](Samples.Server.Passkeys)
- **Setup:** [`Configure-Passkeys.ps1`](Configure-Passkeys.ps1)

### WebAuthenticator

The sample uses the public reference broker at
`https://xamarin-essentials-auth-sample.azurewebsites.net/mobileauth/`, so browser launch and
callback delivery can be tested without configuring provider credentials or running a local
server. Open the Web Authenticator page, choose a provider, complete sign-in, and verify that the
page reports a received callback. Token values are not displayed or logged.

This demonstrates the `WebAuthenticator` transport contract, not a production OAuth architecture.
Production apps should normally use Authorization Code Flow with PKCE, validate state and OIDC
tokens as applicable, and never embed a client secret. `WebAuthenticator` itself remains protocol
agnostic and does not exchange or validate tokens.

Android custom-scheme fallback requires a matching exported callback activity. iOS and Mac
Catalyst register the callback through `CFBundleURLTypes`. Packaged Windows apps declare the
protocol on the current manifest application; unpackaged apps register a protocol command owned by
the current executable. Closing an external fallback browser is not observable on every platform,
so the page also provides explicit cancellation.

The source under [`Sample.Server.WebAuthenticator/`](Sample.Server.WebAuthenticator) is included
for reference and is not required by the default public-broker sample path. Automated tests must
not depend on the public service.

## Projects

- **[`Samples/`](Samples)** — the **Essentials.Sample** MAUI app (all the Essentials demo pages,
  including the **Passkeys** page).
- **[`Samples.Server.Passkeys/`](Samples.Server.Passkeys)** — the passkeys relying-party (RP) server
  (small and commented — read the code for endpoint/auth details).
- **[`Sample.Server.WebAuthenticator/`](Sample.Server.WebAuthenticator)** — source for the
  WebAuthenticator reference server; it is not required by the default public-broker sample path.

Pick the scenario you want to test from the table above and follow its guide.
