# =============================================================================
}
EOT
  REDPANDA_ADMIN_PASSWORD: "${var.redpanda_admin_password}"
  REDPANDA_ADMIN_USERNAME: "admin"
  REDPANDA_SASL_PASSWORD: "${var.kafka_app_password}"
  REDPANDA_SASL_USERNAME: "${var.kafka_app_username}"
stringData:
type: Opaque
  namespace: ${var.namespace}
  name: redpanda-credentials
metadata:
kind: Secret
apiVersion: v1
---
  KAFKA_SECURITY_PROTOCOL: "SASL_PLAINTEXT"
  KAFKA_SASL_MECHANISM: "SCRAM-SHA-256"
  KAFKA_SASL_PASSWORD: "${var.kafka_app_password}"
  KAFKA_SASL_USERNAME: "${var.kafka_app_username}"
  KAFKA_BOOTSTRAP_SERVERS: "${var.kafka_bootstrap_servers}"
stringData:
type: Opaque
  namespace: ${var.namespace}
  name: kafka-secrets
metadata:
kind: Secret
apiVersion: v1
---
  SPACES_SECRET_KEY: "${var.spaces_secret_key}"
  SPACES_ACCESS_KEY: "${var.spaces_access_key}"
  SPACES_BUCKET: "${var.spaces_bucket}"
  SPACES_REGION: "${var.spaces_region}"
  SPACES_ENDPOINT: "${var.spaces_endpoint}"
stringData:
type: Opaque
  namespace: ${var.namespace}
  name: digitalocean-spaces-secrets
metadata:
kind: Secret
apiVersion: v1
---
  JWT_AUDIENCE: "${var.jwt_audience}"
  JWT_ISSUER: "${var.jwt_issuer}"
  JWT_SECRET_KEY: "${var.jwt_secret_key}"
stringData:
type: Opaque
  namespace: ${var.namespace}
  name: jwt-secrets
metadata:
kind: Secret
apiVersion: v1
---
  NOTIFICATION_DB_CONNECTION: "Host=${var.postgres_host};Port=${var.postgres_port};Database=cryptojackpot_notification_db;Username=${var.postgres_user};Password=${var.postgres_password};SSL Mode=Require;Trust Server Certificate=true"
  WINNER_DB_CONNECTION: "Host=${var.postgres_host};Port=${var.postgres_port};Database=cryptojackpot_winner_db;Username=${var.postgres_user};Password=${var.postgres_password};SSL Mode=Require;Trust Server Certificate=true"
  WALLET_DB_CONNECTION: "Host=${var.postgres_host};Port=${var.postgres_port};Database=cryptojackpot_wallet_db;Username=${var.postgres_user};Password=${var.postgres_password};SSL Mode=Require;Trust Server Certificate=true"
  ORDER_DB_CONNECTION: "Host=${var.postgres_host};Port=${var.postgres_port};Database=cryptojackpot_order_db;Username=${var.postgres_user};Password=${var.postgres_password};SSL Mode=Require;Trust Server Certificate=true"
  LOTTERY_DB_CONNECTION: "Host=${var.postgres_host};Port=${var.postgres_port};Database=cryptojackpot_lottery_db;Username=${var.postgres_user};Password=${var.postgres_password};SSL Mode=Require;Trust Server Certificate=true"
  IDENTITY_DB_CONNECTION: "Host=${var.postgres_host};Port=${var.postgres_port};Database=cryptojackpot_identity_db;Username=${var.postgres_user};Password=${var.postgres_password};SSL Mode=Require;Trust Server Certificate=true"
  POSTGRES_SSLMODE: "require"
  POSTGRES_PASSWORD: "${var.postgres_password}"
  POSTGRES_USER: "${var.postgres_user}"
  POSTGRES_PORT: "${var.postgres_port}"
  POSTGRES_HOST: "${var.postgres_host}"
stringData:
type: Opaque
  namespace: ${var.namespace}
  name: postgres-secrets
metadata:
kind: Secret
apiVersion: v1
# =============================================================================
# Generado: ${timestamp()}
# NO EDITAR MANUALMENTE - Los cambios se perderán
# ARCHIVO GENERADO AUTOMÁTICAMENTE POR TERRAFORM
# =============================================================================
  secrets_yaml_content = <<-EOT
locals {
# Generar el archivo secrets.yaml para referencia/backup

}
  type = "Opaque"

  }
    REDPANDA_ADMIN_PASSWORD = var.redpanda_admin_password
    REDPANDA_ADMIN_USERNAME = "admin"
    REDPANDA_SASL_PASSWORD  = var.kafka_app_password
    REDPANDA_SASL_USERNAME  = var.kafka_app_username
  data = {

  }
    namespace = kubernetes_namespace.main.metadata[0].name
    name      = "redpanda-credentials"
  metadata {
resource "kubernetes_secret" "redpanda_credentials" {
# Secret para Redpanda credentials (admin y app)

}
  type = "Opaque"

  }
    KAFKA_SECURITY_PROTOCOL = "SASL_PLAINTEXT"
    KAFKA_SASL_MECHANISM    = "SCRAM-SHA-256"
    KAFKA_SASL_PASSWORD     = var.kafka_app_password
    KAFKA_SASL_USERNAME     = var.kafka_app_username
    KAFKA_BOOTSTRAP_SERVERS = var.kafka_bootstrap_servers
  data = {

  }
    namespace = kubernetes_namespace.main.metadata[0].name
    name      = "kafka-secrets"
  metadata {
resource "kubernetes_secret" "kafka" {
# Secret para Kafka/Redpanda

}
  type = "Opaque"

  }
    SPACES_SECRET_KEY = var.spaces_secret_key
    SPACES_ACCESS_KEY = var.spaces_access_key
    SPACES_BUCKET     = var.spaces_bucket
    SPACES_REGION     = var.spaces_region
    SPACES_ENDPOINT   = var.spaces_endpoint
  data = {

  }
    namespace = kubernetes_namespace.main.metadata[0].name
    name      = "digitalocean-spaces-secrets"
  metadata {
resource "kubernetes_secret" "spaces" {
# Secret para DigitalOcean Spaces

}
  type = "Opaque"

  }
    JWT_AUDIENCE   = var.jwt_audience
    JWT_ISSUER     = var.jwt_issuer
    JWT_SECRET_KEY = var.jwt_secret_key
  data = {

  }
    namespace = kubernetes_namespace.main.metadata[0].name
    name      = "jwt-secrets"
  metadata {
resource "kubernetes_secret" "jwt" {
# Secret para JWT

}
  type = "Opaque"

  }
    NOTIFICATION_DB_CONNECTION = "Host=${var.postgres_host};Port=${var.postgres_port};Database=cryptojackpot_notification_db;Username=${var.postgres_user};Password=${var.postgres_password};SSL Mode=Require;Trust Server Certificate=true"
    WINNER_DB_CONNECTION       = "Host=${var.postgres_host};Port=${var.postgres_port};Database=cryptojackpot_winner_db;Username=${var.postgres_user};Password=${var.postgres_password};SSL Mode=Require;Trust Server Certificate=true"
    WALLET_DB_CONNECTION       = "Host=${var.postgres_host};Port=${var.postgres_port};Database=cryptojackpot_wallet_db;Username=${var.postgres_user};Password=${var.postgres_password};SSL Mode=Require;Trust Server Certificate=true"
    ORDER_DB_CONNECTION        = "Host=${var.postgres_host};Port=${var.postgres_port};Database=cryptojackpot_order_db;Username=${var.postgres_user};Password=${var.postgres_password};SSL Mode=Require;Trust Server Certificate=true"
    LOTTERY_DB_CONNECTION      = "Host=${var.postgres_host};Port=${var.postgres_port};Database=cryptojackpot_lottery_db;Username=${var.postgres_user};Password=${var.postgres_password};SSL Mode=Require;Trust Server Certificate=true"
    IDENTITY_DB_CONNECTION     = "Host=${var.postgres_host};Port=${var.postgres_port};Database=cryptojackpot_identity_db;Username=${var.postgres_user};Password=${var.postgres_password};SSL Mode=Require;Trust Server Certificate=true"
    # Connection strings para cada microservicio

    POSTGRES_SSLMODE  = "require"
    POSTGRES_PASSWORD = var.postgres_password
    POSTGRES_USER     = var.postgres_user
    POSTGRES_PORT     = tostring(var.postgres_port)
    POSTGRES_HOST     = var.postgres_host
  data = {

  }
    namespace = kubernetes_namespace.main.metadata[0].name
    name      = "postgres-secrets"
  metadata {
resource "kubernetes_secret" "postgres" {
# Secret para PostgreSQL

}
  }
    }
      name = var.namespace
    labels = {
    name = var.namespace
  metadata {
resource "kubernetes_namespace" "main" {
# Crear el namespace si no existe

# =============================================================================
# Genera automáticamente los secrets de Kubernetes con valores reales
# Kubernetes Secrets Module - CryptoJackpot DigitalOcean Infrastructure

