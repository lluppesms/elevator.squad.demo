# GitHub Actions Workflows

This repository uses GitHub Actions to automate the deployment of the Elevator Dispatch application to Azure. This guide explains the available workflows, how to set up secrets, and the deployment sequence.

---

## Workflow Overview

### Template Workflows (Reusable)

These are reusable templates called by the concrete workflows below. You typically won't run these directly.

- **`template-load-config.yml`** — Loads project configuration from `.github/config/projects.yml` and outputs build/test directories.
- **`template-bicep-deploy.yml`** — Deploys Azure infrastructure (resource group, App Service, monitoring) using Bicep templates.
- **`template-webapp-build.yml`** — Builds the ElevatorApi web application, runs tests, and generates code coverage reports.
- **`template-webapp-deploy.yml`** — Deploys the built web app to Azure App Service.

### Concrete Workflows (Run These)

These are the workflows you trigger manually via GitHub Actions dispatch.

- **`1-deploy-bicep.yml`** — Deploys infrastructure only. Use this to set up or update Azure resources without redeploying the application.
- **`2.1-bicep-build-deploy-webapp.yml`** — Full deployment pipeline: deploys infrastructure, builds the web app, and deploys it to Azure App Service. This is the primary workflow for end-to-end deployments.

---

## Deployment Sequence

For a fresh deployment or full update, use the **2.1** workflow, which runs these jobs in order:

1. **Load Config** — Extracts project paths from `.github/config/projects.yml`.
2. **Create Infra** — Deploys Bicep template to Azure (if enabled).
3. **Build WebApp** — Builds ElevatorApi, runs xUnit tests, generates code coverage.
4. **Deploy WebApp** — Uploads the built application to Azure App Service.
5. **Smoke Test** *(optional)* — Runs Playwright tests against the deployed app.

If you only need to update infrastructure, run the **1** workflow directly.

---

## Required Secrets

Before running any deployment workflows, you must set up authentication secrets in GitHub. These enable the workflows to authenticate to Azure using OpenID Connect (OIDC).

### Setting Up Secrets

The recommended approach is to set environment-level secrets, so different environments (dev, test, prod) can have separate credentials.

**Using GitHub CLI:**

```bash
# For a specific environment (e.g., 'dev')
gh secret set --env dev AZURE_SUBSCRIPTION_ID -b <your-subscription-id>
gh secret set --env dev AZURE_TENANT_ID -b <your-tenant-id>
gh secret set --env dev AZURE_CLIENT_ID -b <your-client-id>
```

**Using GitHub Web UI:**

1. Go to your repository → Settings → Secrets and variables → Actions
2. Click "New repository secret" or "New environment secret" (for environment-level)
3. Add each secret:
   - Name: `AZURE_SUBSCRIPTION_ID` | Value: Your Azure subscription ID
   - Name: `AZURE_TENANT_ID` | Value: Your Azure Entra (AAD) tenant ID
   - Name: `AZURE_CLIENT_ID` | Value: Your service principal client ID

### Secret Details

| Secret | Description |
|--------|-------------|
| `AZURE_SUBSCRIPTION_ID` | Your Azure subscription ID (find in Azure Portal or `az account show --query id`) |
| `AZURE_TENANT_ID` | Your Azure Entra tenant ID (find in Azure Portal → Entra ID → Overview) |
| `AZURE_CLIENT_ID` | Service principal application ID used for OIDC authentication |

**Note:** If using client secret authentication instead of OIDC, also set `AZURE_CLIENT_SECRET`. However, OIDC (federated credentials) is recommended for security.

---

## Azure Login Action

The workflows use a custom reusable action (`.github/actions/login-action`) to handle Azure authentication. This action:

1. **Detects the authentication method** — checks if a client secret is provided
2. **Authenticates to Azure** — uses OIDC (recommended) or client secret fallback
3. **Displays account info** — confirms successful login

The action is called with the three secrets and handles all Azure CLI setup automatically. Most workflows use this under the hood — you don't need to configure it separately.

---

## Environment Variables & Configuration

### Repository / Environment Variables

The workflows reference these variables (set at repository or environment level):

- `APP_NAME` — The base name for deployed resources (e.g., `elevator`)
- `INSTANCE_NUMBER` — Instance identifier appended to resource names (e.g., `01`)
- `RESOURCE_GROUP_PREFIX` — Prefix for resource group naming (e.g., `rg`)
- `RESOURCE_GROUP_LOCATION` — Azure region for deployment (e.g., `eastus`)

### Project Configuration

The `.github/config/projects.yml` file maps project names to their source directories:

```yaml
projects:
  web:
    shortName: 'web'
    rootDirectory: 'src/ElevatorApi'
    projectName: 'ElevatorApi'
    testDirectory: 'src/ElevatorTests'
    testProjectName: 'ElevatorTests'
```

This allows the build and deploy workflows to dynamically locate the correct projects.

---

## Running a Workflow

### Option 1: GitHub Web UI

1. Go to your repository → Actions
2. Select the workflow (e.g., "2.1 - Deploy Bicep/Build & Deploy Web App")
3. Click "Run workflow"
4. Fill in the input parameters:
   - **Environment** — Select `dev`, `test`, `prod`, etc.
   - **Bicep Mode** — `create` (deploy) or `whatIf` (preview)
   - **Deploy Bicep** — Check to deploy infrastructure
   - **Build Web App** — Check to build the app
   - **Deploy Web App** — Check to deploy to App Service
   - **Run Smoke Tests** — Optional, runs Playwright tests
5. Click "Run workflow"

### Option 2: GitHub CLI

```bash
gh workflow run "2.1-bicep-build-deploy-webapp.yml" \
  -f deployEnvironment=dev \
  -f bicepDeploymentMode=create \
  -f runCreateInfra=true \
  -f runBuild=true \
  -f runDeploy=true
```

---

## Bicep Deployment Modes

The Bicep deploy workflow supports three modes:

| Mode | Behavior |
|------|----------|
| `create` | Deploy and create/update resources in Azure |
| `validate` | Validate the Bicep template without deploying |
| `whatIf` | Show what would change without making changes |

Use `whatIf` to preview changes before deployment.

---

## Troubleshooting

### Authentication Failures

**Error:** `azure/login: Error: AADSTS700016`

→ Verify `AZURE_CLIENT_ID`, `AZURE_TENANT_ID`, and `AZURE_SUBSCRIPTION_ID` are correctly set for your environment.

**Error:** `Insufficient privileges to complete the operation`

→ The service principal (client ID) must have **Contributor** rights on the subscription or resource group.

### Build/Test Failures

**Error:** `Unable to find project file`

→ Check that the paths in `.github/config/projects.yml` match your actual directory structure.

**Error:** `Test failed`

→ Run `dotnet test src/ElevatorTests` locally to debug. The workflow runs the same tests.

### Deployment Timeouts

→ Check the GitHub Actions runner logs. Long builds may need `timeout-minutes` adjustment in the workflow YAML.

---

## Code Coverage

The build workflow automatically:

1. Runs xUnit tests in `src/ElevatorTests`
2. Collects code coverage using Coverlet
3. Generates a coverage report
4. Posts a coverage summary as a pull request comment (for PRs)

Coverage is measured on the backend (`ElevatorSimulation` and `ElevatorApi` domains), excluding generated code and the Blazor UI. Current baseline: **95.30% line coverage**.

---

## Security Notes

- **Never commit secrets** to the repository. Use GitHub secrets.
- **Use OIDC** (federated credentials) instead of client secrets when possible.
- **Limit permissions** — The service principal should have only the minimum required roles (Contributor on the resource group, not the entire subscription if possible).
- **Rotate credentials** regularly.

---

## References

- [Azure Credentials in GitHub Actions](https://docs.microsoft.com/en-us/azure/azure-resource-manager/templates/deploy-github-actions)
- [GitHub Secrets](https://docs.github.com/en/actions/security-guides/encrypted-secrets)
- [Azure Developer CLI (azd)](../.azure/readme.md) — Alternative deployment method
- [Bicep Documentation](https://learn.microsoft.com/en-us/azure/azure-resource-manager/bicep/)
