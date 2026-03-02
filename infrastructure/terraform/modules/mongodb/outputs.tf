output "id" {
  description = "ID del cluster MongoDB"
  value       = digitalocean_database_cluster.mongodb.id
}

output "host" {
  description = "Host público del cluster"
  value       = digitalocean_database_cluster.mongodb.host
}

output "private_host" {
  description = "Host privado del cluster (dentro de VPC)"
  value       = digitalocean_database_cluster.mongodb.private_host
}

output "port" {
  description = "Puerto del cluster"
  value       = digitalocean_database_cluster.mongodb.port
}

output "user" {
  description = "Usuario por defecto"
  value       = digitalocean_database_cluster.mongodb.user
}

output "password" {
  description = "Contraseña del usuario por defecto"
  value       = digitalocean_database_cluster.mongodb.password
  sensitive   = true
}

output "uri" {
  description = "URI de conexión completa (mongodb+srv://...)"
  value       = digitalocean_database_cluster.mongodb.uri
  sensitive   = true
}

output "private_uri" {
  description = "URI de conexión privada (dentro de VPC)"
  value       = digitalocean_database_cluster.mongodb.private_uri
  sensitive   = true
}

output "audit_database" {
  description = "Nombre de la base de datos de auditoría"
  value       = digitalocean_database_db.audit.name
}

