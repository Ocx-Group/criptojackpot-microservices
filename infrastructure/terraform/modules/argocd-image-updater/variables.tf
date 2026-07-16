# =============================================================================
# ArgoCD Image Updater Module Variables
# =============================================================================

variable "chart_version" {
  description = "Version del chart argocd-image-updater"
  type        = string
  default     = "0.11.0"
}

variable "argocd_namespace" {
  description = "Namespace donde esta instalado ArgoCD"
  type        = string
  default     = "argocd"
}

variable "poll_interval" {
  description = "Intervalo de sondeo del registry (formato Go: 2m, 60s)"
  type        = string
  default     = "2m"
}

variable "log_level" {
  description = "Nivel de log (debug, info, warn, error)"
  type        = string
  default     = "info"
}

variable "docr_token" {
  description = "DigitalOcean API token para autenticacion con DOCR"
  type        = string
  sensitive   = true
}
