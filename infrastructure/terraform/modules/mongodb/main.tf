# =============================================================================
# MongoDB Module - CriptoJackpot DigitalOcean Infrastructure
# DO Managed MongoDB — usado por el servicio de Auditoría
# =============================================================================

resource "digitalocean_database_cluster" "mongodb" {
  name       = var.name
  engine     = "mongodb"
  version    = var.version_mongodb
  size       = var.size
  region     = var.region
  node_count = var.node_count

  private_network_uuid = var.vpc_uuid

  tags = var.tags


  lifecycle {
    prevent_destroy = false
  }
}

# Firewall — solo el cluster K8s puede conectarse
resource "digitalocean_database_firewall" "mongodb" {
  cluster_id = digitalocean_database_cluster.mongodb.id

  dynamic "rule" {
    for_each = var.trusted_sources_ids
    content {
      type  = "k8s"
      value = rule.value
    }
  }
}

# Base de datos para auditoría
resource "digitalocean_database_db" "audit" {
  cluster_id = digitalocean_database_cluster.mongodb.id
  name       = var.audit_database
}

