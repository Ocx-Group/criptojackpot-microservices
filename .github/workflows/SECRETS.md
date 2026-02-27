# GitHub Actions — Configuración de Secrets y Environments

## Repository Secrets requeridos

Ve a **Settings → Secrets and variables → Actions → New repository secret**

### Compartidos (QA y Prod)

| Secret | Descripción |
|--------|-------------|
| `DO_TOKEN` | Token de API de DigitalOcean (`dop_v1_...`) |
| `DO_SPACES_ACCESS_KEY` | Access Key de DO Spaces (también usada para el backend Terraform) |
| `DO_SPACES_SECRET_KEY` | Secret Key de DO Spaces |
| `CLOUDFLARE_API_TOKEN` | Token de Cloudflare con permiso `Zone:DNS:Edit` |
| `CLOUDFLARE_ZONE_ID` | Zone ID de `criptojackpot.com` en Cloudflare |

### QA (prefijo `QA_`)

| Secret | Descripción |
|--------|-------------|
| `QA_JWT_SECRET_KEY` | Clave JWT para QA (mín. 32 chars, dejar vacío para autogenerar) |
| `QA_KAFKA_BOOTSTRAP_SERVERS` | Upstash Kafka QA endpoint (ej: `host.upstash.io:9092`) |
| `QA_KAFKA_SASL_USERNAME` | Username Upstash Kafka QA |
| `QA_KAFKA_SASL_PASSWORD` | Password Upstash Kafka QA |
| `QA_REDIS_CONNECTION_STRING` | Upstash Redis QA (ej: `host:6379,password=...,ssl=True`) |
| `QA_MONGODB_CONNECTION_STRING` | MongoDB Atlas QA (`mongodb+srv://...`) |
| `QA_BREVO_API_KEY` | API Key de Brevo para QA |

### Production (prefijo `PROD_`)

| Secret | Descripción |
|--------|-------------|
| `PROD_JWT_SECRET_KEY` | Clave JWT para Prod (diferente a QA) |
| `PROD_KAFKA_BOOTSTRAP_SERVERS` | Upstash Kafka Prod endpoint |
| `PROD_KAFKA_SASL_USERNAME` | Username Upstash Kafka Prod |
| `PROD_KAFKA_SASL_PASSWORD` | Password Upstash Kafka Prod |
| `PROD_REDIS_CONNECTION_STRING` | Upstash Redis Prod |
| `PROD_MONGODB_CONNECTION_STRING` | MongoDB Atlas Prod |
| `PROD_BREVO_API_KEY` | API Key de Brevo para Prod |

---

## Environments

Ve a **Settings → Environments**

### Environment: `qa`
- **No requiere aprobación** (deploy automático al hacer push a `qa`)
- Opcional: agregar reviewers si quieres un gate manual

### Environment: `production`
- **Required reviewers**: agregar 1-2 personas del equipo
- **Wait timer**: 0 minutos (la aprobación manual ya es el gate)
- **Deployment branches**: solo rama `main`

Cuando se hace push a `main`, el job `terraform-deploy` queda en **pending** hasta que un reviewer apruebe en la UI de GitHub Actions.

---

## Flujo completo

```
developer → PR: feature/* → develop    [CI: build + unit tests]
                                ↓
           PR: develop → qa            [CI: build + unit + integration tests]
                                ↓
           push merge → qa             [Deploy QA automático]
                                ↓
           PR: qa → main               [CI: build + unit + integration tests]
                                ↓
           push merge → main           [Deploy Prod — requiere aprobación]
                                ↓
           reviewer aprueba            [Terraform apply + kustomize + smoke test]
                                ↓
           GitHub Release creado       [Tag automático en GitHub]
```

---

## Creación manual de un tag semver (release)

```bash
git checkout main
git pull
git tag v1.2.0
git push origin v1.2.0
```

El workflow de prod detecta el tag y lo usa como nombre de imagen y release.

