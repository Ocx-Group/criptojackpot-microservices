# =============================================================================
# Secrets Module - CriptoJackpot
# =============================================================================
# NOTA (2026-06): Los Secrets de aplicación los gestiona EXCLUSIVAMENTE ArgoCD
# vía SealedSecrets (overlays/{env}/secrets/*). Anteriormente este módulo también
# creaba kubernetes_secret con los mismos nombres, lo que duplicaba la gestión:
# el SealedSecret-controller dueña los Secrets vivos (ownerReference=SealedSecret)
# y los kubernetes_secret de Terraform quedaban inertes (lifecycle ignore_changes).
#
# Se eliminaron esos recursos. Los bloques `removed { destroy = false }` de abajo
# los sacan del state de Terraform SIN borrar el Secret vivo en el cluster.
# Tras el primer apply que los retire del state, estos bloques pueden eliminarse.
#
# Este módulo solo conserva la creación del namespace.
# =============================================================================

resource "kubernetes_namespace" "main" {
  metadata {
    name = var.namespace
    labels = {
      name        = var.namespace
      environment = var.environment
    }
  }
}

# -----------------------------------------------------------------------------
# Retiro de los kubernetes_secret legacy del state (sin destruir el objeto vivo).
# Los Secrets reales los materializa el SealedSecret-controller desde
# overlays/{env}/secrets/*.yaml — esa es la única fuente de verdad.
# -----------------------------------------------------------------------------
removed {
  from = kubernetes_secret.postgres
  lifecycle {
    destroy = false
  }
}

removed {
  from = kubernetes_secret.jwt
  lifecycle {
    destroy = false
  }
}

removed {
  from = kubernetes_secret.kafka
  lifecycle {
    destroy = false
  }
}

removed {
  from = kubernetes_secret.redis
  lifecycle {
    destroy = false
  }
}

removed {
  from = kubernetes_secret.mongodb
  lifecycle {
    destroy = false
  }
}

removed {
  from = kubernetes_secret.spaces
  lifecycle {
    destroy = false
  }
}

removed {
  from = kubernetes_secret.brevo
  lifecycle {
    destroy = false
  }
}

removed {
  from = kubernetes_secret.google
  lifecycle {
    destroy = false
  }
}
