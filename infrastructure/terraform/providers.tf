# =============================================================================
# Terraform Providers - CryptoJackpot DigitalOcean Infrastructure
# =============================================================================

provider "digitalocean" {
  token             = var.do_token
  spaces_access_id  = var.spaces_access_key
  spaces_secret_key = var.spaces_secret_key
}

# Provider Kubernetes — usa kubeconfig local (guardado con doctl kubeconfig save)
provider "kubernetes" {
  config_path    = "~/.kube/config"
  config_context = "do-nyc3-${var.project_name}-${var.environment}-cluster"
}

# Provider Helm para instalar cert-manager y nginx-ingress
provider "helm" {
  kubernetes {
    config_path    = "~/.kube/config"
    config_context = "do-nyc3-${var.project_name}-${var.environment}-cluster"
  }
}

# Provider Cloudflare para automatización DNS
provider "cloudflare" {
  api_token = var.cloudflare_api_token
}

