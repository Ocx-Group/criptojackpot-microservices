# =============================================================================
# ArgoCD Image Updater Module Outputs
# =============================================================================

output "namespace" {
  description = "Namespace donde se instalo ArgoCD Image Updater"
  value       = helm_release.argocd_image_updater.namespace
}

output "chart_version" {
  description = "Version del chart instalado"
  value       = helm_release.argocd_image_updater.version
}
