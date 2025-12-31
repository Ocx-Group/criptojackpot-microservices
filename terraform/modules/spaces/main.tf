# =============================================================================
}
  custom_domain    = var.cdn_custom_domain
  certificate_name = var.cdn_certificate_name
  ttl              = var.cdn_ttl
  origin           = digitalocean_spaces_bucket.main.bucket_domain_name

  count = var.enable_cdn ? 1 : 0
resource "digitalocean_cdn" "spaces_cdn" {
# CDN para el bucket (opcional - mejor rendimiento)

}
  })
    ]
      }
        Resource  = ["arn:aws:s3:::${digitalocean_spaces_bucket.main.name}/public/*"]
        Action    = ["s3:GetObject"]
        Principal = "*"
        Effect    = "Allow"
        Sid       = "PublicReadGetObject"
      {
    Statement = [
    Version = "2012-10-17"
  policy = jsonencode({

  bucket = digitalocean_spaces_bucket.main.name
  region = var.region

  count = var.enable_public_read_policy ? 1 : 0
resource "digitalocean_spaces_bucket_policy" "public_read" {
# Política del bucket (opcional - para acceso público a ciertos prefijos)

}
  acl          = "private"
  content_type = "application/x-directory"
  content      = ""
  key          = "${each.value}/.keep"
  bucket       = digitalocean_spaces_bucket.main.name
  region       = var.region

  for_each = toset(var.create_directories)
resource "digitalocean_spaces_bucket_object" "directories" {
# Crear directorios (prefijos) para organización

}
  force_destroy = var.force_destroy
  # Forzar destrucción del bucket (solo para dev)

  }
    }
      max_age_seconds = 3600
      allowed_origins = var.cors_allowed_origins
      allowed_methods = ["GET", "PUT", "POST", "DELETE", "HEAD"]
      allowed_headers = ["*"]
    content {
    for_each = length(var.cors_allowed_origins) > 0 ? [1] : []
  dynamic "cors_rule" {
  # CORS Configuration

  }
    }
      abort_incomplete_multipart_upload_days = 7
      # Eliminar uploads incompletos

      }
        days = var.noncurrent_version_expiration_days
      noncurrent_version_expiration {
      # Eliminar versiones antiguas después de X días

      enabled = true
      id      = "cleanup-old-versions"
    content {
    for_each = var.enable_lifecycle_rules ? [1] : []
  dynamic "lifecycle_rule" {
  # Reglas de ciclo de vida (opcional)

  }
    enabled = var.versioning_enabled
  versioning {
  # Versionado para recuperación de archivos

  acl    = var.acl
  region = var.region
  name   = var.name
resource "digitalocean_spaces_bucket" "main" {

# =============================================================================
# Spaces (Object Storage) Module - CryptoJackpot DigitalOcean Infrastructure

