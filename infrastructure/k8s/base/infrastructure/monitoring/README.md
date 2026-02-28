# Observabilidad — Grafana Cloud Free + OTel Collector

## Arquitectura

```
Microservicios .NET 8 (namespace: cryptojackpot)
    │  OTLP gRPC :4317
    ▼
OTel Collector  (namespace: monitoring, in-cluster)
    │
    ├──► Grafana Cloud Tempo      → Trazas distribuidas
    ├──► Grafana Cloud Prometheus → Métricas (.NET runtime, HTTP, DB, MassTransit)
    └──► Grafana Cloud Loki       → Logs (via Promtail DaemonSet)
```

## 1. Crear cuenta Grafana Cloud (Free)

1. Ir a https://grafana.com/auth/sign-up
2. Crear un stack (ej: `cryptojackpot`)
3. Región recomendada: **US East** (menor latencia desde DigitalOcean nyc3)

## 2. Obtener credenciales

En tu stack de Grafana Cloud → **"Send metrics"** (o "Details"):

| Variable | Dónde encontrarla | Ejemplo |
|---|---|---|
| `GRAFANA_INSTANCE_ID` | My Account → Stack Details → "User" | `123456` |
| `GRAFANA_API_KEY` | My Account → API Keys → "Add API key" (rol: MetricsPublisher) | `glc_eyJ...` |
| `GRAFANA_TEMPO_ENDPOINT` | Stack → Tempo → "Send traces" → URL | `https://tempo-prod-04-prod-us-east-0.grafana.net:443` |
| `GRAFANA_PROMETHEUS_ENDPOINT` | Stack → Prometheus → "Remote write" → URL | `https://prometheus-prod-24-prod-us-east-0.grafana.net/api/prom/push` |
| `GRAFANA_LOKI_ENDPOINT` | Stack → Loki → "Send logs" → URL (sin path) | `https://logs-prod-006.grafana.net` |

> **Tip**: Crear un único API Key con rol **"MetricsPublisher"** sirve para los 3 servicios (Tempo, Prometheus, Loki).

## 3. Aplicar el Secret (una sola vez por entorno)

### Local / Minikube
```powershell
kubectl create secret generic grafana-cloud-secrets `
  --namespace monitoring `
  --from-literal=GRAFANA_INSTANCE_ID="<tu-instance-id>" `
  --from-literal=GRAFANA_API_KEY="<tu-api-key>" `
  --from-literal=GRAFANA_TEMPO_ENDPOINT="https://<tu-tempo>.grafana.net:443" `
  --from-literal=GRAFANA_PROMETHEUS_ENDPOINT="https://<tu-prometheus>.grafana.net/api/prom/push" `
  --from-literal=GRAFANA_LOKI_ENDPOINT="https://<tu-loki>.grafana.net" `
  --dry-run=client -o yaml | kubectl apply -f -
```

### CI/CD (GitHub Actions) — QA y Prod
Añadir estos secrets en **Settings → Secrets → Actions** del repo:
```
GRAFANA_INSTANCE_ID
GRAFANA_API_KEY
GRAFANA_TEMPO_ENDPOINT
GRAFANA_PROMETHEUS_ENDPOINT
GRAFANA_LOKI_ENDPOINT
```

Y en el workflow de deploy:
```yaml
- name: Apply Grafana Cloud Secret
  run: |
    kubectl create secret generic grafana-cloud-secrets \
      --namespace monitoring \
      --from-literal=GRAFANA_INSTANCE_ID="${{ secrets.GRAFANA_INSTANCE_ID }}" \
      --from-literal=GRAFANA_API_KEY="${{ secrets.GRAFANA_API_KEY }}" \
      --from-literal=GRAFANA_TEMPO_ENDPOINT="${{ secrets.GRAFANA_TEMPO_ENDPOINT }}" \
      --from-literal=GRAFANA_PROMETHEUS_ENDPOINT="${{ secrets.GRAFANA_PROMETHEUS_ENDPOINT }}" \
      --from-literal=GRAFANA_LOKI_ENDPOINT="${{ secrets.GRAFANA_LOKI_ENDPOINT }}" \
      --dry-run=client -o yaml | kubectl apply -f -
```

## 4. Deploy del stack de monitoring

### Local
```powershell
# Aplicar el overlay local completo (incluye monitoring)
kubectl apply -k infrastructure/k8s/overlays/local

# Verificar que el OTel Collector arrancó
kubectl get pods -n monitoring
kubectl logs -n monitoring deployment/otel-collector

# Verificar que Promtail está corriendo en todos los nodos
kubectl get pods -n monitoring -l app=promtail
```

### QA / Prod
```powershell
# QA
kubectl apply -k infrastructure/k8s/overlays/qa

# Prod
kubectl apply -k infrastructure/k8s/overlays/prod
```

## 5. Verificar que los datos llegan a Grafana Cloud

1. **Trazas**: Grafana Cloud → Explore → Data source: **Tempo** → buscar `service.name = cryptojackpot-identity`
2. **Métricas**: Grafana Cloud → Explore → Data source: **Prometheus** → query: `{job="cryptojackpot-identity"}`
3. **Logs**: Grafana Cloud → Explore → Data source: **Loki** → query: `{namespace="cryptojackpot"}`

## 6. Dashboards recomendados (importar en Grafana)

| Dashboard | ID | Para qué |
|---|---|---|
| ASP.NET Core | `10915` | HTTP requests, errors, latencia |
| .NET Runtime | `13978` | GC, threads, memoria |
| Kubernetes Pods | `6417` | CPU/memoria por pod |
| Loki Logs | `13639` | Vista unificada de logs |

En Grafana Cloud → Dashboards → Import → pegar el ID.

## 7. Límites del Free Tier

| Servicio | Límite Free | Cuándo lo alcanzarás |
|---|---|---|
| Prometheus | 10.000 series activas | Con 8 microservicios ~2.000 series — OK |
| Tempo | 50 GB/mes trazas | Con tráfico moderado — OK |
| Loki | 50 GB/mes logs | Depende del volumen de logs — monitorear |
| Grafana | 3 usuarios activos | OK para equipo pequeño |

Cuando superes los límites → migrar a stack in-cluster (~$7/mes extra).

## Estructura de archivos creados

```
infrastructure/k8s/
├── base/infrastructure/monitoring/
│   ├── namespace.yaml                    ← namespace "monitoring"
│   ├── kustomization.yaml
│   ├── grafana-cloud-secret.template.yaml ← template (sin valores reales)
│   ├── otel-collector/
│   │   ├── configmap.yaml               ← pipelines: traces→Tempo, metrics→Prometheus, logs→Loki
│   │   ├── deployment.yaml              ← otel/opentelemetry-collector-contrib:0.114.0
│   │   ├── service.yaml                 ← ClusterIP :4317 (gRPC) + :4318 (HTTP)
│   │   ├── rbac.yaml                    ← ServiceAccount + ClusterRole para leer pods k8s
│   │   └── kustomization.yaml
│   └── promtail/
│       ├── daemonset.yaml               ← DaemonSet + ConfigMap + RBAC
│       └── kustomization.yaml
└── overlays/
    ├── local/secrets/grafana-cloud-secrets.yaml   ← completar con valores reales
    ├── qa/secrets/grafana-cloud-secrets.yaml      ← completar o inyectar via CI/CD
    └── prod/secrets/grafana-cloud-secrets.yaml    ← inyectar SIEMPRE via CI/CD
```

