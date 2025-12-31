# =============================================================================
tags = ["cryptojackpot", "dev", "terraform-managed"]
# Tags

domain = "dev-api.cryptojackpot.com"
# Domain

letsencrypt_email = "dev@cryptojackpot.com"
enable_ssl        = true
# SSL - Usar staging de Let's Encrypt para evitar rate limits

spaces_acl         = "private"
spaces_bucket_name = "cryptojackpot-dev-assets"
# Spaces

registry_subscription_tier = "starter"
# Registry

db_version    = "16"
db_node_count = 1
db_size       = "db-s-1vcpu-1gb"
# Database - Standalone para desarrollo

k8s_max_nodes  = 3
k8s_min_nodes  = 1
k8s_auto_scale = false
k8s_node_count = 2
k8s_node_size  = "s-2vcpu-2gb"
k8s_version    = "1.29.1-do.0"
# Kubernetes - Configuración mínima para desarrollo

vpc_ip_range = "10.20.0.0/16"
# VPC

region       = "nyc3"
environment  = "dev"
project_name = "cryptojackpot"
# Project

# =============================================================================
# Development Environment Variables

