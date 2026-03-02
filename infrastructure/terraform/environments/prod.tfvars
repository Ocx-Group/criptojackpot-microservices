# =============================================================================
# Production Environment Variables - CriptoJackpot
# =============================================================================
# Uso:
#   terraform init -backend-config="key=prod/terraform.tfstate"
#   terraform apply -var-file="environments/prod.tfvars"
#
# Secrets sensibles via variables de entorno:
#   $env:TF_VAR_do_token              = "dop_v1_..."
#   $env:TF_VAR_spaces_access_key     = "..."
#   $env:TF_VAR_spaces_secret_key     = "..."
#   $env:TF_VAR_cloudflare_api_token  = "..."
#   $env:TF_VAR_cloudflare_zone_id    = "..."
#   $env:TF_VAR_brevo_api_key         = "..."
#   (Kafka: Redpanda interno — sin TF_VAR)
#   (Redis: DO Managed — sin TF_VAR)
#   (MongoDB: DO Managed — sin TF_VAR)
# =============================================================================

# Project
project_name = "criptojackpot"
environment  = "prod"
region       = "nyc3"

# VPC
vpc_ip_range = "10.10.0.0/16"

# Kubernetes - Configuración robusta para producción
k8s_version    = "1.32.10-do.5"
k8s_node_size  = "s-4vcpu-8gb"
k8s_node_count = 3
k8s_auto_scale = true
k8s_min_nodes  = 3
k8s_max_nodes  = 10

# Database - HA para producción (replicación + failover automático)
db_size       = "db-s-2vcpu-4gb"
db_node_count = 2
db_version    = "16"

# Registry
registry_subscription_tier = "professional"

# Spaces
spaces_bucket_name   = "criptojackpot-prod-assets"
spaces_acl           = "private"
spaces_force_destroy = false  # CRÍTICO: nunca true en prod

# Domain
domain = "api.criptojackpot.com"

# Cloudflare (TLS terminado en CF, no se usa cert-manager)
enable_cloudflare_dns = true
cloudflare_proxied    = true  # Nube naranja: CDN + WAF activo

# JWT
jwt_issuer   = "CriptoJackpotIdentity"
jwt_audience = "CriptoJackpotApp"

# Redis - DO Managed (misma VPC, gestionado por DigitalOcean)
redis_size = "db-s-1vcpu-1gb"

# MongoDB - DO Managed (mismo VPC, reemplaza Atlas)
mongodb_size           = "db-s-1vcpu-1gb"
mongodb_audit_database = "criptojackpot_audit"

# Kafka - Redpanda interno (pod en el cluster, sin credenciales externas)

# Tags
tags = ["criptojackpot", "prod", "terraform-managed", "critical"]

