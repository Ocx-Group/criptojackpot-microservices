# =============================================================================
# DOKS (Kubernetes) Module - CryptoJackpot DigitalOcean Infrastructure
# =============================================================================

resource "digitalocean_kubernetes_cluster" "main" {
  name    = var.name
  region  = var.region
  version = var.version_k8s
  vpc_uuid = var.vpc_uuid

  # High Availability para producción
  ha = var.ha_enabled

  # Auto-upgrade para parches de seguridad
  auto_upgrade = var.auto_upgrade

  # Maintenance window
  maintenance_policy {
    start_time = var.maintenance_start_time
    day        = var.maintenance_day
  }

  # Node Pool principal
  node_pool {
    name       = var.node_pool_name
    size       = var.node_size
    node_count = var.auto_scale ? null : var.node_count
    auto_scale = var.auto_scale
    min_nodes  = var.auto_scale ? var.min_nodes : null
    max_nodes  = var.auto_scale ? var.max_nodes : null
    
    labels = {
      service  = "criptojackpot"
      pool     = "workers"
    }

    tags = var.tags

    # Taint opcional para workloads específicos
    # taint {
    #   key    = "workload"
    #   value  = "api"
    #   effect = "NoSchedule"
    # }
  }

  tags = var.tags

  lifecycle {
    # CRÍTICO: nunca destruir el cluster K8s automáticamente.
    # Un cambio en name/region/version_k8s forzaría recreación — hacerlo manualmente.
    # Para actualizar la versión de K8s usa auto_upgrade=true o doctl directamente.
    prevent_destroy = true

    # Ignorar cambios que DO gestiona o que pueden variar sin requerir recreación
    ignore_changes = [
      # La versión exacta puede cambiar con auto_upgrade
      version,
      # Tags pueden cambiar externamente sin problema
      tags,
      # El node_count es gestionado por auto_scale
      node_pool[0].node_count,
      # Los tags del node_pool también
      node_pool[0].tags,
    ]
  }
}

# Obtener credenciales del cluster
data "digitalocean_kubernetes_cluster" "main" {
  name = digitalocean_kubernetes_cluster.main.name
  
  depends_on = [digitalocean_kubernetes_cluster.main]
}
