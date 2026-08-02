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

Browser-based OAuth sign-in via `WebAuthenticator`, brokered by a small reference server.

- **Guide:** _coming — being reworked_
- **Reference server:** [`Sample.Server.WebAuthenticator/`](Sample.Server.WebAuthenticator)

## Projects

- **[`Samples/`](Samples)** — the **Essentials.Sample** MAUI app (all the Essentials demo pages,
  including the **Passkeys** page).
- **[`Samples.Server.Passkeys/`](Samples.Server.Passkeys)** — the passkeys relying-party (RP) server
  (small and commented — read the code for endpoint/auth details).
- **[`Sample.Server.WebAuthenticator/`](Sample.Server.WebAuthenticator)** — the WebAuthenticator
  reference server.

Pick the scenario you want to test from the table above and follow its guide.
