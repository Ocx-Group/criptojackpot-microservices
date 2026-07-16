# =============================================================================
# Secrets Module Outputs
# =============================================================================
# Los Secrets de aplicación los gestiona ArgoCD vía SealedSecrets, no este módulo.
# Solo se expone el namespace.

output "namespace" {
  description = "Namespace donde viven los recursos de la aplicación"
  value       = kubernetes_namespace.main.metadata[0].name
}
