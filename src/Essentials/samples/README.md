# .NET MAUI Essentials — samples

This folder holds the **Essentials.Sample** app and the small reference **servers** that some of its
pages talk to. Each scenario has its own setup + testing guide — start there.

## Scenarios

| Scenario | Guide | Reference server | What it demonstrates |
| --- | --- | --- | --- |
| **Passkeys (WebAuthn / FIDO2)** | [**README-Passkeys.md**](README-Passkeys.md) | [`Samples.Server.Passkeys/`](Samples.Server.Passkeys) | Creating and using passkeys with the platform authenticator (Face ID / Touch ID / Windows Hello / Android), verified by a minimal ASP.NET Core Identity relying-party server. Set up with [`Configure-Passkeys.ps1`](Configure-Passkeys.ps1). |
| **WebAuthenticator** | _(guide coming — being reworked)_ | [`Sample.Server.WebAuthenticator/`](Sample.Server.WebAuthenticator) | Browser-based OAuth sign-in via `WebAuthenticator`, brokered by a small reference server. |

## Projects

- **[`Samples/`](Samples)** — the **Essentials.Sample** MAUI app (all the Essentials demo pages,
  including the **Passkeys** page).
- **[`Samples.Server.Passkeys/`](Samples.Server.Passkeys)** — the passkeys relying-party (RP) server.
  See its [README](Samples.Server.Passkeys/README.md).
- **[`Sample.Server.WebAuthenticator/`](Sample.Server.WebAuthenticator)** — the WebAuthenticator
  reference server.

Pick the scenario you want to test from the table above and follow its guide.
