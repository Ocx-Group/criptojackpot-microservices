# Kubernetes Deployment - CriptoJackpot Distributed

## Estructura

```
k8s/
├── base/                          # Recursos comunes a todos los entornos
│   ├── namespace.yaml
│   ├── configmap.yaml
│   ├── microservices/             # Deployments base (identity, lottery, order, wallet, winner, notification, audit, bff)
│   └── infrastructure/           # Infraestructura local (postgres, redis, mongodb, redpanda, minio, pgbouncer)
│
└── overlays/
    ├── local/                     # Desarrollo local (Docker Desktop / Minikube)
    │   ├── kustomization.yaml
    │   ├── configmaps/            # Scripts de init para postgres/mongodb
    │   ├── jobs/                  # Migration job (db-migrations)
    │   ├── patches/               # imagePullPolicy, initContainers
    │   └── secrets/               # Secrets locales (valores de dev)
    │
    ├── qa/                        # Entorno QA (DigitalOcean Kubernetes)
    │   ├── kustomization.yaml
    │   ├── pgbouncer/             # Connection pooler → DO Managed Postgres
    │   └── secrets/               # Placeholders para secrets reales
    │
    └── prod/                      # Producción (DigitalOcean Kubernetes)
        ├── kustomization.yaml
        ├── pgbouncer/             # Connection pooler → DO Managed Postgres
        └── secrets/               # Placeholders — gestionar con Sealed Secrets
```

---

## Arquitectura de red

### Local
```
Browser → http://localhost → NGINX Ingress → BFF Gateway → microservicios (ClusterIP)
                                              ↓
                                   Infraestructura en cluster
                                   (postgres, redis, mongodb, redpanda, minio)
```

### QA y Producción
```
Frontend → Cloudflare (TLS terminado) → NGINX Ingress → BFF Gateway → microservicios (ClusterIP)
                                                                        ↓
                                                             Servicios gestionados externos
                                                             (DO Managed Postgres vía PgBouncer,
                                                              Upstash Kafka, Upstash Redis,
                                                              MongoDB Atlas, DO Spaces)
```

**Principios clave:**
- El **BFF Gateway** es el **único punto de entrada externo**. Todos los microservicios son `ClusterIP`.
- **Cloudflare** termina el TLS externo en QA y producción. No se usa `cert-manager` ni Let's Encrypt.
- El ingress NGINX recibe tráfico HTTP plano desde Cloudflare y lo reenvía al BFF.
- La IP real del cliente llega al BFF mediante el header `CF-Connecting-IP`.

### Dominios

| Entorno | Dominio externo (Cloudflare) | Ingress interno |
|---------|------------------------------|-----------------|
| Local   | `http://localhost`           | Sin host (bare) |
| QA      | `https://api-qa.criptojackpot.com` | `api-qa.criptojackpot.com` |
| Prod    | `https://api.criptojackpot.com`    | `api.criptojackpot.com`    |

---

## Servicios gestionados por entorno

| Servicio         | Local                          | QA / Prod                          |
|------------------|--------------------------------|------------------------------------|
| PostgreSQL        | StatefulSet en cluster         | DO Managed Postgres + PgBouncer    |
| Kafka             | Redpanda en cluster            | Upstash Kafka (SASL/SSL)           |
| Redis             | StatefulSet en cluster         | Upstash Redis (TLS)                |
| MongoDB           | StatefulSet en cluster         | MongoDB Atlas                      |
| Object Storage    | MinIO en cluster               | DigitalOcean Spaces                |
| TLS / Certificados| No                             | Cloudflare (externo al cluster)    |

---

## Comandos rápidos

### Desarrollo local (Skaffold)
```bash
# Levantar todo con hot-reload
skaffold dev --cleanup=false

# Solo un microservicio
skaffold dev -f skaffold-modules.yaml -m identity
```

### Kustomize — previsualizar manifiestos
```bash
# Ver manifiestos finales de QA
kubectl kustomize infrastructure/k8s/overlays/qa

# Aplicar QA
kubectl apply -k infrastructure/k8s/overlays/qa

# Ver manifiestos de prod (dry-run)
kubectl apply -k infrastructure/k8s/overlays/prod --dry-run=client
```

### Comandos kubectl útiles
```bash
# Ver pods
kubectl get pods -n cryptojackpot

# Logs de un servicio
kubectl logs -f deployment/bff-gateway -n cryptojackpot

# Reiniciar un deployment
kubectl rollout restart deployment/identity-api -n cryptojackpot

# Port-forward PostgreSQL (local)
kubectl port-forward svc/postgres 5433:5432 -n cryptojackpot

# Limpiar namespace completo
kubectl delete namespace cryptojackpot
```

---

## Secrets

Los secrets **nunca se commitean con valores reales** en QA/prod. Los archivos en `overlays/qa/secrets/` y `overlays/prod/secrets/` son **plantillas** con `REPLACE_WITH_*` como valores.

### Gestión recomendada en QA/Prod
- **Sealed Secrets** (recomendado) — cifrado con la clave del cluster
- **External Secrets Operator** — sincronización desde DigitalOcean Secrets / Vault
- `kubectl create secret` manual (solo para bootstrap inicial)

### Secrets requeridos por entorno

| Secret                        | Descripción                              |
|-------------------------------|------------------------------------------|
| `jwt-secrets`                 | JWT key, issuer, audience                |
| `postgres-secrets`            | Host, puerto, usuario, password, SSL, connection strings |
| `kafka-secrets`               | Bootstrap servers, SASL user/password    |
| `mongodb-secrets`             | Atlas connection string                  |
| `digitalocean-spaces-secrets` | Endpoint, bucket, access/secret key      |
| `brevo-secrets`               | API key, sender email, frontend base URL |
| `redis-secrets`               | Upstash connection string                |

---

## Configuración de Cloudflare (QA y Prod)

1. En el dashboard de Cloudflare, crear un registro DNS tipo `A` (o `CNAME`) apuntando al **Load Balancer IP** del cluster de DigitalOcean con **proxy habilitado (nube naranja)**.
2. Configurar SSL/TLS mode en **"Full"** (no Full Strict, ya que el ingress NGINX recibe HTTP plano).
3. El header `CF-Connecting-IP` se propaga automáticamente — el ingress NGINX lo usa para obtener la IP real del cliente.
4. _(Opcional)_ Configurar una **WAF Rule** en Cloudflare para bloquear tráfico que no provenga de los rangos de IPs de Cloudflare.
