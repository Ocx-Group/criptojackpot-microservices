# =============================================================================
# Redis Module - CryptoJackpot DigitalOcean Infrastructure
# Valkey (fork de Redis, 100% compatible) — reemplaza DO Managed Redis
# =============================================================================

resource "digitalocean_database_cluster" "redis" {
  name       = var.name
  engine     = "valkey"
  version    = var.version_redis
  size       = var.size
  region     = var.region
  node_count = var.node_count

  # Conexión a VPC privada
  private_network_uuid = var.vpc_uuid

  # Tags para identificación
  tags = var.tags


  # Eviction policy para cache
  eviction_policy = var.eviction_policy

  lifecycle {
    prevent_destroy = false
  }
}

# Firewall de Redis - Solo permitir acceso desde el cluster K8s
resource "digitalocean_database_firewall" "redis" {
  cluster_id = digitalocean_database_cluster.redis.id

  # Permitir acceso desde el cluster K8s
  dynamic "rule" {
    for_each = var.trusted_sources_ids
    content {
      type  = "k8s"
      value = rule.value
    }
  }

  # Opcionalmente permitir IPs específicas (para desarrollo)
  dynamic "rule" {
    for_each = var.trusted_ips
    content {
      type  = "ip_addr"
      value = rule.value
    }
  }
}
