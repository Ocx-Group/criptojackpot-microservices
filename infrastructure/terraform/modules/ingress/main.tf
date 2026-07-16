# =============================================================================
# Ingress Module - CriptoJackpot
# Instala NGINX Ingress Controller via Helm
#
# Cloudflare SSL "Full" → LB (TCP passthrough) → ingress-nginx (TLS termination
# con Cloudflare Origin Certificate) → HTTP → pods
#
# IMPORTANTE: El protocolo del LB DEBE ser "tcp" para que Cloudflare pueda
# establecer la conexion TLS con el ingress-nginx controller. Si se cambia
# a "http", Cloudflare no podra conectar y mostrara error 521.
# =============================================================================

resource "helm_release" "nginx_ingress" {
  name             = "ingress-nginx"
  repository       = "https://kubernetes.github.io/ingress-nginx"
  chart            = "ingress-nginx"
  namespace        = "ingress-nginx"
  create_namespace = true
  version          = var.nginx_ingress_version

  values = [
    <<-EOT
    controller:
      replicaCount: ${var.ingress_replicas}
      # Usa el Cloudflare Origin Certificate como cert por defecto para TLS
      extraArgs:
        default-ssl-certificate: "ingress-nginx/cloudflare-origin-cert"
      service:
        type: LoadBalancer
        annotations:
          service.beta.kubernetes.io/do-loadbalancer-name: "criptojackpot-lb"
          # CRITICO: Debe ser "tcp" para TLS passthrough con Cloudflare Full SSL.
          # "http" rompe la conexion TLS y causa error 521.
          service.beta.kubernetes.io/do-loadbalancer-protocol: "tcp"
          service.beta.kubernetes.io/do-loadbalancer-algorithm: "round_robin"
          service.beta.kubernetes.io/do-loadbalancer-healthcheck-path: "/healthz"
          service.beta.kubernetes.io/do-loadbalancer-healthcheck-protocol: "http"
          service.beta.kubernetes.io/do-loadbalancer-enable-proxy-protocol: "false"
      config:
        use-forwarded-headers: "true"
        forwarded-for-header: "CF-Connecting-IP"
      metrics:
        enabled: ${var.enable_metrics}
      resources:
        requests:
          cpu: 100m
          memory: 128Mi
        limits:
          cpu: 500m
          memory: 256Mi
    EOT
  ]

  wait    = true
  timeout = 600
}

# Obtener la IP del Load Balancer
data "kubernetes_service" "nginx_ingress" {
  metadata {
    name      = "ingress-nginx-controller"
    namespace = "ingress-nginx"
  }

  depends_on = [helm_release.nginx_ingress]
}
