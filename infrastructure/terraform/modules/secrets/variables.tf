# =============================================================================
# Secrets Module Variables
# =============================================================================
# Este módulo ya no crea Secrets (los gestiona ArgoCD vía SealedSecrets).
# Solo crea el namespace, por lo que únicamente necesita estas dos variables.

variable "namespace" {
  description = "Namespace de Kubernetes (= project_name)"
  type        = string
  default     = "criptojackpot"
}

variable "environment" {
  description = "Ambiente (qa, prod)"
  type        = string
}
