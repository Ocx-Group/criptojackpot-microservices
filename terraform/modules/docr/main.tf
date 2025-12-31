# =============================================================================
}
  depends_on = [digitalocean_container_registry.main]

  }
    EOT
        --dry-run=client -o yaml | kubectl apply -f -
        --namespace=cryptojackpot \
        --docker-password=${digitalocean_container_registry_docker_credentials.main.docker_credentials} \
        --docker-username=${digitalocean_container_registry_docker_credentials.main.docker_credentials} \
        --docker-server=${digitalocean_container_registry.main.server_url} \
      kubectl create secret docker-registry do-registry \
    command = <<-EOT
  provisioner "local-exec" {

  count = var.kubernetes_cluster_id != "" ? 1 : 0
resource "null_resource" "registry_secret" {
# Crear secret de Docker Registry en Kubernetes

}
  node_count = 0
  size       = "s-1vcpu-2gb"
  name       = "registry-integration"
  cluster_id = var.kubernetes_cluster_id
  count      = var.kubernetes_cluster_id != "" ? 0 : 0  # Disabled, se hace vía DOCR integration
resource "digitalocean_kubernetes_cluster_node_pool" "registry_integration" {
# Configurar el cluster para usar el registry (si se proporciona el ID)

}
  registry_name = digitalocean_container_registry.main.name
resource "digitalocean_container_registry_docker_credentials" "main" {
# Integración automática del registry con el cluster de Kubernetes

}
  region                 = var.region
  subscription_tier_slug = var.subscription_tier
  name                   = var.name
resource "digitalocean_container_registry" "main" {

# =============================================================================
# DOCR (Container Registry) Module - CryptoJackpot DigitalOcean Infrastructure

