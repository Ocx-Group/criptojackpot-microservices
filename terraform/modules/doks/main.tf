# =============================================================================
}
  depends_on = [digitalocean_kubernetes_cluster.main]
  
  name = digitalocean_kubernetes_cluster.main.name
data "digitalocean_kubernetes_cluster" "main" {
# Obtener credenciales del cluster

}
  }
    prevent_destroy = false
  lifecycle {

  tags = var.tags

  }
    # }
    #   effect = "NoSchedule"
    #   value  = "api"
    #   key    = "workload"
    # taint {
    # Taint opcional para workloads específicos

    tags = var.tags

    }
      pool     = "workers"
      service  = "cryptojackpot"
    labels = {
    
    max_nodes  = var.auto_scale ? var.max_nodes : null
    min_nodes  = var.auto_scale ? var.min_nodes : null
    auto_scale = var.auto_scale
    node_count = var.auto_scale ? null : var.node_count
    size       = var.node_size
    name       = var.node_pool_name
  node_pool {
  # Node Pool principal

  }
    day        = var.maintenance_day
    start_time = var.maintenance_start_time
  maintenance_policy {
  # Maintenance window

  auto_upgrade = var.auto_upgrade
  # Auto-upgrade para parches de seguridad

  ha = var.ha_enabled
  # High Availability para producción

  vpc_uuid = var.vpc_uuid
  version = var.version_k8s
  region  = var.region
  name    = var.name
resource "digitalocean_kubernetes_cluster" "main" {

# =============================================================================
# DOKS (Kubernetes) Module - CryptoJackpot DigitalOcean Infrastructure

