# =============================================================================
# Terraform Versions - CryptoJackpot DigitalOcean Infrastructure
# =============================================================================

terraform {
  required_version = ">= 1.5.0"

  required_providers {
    digitalocean = {
      source  = "digitalocean/digitalocean"
      version = "~> 2.34"
    }
    kubernetes = {
      source  = "hashicorp/kubernetes"
      version = "~> 2.25"
    }
    helm = {
      source  = "hashicorp/helm"
      version = "~> 2.12"
    }
    random = {
      source  = "hashicorp/random"
      version = "~> 3.6"
    }
    local = {
      source  = "hashicorp/local"
      version = "~> 2.4"
    }
    null = {
      source  = "hashicorp/null"
      version = "~> 3.2"
    }
  }

  # Backend remoto en DigitalOcean Spaces (descomentar para uso en equipo/CI-CD)
  # backend "s3" {
  #   endpoint                    = "nyc3.digitaloceanspaces.com"
  #   bucket                      = "cryptojackpot-terraform-state"
  #   key                         = "terraform.tfstate"
  #   region                      = "us-east-1" # Requerido pero ignorado por DO
  #   skip_credentials_validation = true
  #   skip_metadata_api_check     = true
  #   skip_region_validation      = true
  #   skip_requesting_account_id  = true
  #   skip_s3_checksum            = true
  # }
}

