# =============================================================================
# ArgoCD Image Updater Module - CriptoJackpot
# Instala ArgoCD Image Updater via Helm y configura acceso a DOCR privado.
#
# Image Updater sondea el registry de DigitalOcean (DOCR) buscando nuevas
# imagenes. Cuando detecta un tag nuevo que coincide con el patron configurado
# (prod-* o qa-*), actualiza el ArgoCD Application directamente sin hacer
# commits a git (write-back method: argocd).
#
# Flujo: CI/CD push imagen → Image Updater detecta (~2 min) → ArgoCD sync
# =============================================================================

# -----------------------------------------------------------------------------
# Secret con credenciales Docker para DOCR
# Image Updater necesita pull access al registry privado para listar tags.
# DOCR usa do_token como password con cualquier username.
# -----------------------------------------------------------------------------
resource "kubernetes_secret" "docr_credentials" {
  metadata {
    name      = "docr-credentials"
    namespace = var.argocd_namespace
    labels = {
      "app.kubernetes.io/part-of" = "argocd-image-updater"
    }
  }

  type = "kubernetes.io/dockerconfigjson"

  data = {
    ".dockerconfigjson" = jsonencode({
      auths = {
        "registry.digitalocean.com" = {
          username = "_"
          password = var.docr_token
          auth     = base64encode("_:${var.docr_token}")
        }
      }
    })
  }
}

# -----------------------------------------------------------------------------
# Helm Release — ArgoCD Image Updater
# -----------------------------------------------------------------------------
resource "helm_release" "argocd_image_updater" {
  name             = "argocd-image-updater"
  repository       = "https://argoproj.github.io/argo-helm"
  chart            = "argocd-image-updater"
  namespace        = var.argocd_namespace
  create_namespace = false
  version          = var.chart_version

  values = [
    <<-EOT
    config:
      registries:
        - name: DOCR
          prefix: registry.digitalocean.com
          api_url: https://registry.digitalocean.com
          credentials: pullsecret:${var.argocd_namespace}/docr-credentials
          defaultns: criptojackpot
          default: true
      argocd:
        grpcWeb: true
        serverAddress: argocd-server.${var.argocd_namespace}.svc.cluster.local
        insecure: true
        plaintext: false
    extraArgs:
      - --interval=${var.poll_interval}
    logLevel: ${var.log_level}
    resources:
      requests:
        cpu: 50m
        memory: 64Mi
      limits:
        cpu: 200m
        memory: 128Mi
    EOT
  ]

  depends_on = [kubernetes_secret.docr_credentials]

  wait    = true
  timeout = 300
}
