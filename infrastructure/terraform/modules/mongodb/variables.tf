variable "name" {
  description = "Nombre del cluster MongoDB"
  type        = string
}

variable "region" {
  description = "Región de DigitalOcean"
  type        = string
}

variable "size" {
  description = "Plan del cluster (db-s-1vcpu-1gb, db-s-1vcpu-2gb, etc.)"
  type        = string
  default     = "db-s-1vcpu-1gb"
}

variable "node_count" {
  description = "Número de nodos (1 = standalone, 3 = replica set)"
  type        = number
  default     = 1
}

variable "version_mongodb" {
  description = "Versión de MongoDB"
  type        = string
  default     = "8"
}

variable "vpc_uuid" {
  description = "UUID de la VPC privada"
  type        = string
}

variable "tags" {
  description = "Tags para el cluster"
  type        = list(string)
  default     = []
}

variable "trusted_sources_ids" {
  description = "IDs de clusters K8s con acceso permitido"
  type        = list(string)
  default     = []
}

variable "audit_database" {
  description = "Nombre de la base de datos de auditoría"
  type        = string
  default     = "criptojackpot_audit"
}

