# =============================================================================
}
  depends_on = [digitalocean_database_db.databases]

  user       = digitalocean_database_cluster.main.user
  db_name    = each.value
  size       = var.connection_pool_size
  mode       = "transaction"
  name       = "${each.value}-pool"
  cluster_id = digitalocean_database_cluster.main.id

  for_each = var.enable_connection_pool ? toset(var.databases) : []
resource "digitalocean_database_connection_pool" "main" {
# Configuración de conexión pool (para mejor rendimiento)

}
  }
    }
      value = rule.value
      type  = "ip_addr"
    content {
    for_each = var.trusted_ips
  dynamic "rule" {
  # Opcionalmente permitir IPs específicas (para desarrollo)

  }
    }
      value = rule.value
      type  = "k8s"
    content {
    for_each = var.trusted_sources_ids
  dynamic "rule" {
  # Permitir acceso desde recursos específicos de DO (cluster K8s)

  cluster_id = digitalocean_database_cluster.main.id
resource "digitalocean_database_firewall" "main" {
# Firewall de base de datos - Solo permitir acceso desde el cluster K8s

}
  name       = var.app_user_name
  cluster_id = digitalocean_database_cluster.main.id

  count = var.create_app_user ? 1 : 0
resource "digitalocean_database_user" "app_user" {
# Usuario de aplicación (opcional - usar si se quiere separar del admin)

}
  name       = each.value
  cluster_id = digitalocean_database_cluster.main.id

  for_each = toset(var.databases)
resource "digitalocean_database_db" "databases" {
# Crear las 6 bases de datos para cada microservicio

}
  }
    prevent_destroy = false
  lifecycle {

  }
    hour = var.maintenance_hour
    day  = var.maintenance_day
  maintenance_window {
  # Configuraciones de mantenimiento

  tags = var.tags
  # Tags para identificación

  private_network_uuid = var.vpc_uuid
  # Conexión a VPC privada
  
  node_count = var.node_count
  region     = var.region
  size       = var.size
  version    = var.version_pg
  engine     = "pg"
  name       = var.name
resource "digitalocean_database_cluster" "main" {

# =============================================================================
# Database (PostgreSQL) Module - CryptoJackpot DigitalOcean Infrastructure

