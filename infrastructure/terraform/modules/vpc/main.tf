# =============================================================================
# VPC Module - CryptoJackpot DigitalOcean Infrastructure
# =============================================================================

resource "digitalocean_vpc" "main" {
  name        = var.name
  region      = var.region
  ip_range    = var.ip_range
  description = var.description

  lifecycle {
    # CRÍTICO: cambiar name/region/ip_range destruye la VPC y lo conectado
    # (cluster K8s, bases de datos). Hacerlo solo manualmente con migration plan.
    prevent_destroy = true

    ignore_changes = [description]
  }
}
