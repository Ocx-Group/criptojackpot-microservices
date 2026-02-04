# Kubernetes Configuration with Kustomize

This directory contains Kubernetes configurations using the **Base and Overlays** pattern with Kustomize.

## 📁 Structure

```
infrastructure/k8s/
├── base/                           # Common configurations (Templates)
│   ├── namespace.yaml
│   ├── configmap.yaml
│   ├── kustomization.yaml
│   ├── microservices/              # Base microservice definitions
│   │   ├── identity/
│   │   ├── lottery/
│   │   ├── order/
│   │   ├── wallet/
│   │   ├── winner/
│   │   ├── notification/
│   │   └── audit/
│   └── infrastructure/             # Base infrastructure (for local)
│       ├── postgres/
│       ├── redis/
│       ├── mongodb/
│       ├── redpanda/
│       └── minio/
│
├── overlays/                       # Environment-specific customizations
│   ├── local/                      # Local development
│   │   ├── kustomization.yaml
│   │   ├── patches/
│   │   ├── secrets/
│   │   ├── configmaps/
│   │   └── ingress/
│   ├── qa/                         # QA/Staging environment
│   │   ├── kustomization.yaml
│   │   ├── patches/
│   │   ├── secrets/
│   │   └── ingress/
│   └── prod/                       # Production environment
│       ├── kustomization.yaml
│       ├── patches/
│       ├── secrets/
│       └── ingress/
│
└── local/                          # [DEPRECATED] Legacy configurations
└── prod/                           # [DEPRECATED] Legacy configurations
```

## 🚀 Usage

### With Skaffold (Recommended)

```bash
# Local development with hot-reload
skaffold dev

# Debug with remote debugging
skaffold debug

# Deploy to QA
skaffold run -p qa

# Deploy to Production
skaffold run -p prod
```

### With kubectl directly

```bash
# Preview what will be applied
kubectl kustomize infrastructure/k8s/overlays/local

# Apply local configuration
kubectl apply -k infrastructure/k8s/overlays/local

# Apply QA configuration
kubectl apply -k infrastructure/k8s/overlays/qa

# Apply Production configuration
kubectl apply -k infrastructure/k8s/overlays/prod
```

## 🔧 Environment Differences

| Feature | Local | QA | Production |
|---------|-------|-----|------------|
| Replicas | 1 | 2 | 3 |
| Resources | Low | Medium | High |
| Database | Local PostgreSQL | DO Managed | DO Managed |
| Kafka | Local Redpanda | Upstash | Upstash |
| MongoDB | Local MongoDB | Atlas | Atlas |
| Storage | Local MinIO | DO Spaces | DO Spaces |
| TLS | None | Let's Encrypt | Let's Encrypt |
| Security Context | None | Enabled | Enabled |
| InitContainers | Yes | No | No |

## 📝 How to Add a New Environment

1. Create a new folder under `overlays/`:
   ```bash
   mkdir -p infrastructure/k8s/overlays/staging
   ```

2. Create `kustomization.yaml`:
   ```yaml
   apiVersion: kustomize.config.k8s.io/v1beta1
   kind: Kustomization
   
   namespace: cryptojackpot
   
   resources:
     - ../../base
     - secrets/
     - ingress/
   
   patches:
     - path: patches/deployments-replicas.yaml
       target:
         kind: Deployment
         labelSelector: "tier=api"
   
   commonLabels:
     environment: staging
   ```

3. Add environment-specific secrets and patches.

4. Add a new profile to `skaffold.yaml`:
   ```yaml
   - name: staging
     manifests:
       kustomize:
         paths:
           - infrastructure/k8s/overlays/staging
   ```

## 🔐 Secrets Management

### Local Development
Secrets are stored in plain YAML files (OK for local dev only).

### QA/Production
Use one of these approaches:
- **Sealed Secrets**: Encrypt secrets that can be stored in Git
- **External Secrets Operator**: Sync from AWS Secrets Manager, Vault, etc.
- **Manual creation**: `kubectl create secret` (not recommended for GitOps)

## 📚 Kustomize Features Used

- **Resources**: Include base configurations
- **Patches**: Modify specific fields per environment
- **ConfigMapGenerator**: Generate ConfigMaps with environment-specific values
- **Images**: Override image names and tags
- **CommonLabels**: Add labels to all resources
- **Namespace**: Set namespace for all resources

## 🔄 Migration from Legacy Structure

The old `local/` and `prod/` folders are deprecated. To migrate:

1. The new structure is already in place under `base/` and `overlays/`
2. Update any CI/CD pipelines to use `skaffold run -p <env>`
3. After testing, remove the legacy folders

## 🐛 Troubleshooting

### Preview rendered manifests
```bash
kubectl kustomize infrastructure/k8s/overlays/local
```

### Dry-run deployment
```bash
kubectl apply -k infrastructure/k8s/overlays/local --dry-run=client
```

### Check for errors
```bash
kubectl kustomize infrastructure/k8s/overlays/local 2>&1 | head -50
```
