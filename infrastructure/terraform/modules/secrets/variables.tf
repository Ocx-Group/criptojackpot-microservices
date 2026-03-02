# =============================================================================
# Secrets Module Variables
# =============================================================================

variable "namespace" {
  description = "Namespace de Kubernetes (= project_name)"
  type        = string
  default     = "criptojackpot"
}

variable "environment" {
  description = "Ambiente (qa, prod)"
  type        = string
}

# PostgreSQL
variable "postgres_host" {
  description = "Host de PostgreSQL"
  type        = string
}

variable "postgres_port" {
  description = "Puerto de PostgreSQL"
  type        = number
  default     = 25060
}

variable "postgres_user" {
  description = "Usuario de PostgreSQL"
  type        = string
}

variable "postgres_password" {
  description = "Contraseña de PostgreSQL"
  type        = string
  sensitive   = true
}

variable "databases" {
  description = "Lista de bases de datos"
  type        = list(string)
}

# JWT
variable "jwt_secret_key" {
  description = "Clave secreta para JWT"
  type        = string
  sensitive   = true
}

variable "jwt_issuer" {
  description = "Issuer del JWT"
  type        = string
  default     = "CriptoJackpotIdentity"
}

variable "jwt_audience" {
  description = "Audience del JWT"
  type        = string
  default     = "CriptoJackpotApp"
}

# Kafka - Redpanda interno (no requiere credenciales, solo namespace para construir el host)
# El bootstrap server se construye como: redpanda.<namespace>.svc.cluster.local:9092

# Redis - Upstash
variable "redis_connection_string" {
  description = "Connection string para Redis (SignalR Backplane)"
  type        = string
  sensitive   = true
}

# MongoDB Atlas
variable "mongodb_connection_string" {
  description = "Connection string para MongoDB Atlas"
  type        = string
  sensitive   = true
}

variable "mongodb_audit_database" {
  description = "Base de datos para auditoría en MongoDB"
  type        = string
  default     = "criptojackpot_audit"
}

# DigitalOcean Spaces
variable "spaces_endpoint" {
  description = "Endpoint de DigitalOcean Spaces"
  type        = string
}

variable "spaces_region" {
  description = "Región de Spaces"
  type        = string
}

variable "spaces_bucket" {
  description = "Nombre del bucket"
  type        = string
}

variable "spaces_access_key" {
  description = "Access Key de Spaces"
  type        = string
  sensitive   = true
}

variable "spaces_secret_key" {
  description = "Secret Key de Spaces"
  type        = string
  sensitive   = true
}

# Brevo
variable "brevo_api_key" {
  description = "API Key de Brevo"
  type        = string
  sensitive   = true
}

variable "brevo_sender_email" {
  description = "Email del remitente en Brevo"
  type        = string
  default     = "noreply@criptojackpot.com"
}

variable "brevo_sender_name" {
  description = "Nombre del remitente en Brevo"
  type        = string
  default     = "CriptoJackpot"
}

variable "brevo_base_url" {
  description = "URL base de Brevo"
  type        = string
}
