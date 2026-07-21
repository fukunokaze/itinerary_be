# k8s manifests

Deploys the `itinerary_be` webapi + migrator to AKS. CI/CD (`.github/workflows/ci-cd.yml`)
applies these automatically on every push to `main`, but a few cluster resources are
sensitive and are **not** stored in this repo — they must be created once by hand before
the pipeline can succeed.

## Prerequisites

- `kubectl` pointed at the target cluster:
  ```
  az aks get-credentials -g trvl_pln_aks_group -n trvl_pln_aks
  ```
- A reachable PostgreSQL instance (e.g. Azure Database for PostgreSQL Flexible Server) with
  a database created for this app.

## One-time bootstrap

Run these once per cluster. They are not managed by CI/CD on purpose — recreating or
rotating them is a deliberate, manual action.

### 1. Namespace

```
kubectl apply -f k8s/namespace.yaml
```

### 2. GHCR image pull secret

GHCR images are private, so the cluster needs credentials to pull them. Use a GitHub
Personal Access Token (classic) with the `read:packages` scope — not `GITHUB_TOKEN`,
which is only valid inside a workflow run.

```
kubectl create secret docker-registry ghcr-creds \
  --docker-server=ghcr.io \
  --docker-username=<gh-username> \
  --docker-password=<gh-PAT-with-read:packages> \
  -n itinerary
```

### 3. App secrets

`k8s/webapi-deployment.yaml` and `k8s/migrator-job.yaml` read from a Secret named
`itinerary-secrets`. It is never checked in — create it directly:

```
kubectl create secret generic itinerary-secrets -n itinerary \
  --from-literal=webapi-connection-string="Host=<pg-host>;Port=5432;Database=<db>;Username=<user>;Password=<password>;" \
  --from-literal=migrator-connection-string="Host=<pg-host>;Port=5432;Database=<db>;Username=<user>;Password=<password>;" \
  --from-literal=jwt-secret="<random 64-byte value>" \
  --from-literal=google-client-id="<google oauth client id>" \
  --from-literal=google-client-secret="<google oauth client secret>"
```

To rotate a value later, `kubectl delete secret itinerary-secrets -n itinerary` and
recreate it, then restart the deployment (`kubectl rollout restart deployment/itinerary-webapi -n itinerary`).

## What CI/CD does on every push to `main`

1. Applies `namespace.yaml`, `configmap.yaml`, `webapi-service.yaml` as-is.
2. Deletes and recreates the `itinerary-migrator` Job (Jobs are immutable — this is the only
   way to re-run it) using the image tag just built, then waits for it to complete.
3. Applies `webapi-deployment.yaml` with the same image tag substituted for `:latest`, then
   waits for the rollout to finish.

See the `deploy` job in `.github/workflows/ci-cd.yml` for the exact commands, and the Azure
OIDC federated-credential setup required for it to authenticate.

## Manual deploy (no CI)

```
kubectl apply -f k8s/namespace.yaml
kubectl apply -f k8s/configmap.yaml
kubectl apply -f k8s/webapi-service.yaml
kubectl apply -f k8s/migrator-job.yaml
kubectl apply -f k8s/webapi-deployment.yaml
```

Both manifests default to the `:latest` tag, so make sure the images you want have already
been pushed to GHCR under that tag (or edit the `image:` line before applying).
