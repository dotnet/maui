# Triggering Azure DevOps Pipelines from GitHub Actions (No PAT)

This guide explains how to invoke Azure DevOps pipelines (e.g. in **dnceng-public** or **DevDiv**)
from GitHub Actions using **OIDC federated credentials** — no PAT or stored secrets needed.

## Architecture

```
GitHub Actions ──► GitHub OIDC Provider ──► Azure AD (federated credential) ──► AzDO REST API
                   (JWT id-token)           (exchange for bearer token)          (Run Pipeline)
```

1. The workflow requests an OIDC JWT from GitHub's token endpoint
2. The JWT is exchanged with Azure AD via the managed identity's federated credential
3. Azure AD returns a bearer token scoped to Azure DevOps
4. The bearer token is used to call the AzDO REST API to trigger the pipeline

> **Important:** The `azure/login` GitHub Action may be **blocked by org policy**
> (e.g. in the `dotnet` org). The workflow uses **manual OIDC token exchange via
> `curl`** instead, which works everywhere that `id-token: write` is allowed.

## Prerequisites

- Azure CLI installed locally (for one-time setup)
- Access to an Azure subscription + resource group
- **Project Collection Administrator** (or delegated) access in the target AzDO org to add users
- GitHub repo admin access to configure secrets

---

## Step 1: Create a User-Assigned Managed Identity

```bash
# Choose your resource group and identity name
RG="rg-maui-automation"
IDENTITY_NAME="id-maui-azdo-trigger"
LOCATION="eastus"

# Create the resource group if it doesn't exist
az group create --name $RG --location $LOCATION

# Create the managed identity
az identity create --name $IDENTITY_NAME --resource-group $RG --location $LOCATION

# Capture the IDs you'll need
CLIENT_ID=$(az identity show --name $IDENTITY_NAME --resource-group $RG --query clientId -o tsv)
PRINCIPAL_ID=$(az identity show --name $IDENTITY_NAME --resource-group $RG --query principalId -o tsv)
TENANT_ID=$(az account show --query tenantId -o tsv)
SUBSCRIPTION_ID=$(az account show --query id -o tsv)

echo "CLIENT_ID:       $CLIENT_ID"
echo "PRINCIPAL_ID:    $PRINCIPAL_ID"
echo "TENANT_ID:       $TENANT_ID"
echo "SUBSCRIPTION_ID: $SUBSCRIPTION_ID"
```

## Step 2: Add OIDC Federated Credential for GitHub Actions

This lets GitHub Actions authenticate as the identity without storing any secrets.

> **Critical: Subject claim is CASE-SENSITIVE.** The GitHub username/org in the
> subject must match the exact casing used by GitHub (e.g. `JanKrivanek` not
> `jankrivanek`). A mismatch produces `AADSTS70021`.

> **Microsoft tenant restriction:** For managed identities in the Microsoft
> corporate tenant (`72f988bf-...`), the OIDC token must include an `enterprise`
> claim with value `microsoft`, `github`, or `microsoftopensource`. Personal forks
> outside these GitHub Enterprise orgs will fail with `AADSTS7002381`.
> This means **only repos in `dotnet`, `microsoft`, etc. orgs work** — not personal forks.

```bash
# Allow from main branch
az identity federated-credential create \
  --name github-actions-main \
  --identity-name $IDENTITY_NAME \
  --resource-group $RG \
  --issuer "https://token.actions.githubusercontent.com" \
  --subject "repo:dotnet/maui:ref:refs/heads/main" \
  --audiences "api://AzureADTokenExchange"
```

Add more federated credentials for other branches or trigger types as needed:

```bash
# Specific dev branch
az identity federated-credential create \
  --name github-actions-dev-branch \
  --identity-name $IDENTITY_NAME \
  --resource-group $RG \
  --issuer "https://token.actions.githubusercontent.com" \
  --subject "repo:dotnet/maui:ref:refs/heads/dev/myteam/feature" \
  --audiences "api://AzureADTokenExchange"

# Pull request events
az identity federated-credential create \
  --name github-actions-pr \
  --identity-name $IDENTITY_NAME \
  --resource-group $RG \
  --issuer "https://token.actions.githubusercontent.com" \
  --subject "repo:dotnet/maui:pull_request" \
  --audiences "api://AzureADTokenExchange"

# GitHub environment (recommended for production — enables approval gates)
az identity federated-credential create \
  --name github-actions-env-azdo \
  --identity-name $IDENTITY_NAME \
  --resource-group $RG \
  --issuer "https://token.actions.githubusercontent.com" \
  --subject "repo:dotnet/maui:environment:azdo-trigger" \
  --audiences "api://AzureADTokenExchange"
```

## Step 3: Add the Identity to Azure DevOps

The managed identity must be added as a user in **each** AzDO organization you want to trigger pipelines in.

### Adding the identity

1. Go to the AzDO org → **Organization Settings** → **Users**
2. Click **Add users**
3. Search for the managed identity by its **display name**
4. Set **Access level** to **Basic** (see note below)
5. Add the user to the target project
6. Click **Add**

> **Critical: Access level must be Basic, not Stakeholder.** Stakeholder access
> does not grant sufficient permissions for build operations. Even with explicit
> "Queue builds" permissions, Stakeholder-level identities get `TF215106: Access
> denied` errors. Request **Basic** access when filing the request.

> **Important:** Use the identity's **Object (Principal) ID** from the
> **Enterprise Applications** pane in Entra admin center — NOT the App
> Registration object ID.

### Grant Build Queue Permission

The identity needs **"Queue builds"** permission on the target pipeline(s):

1. Go to the project → **Pipelines** → find the target pipeline
2. Click the **⋮** menu → **Manage security**
3. Find your managed identity user
4. Set **"Queue builds"** to **Allow**

### Per-organization requirements

| AzDO Organization | Project | Example Pipelines |
|---|---|---|
| `dnceng-public` | `public` | 302 (maui-pr), 314 (maui-pr-devicetests) |
| `DevDiv` | `DevDiv` | 27723 |

## Step 4: Set GitHub Repository Secrets

In **dotnet/maui** → **Settings** → **Secrets and variables** → **Actions**, add:

| Secret Name | Value |
|---|---|
| `AZDO_TRIGGER_CLIENT_ID` | The managed identity's Client ID |
| `AZDO_TRIGGER_TENANT_ID` | Your Azure AD Tenant ID |
| `AZDO_TRIGGER_SUBSCRIPTION_ID` | Your Azure Subscription ID |

> Using distinct secret names (prefixed with `AZDO_TRIGGER_`) avoids conflicts
> with any existing `AZURE_*` secrets in the repo.

## Step 5: Create the GitHub Actions Workflow

See [`.github/workflows/trigger-azdo-pipeline.yml`](../workflows/trigger-azdo-pipeline.yml) for a ready-to-use workflow.

## How It Works (Token Flow)

```
1. Workflow declares `permissions: { id-token: write }` at job level
2. Step 1 requests an OIDC JWT from GitHub's token endpoint via
   $ACTIONS_ID_TOKEN_REQUEST_URL (audience: api://AzureADTokenExchange)
3. Step 2 sends the JWT to Azure AD token endpoint as a client_assertion
   (grant_type=client_credentials) for the managed identity's client_id
4. Azure AD validates the JWT against the federated credential and returns
   a bearer token scoped to AzDO (resource: 499b84ac-1321-427f-aa17-267ca6975798)
5. Step 3 calls POST dev.azure.com/{org}/{project}/_apis/pipelines/{id}/runs
   with the bearer token
6. AzDO validates the token, checks the identity's permissions, and queues the build
```

> **Why not `azure/login`?** The `dotnet` GitHub org restricts which third-party
> Actions can run. `azure/login@v3` causes `startup_failure` because it's not in
> the org's allowed actions list. The manual `curl`-based OIDC exchange achieves
> the same result without any third-party dependencies.

## Troubleshooting

| Symptom | Cause | Fix |
|---------|-------|-----|
| `startup_failure` (no logs at all) | Third-party Action blocked by org policy | Don't use `azure/login`. Use manual `curl`-based OIDC exchange. |
| `AADSTS70021: No matching federated identity record found` | Subject claim case mismatch | Federated credential subject is **case-sensitive**. Use exact GitHub username casing (e.g. `JanKrivanek` not `jankrivanek`). |
| `AADSTS7002381: ... enterprise claim ... actual value is ''` | Personal fork outside GitHub Enterprise | Microsoft tenant requires `enterprise` claim. Only repos in `dotnet`, `microsoft`, etc. GitHub Enterprise orgs work. |
| `TF215106: Access denied. <name> needs Queue builds permissions` | Stakeholder access level or missing permission | Upgrade identity to **Basic** access (not Stakeholder). Verify "Queue builds" is explicitly allowed on the pipeline. |
| `TF401444: Sign-in required` | Identity not added to AzDO org | Add the MI as a user in the AzDO Organization Settings → Users. |
| `403` from AzDO REST API | Missing permissions | Ensure the identity has "Queue builds" on the specific pipeline AND Basic access level. |
| `OIDC environment variables not available` | Missing `id-token: write` permission | Add `permissions: { id-token: write }` at the **job** level (not workflow level). |
| `Failed to get Azure AD token` | Wrong client_id/tenant_id or federated credential mismatch | Verify secrets match the MI's Client ID and Tenant ID. Check federated credential subject matches the actual OIDC claim. |

## Lessons Learned

1. **`azure/login` Action is blocked** in the `dotnet` GitHub org — use manual
   `curl`-based OIDC token exchange instead.
2. **Federated credential subjects are case-sensitive** — `JanKrivanek` ≠
   `jankrivanek`. Always verify exact GitHub username/org casing.
3. **Microsoft tenant requires GitHub Enterprise membership** — personal forks
   fail with `AADSTS7002381`. Only repos in enterprise-managed orgs work.
4. **Stakeholder access is insufficient** — even with explicit "Queue builds"
   permissions, Stakeholder-level identities get `TF215106`. Request Basic.
5. **Add identity to EACH AzDO org separately** — permissions in `dnceng-public`
   don't carry over to `DevDiv` and vice versa.

## References

- [Use service principals and managed identities in Azure DevOps](https://learn.microsoft.com/en-us/azure/devops/integrate/get-started/authentication/service-principal-managed-identity)
- [AzDO Pipelines REST API — Run Pipeline](https://learn.microsoft.com/en-us/rest/api/azure/devops/pipelines/runs/run-pipeline?view=azure-devops-rest-7.1)
- [GitHub OIDC token docs](https://docs.github.com/en/actions/deployment/security-hardening-your-deployments/about-security-hardening-with-openid-connect)
