# =============================================================================
# Database Module Outputs
# =============================================================================

output "cluster_id" {
  description = "ID del cluster de base de datos"
  value       = digitalocean_database_cluster.main.id
}

output "cluster_urn" {
  description = "URN del cluster de base de datos"
  value       = digitalocean_database_cluster.main.urn
}

output "host" {
  description = "Host de la base de datos"
  value       = digitalocean_database_cluster.main.host
}

output "private_host" {
  description = "Host privado de la base de datos (dentro de la VPC)"
  value       = digitalocean_database_cluster.main.private_host
}

output "port" {
  description = "Puerto de la base de datos"
  value       = digitalocean_database_cluster.main.port
}

output "user" {
  description = "Usuario administrador"
  value       = digitalocean_database_cluster.main.user
}

output "password" {
  description = "Contraseña del usuario administrador"
  value       = digitalocean_database_cluster.main.password
  sensitive   = true
}

output "database" {
  description = "Nombre de la base de datos por defecto"
  value       = digitalocean_database_cluster.main.database
}

output "connection_uri" {
  description = "URI de conexión completa"
  value       = digitalocean_database_cluster.main.uri
  sensitive   = true
}

output "private_uri" {
  description = "URI de conexión privada (dentro de la VPC)"
  value       = digitalocean_database_cluster.main.private_uri
  sensitive   = true
}

output "database_names" {
  description = "Lista de nombres de bases de datos creadas"
  value       = [for db in digitalocean_database_db.databases : db.name]
}

# Connection strings formateados para .NET
locals {
  db_user     = digitalocean_database_cluster.main.user != null && digitalocean_database_cluster.main.user != "" ? digitalocean_database_cluster.main.user : "doadmin"
  db_password = digitalocean_database_cluster.main.password != null ? digitalocean_database_cluster.main.password : ""
  db_port     = tostring(digitalocean_database_cluster.main.port)
}

output "connection_strings" {
  description = "Connection strings para cada microservicio (formato .NET)"
  value = {
    for db_name in var.databases : db_name => "Host=${digitalocean_database_cluster.main.private_host};Port=${local.db_port};Database=${db_name};Username=${local.db_user};Password=${local.db_password};SSL Mode=Require;Trust Server Certificate=true"
  }
  sensitive = true
}

# Mapping de microservicio a connection string (dinámico, basado en var.databases)
output "microservice_connection_strings" {
  description = "Connection strings mapeados por nombre de base de datos"
  value = {
    for db_name in var.databases : upper(replace(db_name, "${var.app_user_name}_", "")) => "Host=${digitalocean_database_cluster.main.private_host};Port=${local.db_port};Database=${db_name};Username=${local.db_user};Password=${local.db_password};SSL Mode=Require;Trust Server Certificate=true"
  }
  sensitive = true
}
