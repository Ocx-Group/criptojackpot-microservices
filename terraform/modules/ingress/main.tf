# =============================================================================
}
  depends_on = [helm_release.nginx_ingress]

  }
    namespace = "ingress-nginx"
    name      = "ingress-nginx-controller"
  metadata {
data "kubernetes_service" "nginx_ingress" {
# Obtener la IP del Load Balancer

}
  depends_on = [helm_release.cert_manager]

  }
    }
      }
        ]
          }
            }
              }
                class = "nginx"
              ingress = {
            http01 = {
          {
        solvers = [
        }
          name = "letsencrypt-staging"
        privateKeySecretRef = {
        email  = var.letsencrypt_email
        server = "https://acme-staging-v02.api.letsencrypt.org/directory"
      acme = {
    spec = {
    }
      name = "letsencrypt-staging"
    metadata = {
    kind       = "ClusterIssuer"
    apiVersion = "cert-manager.io/v1"
  manifest = {

  count = var.enable_ssl ? 1 : 0
resource "kubernetes_manifest" "letsencrypt_staging" {
# ClusterIssuer para Let's Encrypt (staging - para pruebas)

}
  depends_on = [helm_release.cert_manager]

  }
    }
      }
        ]
          }
            }
              }
                class = "nginx"
              ingress = {
            http01 = {
          {
        solvers = [
        }
          name = "letsencrypt-prod"
        privateKeySecretRef = {
        email  = var.letsencrypt_email
        server = "https://acme-v02.api.letsencrypt.org/directory"
      acme = {
    spec = {
    }
      name = "letsencrypt-prod"
    metadata = {
    kind       = "ClusterIssuer"
    apiVersion = "cert-manager.io/v1"
  manifest = {

  count = var.enable_ssl ? 1 : 0
resource "kubernetes_manifest" "letsencrypt_prod" {
# ClusterIssuer para Let's Encrypt (producción)

}
  depends_on = [helm_release.nginx_ingress]

  timeout = 600
  wait = true

  }
    value = "true"
    name  = "installCRDs"
  set {

  version          = var.cert_manager_version
  create_namespace = true
  namespace        = "cert-manager"
  chart            = "cert-manager"
  repository       = "https://charts.jetstack.io"
  name             = "cert-manager"

  count = var.enable_ssl ? 1 : 0
resource "helm_release" "cert_manager" {
# Cert-Manager para SSL/TLS automático

}
  timeout = 600
  wait = true

  ]
    EOT
          memory: 256Mi
          cpu: 500m
        limits:
          memory: 128Mi
          cpu: 100m
        requests:
      resources:
        enabled: ${var.enable_metrics}
      metrics:
          service.beta.kubernetes.io/do-loadbalancer-healthcheck-protocol: "http"
          service.beta.kubernetes.io/do-loadbalancer-healthcheck-path: "/healthz"
          service.beta.kubernetes.io/do-loadbalancer-algorithm: "round_robin"
          service.beta.kubernetes.io/do-loadbalancer-protocol: "http"
          service.beta.kubernetes.io/do-loadbalancer-name: "cryptojackpot-lb"
        annotations:
        type: LoadBalancer
      service:
      replicaCount: ${var.ingress_replicas}
    controller:
    <<-EOT
  values = [

  version          = var.nginx_ingress_version
  create_namespace = true
  namespace        = "ingress-nginx"
  chart            = "ingress-nginx"
  repository       = "https://kubernetes.github.io/ingress-nginx"
  name             = "ingress-nginx"
resource "helm_release" "nginx_ingress" {
# NGINX Ingress Controller

# =============================================================================
# Instala NGINX Ingress Controller y Cert-Manager via Helm
# Ingress Module - CryptoJackpot DigitalOcean Infrastructure

