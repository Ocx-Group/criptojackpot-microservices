# =============================================================================
# Kubernetes Secrets Module - CriptoJackpot
# Crea todos los secrets necesarios directamente en el cluster DOKS
# Los valores vienen de los servicios gestionados (DO Managed PG, Upstash, etc.)
# =============================================================================

resource "kubernetes_namespace" "main" {
  metadata {
    name = var.namespace
    labels = {
      name        = var.namespace
      environment = var.environment
    }
  }
}

# -----------------------------------------------------------------------------
# postgres-secrets
# Los microservicios conectan a PgBouncer (ClusterIP interno) que hace proxy
# al DO Managed PostgreSQL. SSL deshabilitado en la conexión interna al PgBouncer
# porque la comunicación es dentro del cluster (VPC privada de DO).
# -----------------------------------------------------------------------------
resource "kubernetes_secret" "postgres" {
  metadata {
    name      = "postgres-secrets"
    namespace = kubernetes_namespace.main.metadata[0].name
  }

  data = {
    POSTGRES_HOST     = var.postgres_host
    POSTGRES_PORT     = tostring(var.postgres_port)
    POSTGRES_USER     = var.postgres_user != null ? var.postgres_user : ""
    POSTGRES_PASSWORD = var.postgres_password != null ? var.postgres_password : ""
    POSTGRES_SSLMODE  = "require"

    IDENTITY_DB_CONNECTION     = "Host=pgbouncer.${var.namespace}.svc.cluster.local;Port=6432;Database=${var.namespace}_identity_db;Username=${var.postgres_user != null ? var.postgres_user : ""};Password=${var.postgres_password != null ? var.postgres_password : ""};SSL Mode=Disable"
    LOTTERY_DB_CONNECTION      = "Host=pgbouncer.${var.namespace}.svc.cluster.local;Port=6432;Database=${var.namespace}_lottery_db;Username=${var.postgres_user != null ? var.postgres_user : ""};Password=${var.postgres_password != null ? var.postgres_password : ""};SSL Mode=Disable"
    ORDER_DB_CONNECTION        = "Host=pgbouncer.${var.namespace}.svc.cluster.local;Port=6432;Database=${var.namespace}_order_db;Username=${var.postgres_user != null ? var.postgres_user : ""};Password=${var.postgres_password != null ? var.postgres_password : ""};SSL Mode=Disable"
    WALLET_DB_CONNECTION       = "Host=pgbouncer.${var.namespace}.svc.cluster.local;Port=6432;Database=${var.namespace}_wallet_db;Username=${var.postgres_user != null ? var.postgres_user : ""};Password=${var.postgres_password != null ? var.postgres_password : ""};SSL Mode=Disable"
    WINNER_DB_CONNECTION       = "Host=pgbouncer.${var.namespace}.svc.cluster.local;Port=6432;Database=${var.namespace}_winner_db;Username=${var.postgres_user != null ? var.postgres_user : ""};Password=${var.postgres_password != null ? var.postgres_password : ""};SSL Mode=Disable"
    NOTIFICATION_DB_CONNECTION = "Host=pgbouncer.${var.namespace}.svc.cluster.local;Port=6432;Database=${var.namespace}_notification_db;Username=${var.postgres_user != null ? var.postgres_user : ""};Password=${var.postgres_password != null ? var.postgres_password : ""};SSL Mode=Disable"
  }

  type = "Opaque"

  # Workaround: kubernetes provider bug with sensitive attributes on imported secrets
  # https://github.com/hashicorp/terraform-provider-kubernetes/issues/1420
  lifecycle {
    ignore_changes = [data]
  }
}

# -----------------------------------------------------------------------------
# jwt-secrets
# -----------------------------------------------------------------------------
resource "kubernetes_secret" "jwt" {
  metadata {
    name      = "jwt-secrets"
    namespace = kubernetes_namespace.main.metadata[0].name
  }

  data = {
    JWT_SECRET_KEY = var.jwt_secret_key
    JWT_ISSUER     = var.jwt_issuer
    JWT_AUDIENCE   = var.jwt_audience
  }

  type = "Opaque"

  lifecycle {
    ignore_changes = [data]
  }
}

# -----------------------------------------------------------------------------
# kafka-secrets — Redpanda interno (PLAINTEXT, sin SASL)
# -----------------------------------------------------------------------------
resource "kubernetes_secret" "kafka" {
  metadata {
    name      = "kafka-secrets"
    namespace = kubernetes_namespace.main.metadata[0].name
  }

  data = {
    KAFKA_BOOTSTRAP_SERVERS = "redpanda.${var.namespace}.svc.cluster.local:9092"
    KAFKA_SECURITY_PROTOCOL = "PLAINTEXT"
  }

  type = "Opaque"

  lifecycle {
    ignore_changes = [data]
  }
}

# -----------------------------------------------------------------------------
# redis-secrets — Upstash Redis (TLS)
# -----------------------------------------------------------------------------
resource "kubernetes_secret" "redis" {
  metadata {
    name      = "redis-secrets"
    namespace = kubernetes_namespace.main.metadata[0].name
  }

  data = {
    REDIS_CONNECTION_STRING = var.redis_connection_string
  }

  type = "Opaque"

  lifecycle {
    ignore_changes = [data]
  }
}

# -----------------------------------------------------------------------------
# mongodb-secrets — MongoDB Atlas
# -----------------------------------------------------------------------------
resource "kubernetes_secret" "mongodb" {
  metadata {
    name      = "mongodb-secrets"
    namespace = kubernetes_namespace.main.metadata[0].name
  }

  data = {
    MONGODB_AUDIT_CONNECTION = var.mongodb_connection_string
    MONGODB_AUDIT_DATABASE   = var.mongodb_audit_database
  }

  type = "Opaque"

  lifecycle {
    ignore_changes = [data]
  }
}

# -----------------------------------------------------------------------------
# digitalocean-spaces-secrets
# -----------------------------------------------------------------------------
resource "kubernetes_secret" "spaces" {
  metadata {
    name      = "digitalocean-spaces-secrets"
    namespace = kubernetes_namespace.main.metadata[0].name
  }

  data = {
    SPACES_ENDPOINT   = var.spaces_endpoint
    SPACES_REGION     = var.spaces_region
    SPACES_BUCKET     = var.spaces_bucket
    SPACES_ACCESS_KEY = var.spaces_access_key
    SPACES_SECRET_KEY = var.spaces_secret_key
  }

  type = "Opaque"

  lifecycle {
    ignore_changes = [data]
  }
}

# -----------------------------------------------------------------------------
# brevo-secrets — Brevo (Notification service)
# -----------------------------------------------------------------------------
resource "kubernetes_secret" "brevo" {
  metadata {
    name      = "brevo-secrets"
    namespace = kubernetes_namespace.main.metadata[0].name
  }

  data = {
    BREVO_API_KEY      = var.brevo_api_key
    BREVO_SENDER_EMAIL = var.brevo_sender_email
    BREVO_SENDER_NAME  = var.brevo_sender_name
    BREVO_BASE_URL     = var.brevo_base_url
  }

  type = "Opaque"

  lifecycle {
    ignore_changes = [data]
  }
}

# -----------------------------------------------------------------------------
# google-secrets — Google OAuth (Identity service)
# -----------------------------------------------------------------------------
resource "kubernetes_secret" "google" {
  metadata {
    name      = "google-secrets"
    namespace = kubernetes_namespace.main.metadata[0].name
  }

  data = {
    GOOGLEAUTH__CLIENTID     = var.google_client_id
    GOOGLEAUTH__CLIENTSECRET = var.google_client_secret
  }

  type = "Opaque"

  lifecycle {
    ignore_changes = [data]
  }
}

# Generar el archivo secrets.yaml para referencia/backup
# ⚠️ IMPORTANTE: Este archivo contiene credenciales sensibles
# Está incluido en .gitignore para evitar commits accidentales
locals {
  pg_user     = var.postgres_user != null ? var.postgres_user : ""
  pg_password = var.postgres_password != null ? var.postgres_password : ""

  secrets_yaml_content = <<-EOT
# =============================================================================
# ARCHIVO GENERADO AUTOMÁTICAMENTE POR TERRAFORM
# ⚠️ CONTIENE CREDENCIALES SENSIBLES - NO SUBIR A GIT
# NO EDITAR MANUALMENTE - Los cambios se perderán
# =============================================================================
apiVersion: v1
kind: Secret
metadata:
  name: postgres-secrets
  namespace: ${var.namespace}
type: Opaque
stringData:
  POSTGRES_HOST: "${var.postgres_host}"
  POSTGRES_PORT: "${var.postgres_port}"
  POSTGRES_USER: "${local.pg_user}"
  POSTGRES_PASSWORD: "${local.pg_password}"
  POSTGRES_SSLMODE: "require"
  IDENTITY_DB_CONNECTION: "Host=pgbouncer.${var.namespace}.svc.cluster.local;Port=6432;Database=${var.namespace}_identity_db;Username=${local.pg_user};Password=${local.pg_password};SSL Mode=Disable"
  LOTTERY_DB_CONNECTION: "Host=pgbouncer.${var.namespace}.svc.cluster.local;Port=6432;Database=${var.namespace}_lottery_db;Username=${local.pg_user};Password=${local.pg_password};SSL Mode=Disable"
  ORDER_DB_CONNECTION: "Host=pgbouncer.${var.namespace}.svc.cluster.local;Port=6432;Database=${var.namespace}_order_db;Username=${local.pg_user};Password=${local.pg_password};SSL Mode=Disable"
  WALLET_DB_CONNECTION: "Host=pgbouncer.${var.namespace}.svc.cluster.local;Port=6432;Database=${var.namespace}_wallet_db;Username=${local.pg_user};Password=${local.pg_password};SSL Mode=Disable"
  WINNER_DB_CONNECTION: "Host=pgbouncer.${var.namespace}.svc.cluster.local;Port=6432;Database=${var.namespace}_winner_db;Username=${local.pg_user};Password=${local.pg_password};SSL Mode=Disable"
  NOTIFICATION_DB_CONNECTION: "Host=pgbouncer.${var.namespace}.svc.cluster.local;Port=6432;Database=${var.namespace}_notification_db;Username=${local.pg_user};Password=${local.pg_password};SSL Mode=Disable"
---
apiVersion: v1
kind: Secret
metadata:
  name: jwt-secrets
  namespace: ${var.namespace}
type: Opaque
stringData:
  JWT_SECRET_KEY: "${var.jwt_secret_key}"
  JWT_ISSUER: "${var.jwt_issuer}"
  JWT_AUDIENCE: "${var.jwt_audience}"
---
apiVersion: v1
kind: Secret
metadata:
  name: kafka-secrets
  namespace: ${var.namespace}
type: Opaque
stringData:
  KAFKA_BOOTSTRAP_SERVERS: "redpanda.${var.namespace}.svc.cluster.local:9092"
  KAFKA_SECURITY_PROTOCOL: "PLAINTEXT"
---
apiVersion: v1
kind: Secret
metadata:
  name: redis-secrets
  namespace: ${var.namespace}
type: Opaque
stringData:
  REDIS_CONNECTION_STRING: "${var.redis_connection_string}"
---
apiVersion: v1
kind: Secret
metadata:
  name: mongodb-secrets
  namespace: ${var.namespace}
type: Opaque
stringData:
  MONGODB_AUDIT_CONNECTION: "${var.mongodb_connection_string}"
  MONGODB_AUDIT_DATABASE: "${var.mongodb_audit_database}"
---
apiVersion: v1
kind: Secret
metadata:
  name: digitalocean-spaces-secrets
  namespace: ${var.namespace}
type: Opaque
stringData:
  SPACES_ENDPOINT: "${var.spaces_endpoint}"
  SPACES_REGION: "${var.spaces_region}"
  SPACES_BUCKET: "${var.spaces_bucket}"
  SPACES_ACCESS_KEY: "${var.spaces_access_key}"
  SPACES_SECRET_KEY: "${var.spaces_secret_key}"
---
apiVersion: v1
kind: Secret
metadata:
  name: brevo-secrets
  namespace: ${var.namespace}
type: Opaque
stringData:
  BREVO_API_KEY: "${var.brevo_api_key}"
  BREVO_SENDER_EMAIL: "${var.brevo_sender_email}"
  BREVO_SENDER_NAME: "${var.brevo_sender_name}"
  BREVO_BASE_URL: "${var.brevo_base_url}"
---
apiVersion: v1
kind: Secret
metadata:
  name: google-secrets
  namespace: ${var.namespace}
type: Opaque
stringData:
  GOOGLEAUTH__CLIENTID: "${var.google_client_id}"
  GOOGLEAUTH__CLIENTSECRET: "${var.google_client_secret}"
EOT
}
