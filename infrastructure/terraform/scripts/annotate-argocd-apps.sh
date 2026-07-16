#!/bin/bash
# =============================================================================
# Anota las ArgoCD Applications con la configuracion de Image Updater.
# Ejecutar UNA VEZ despues de instalar ArgoCD Image Updater.
#
# Uso:
#   ./annotate-argocd-apps.sh prod    # Anota apps de produccion
#   ./annotate-argocd-apps.sh qa      # Anota apps de QA
# =============================================================================

set -euo pipefail

ENV="${1:-prod}"
REGISTRY="registry.digitalocean.com/criptojackpot"

if [ "$ENV" = "prod" ]; then
  TAG_PATTERN="regexp:^prod-[a-f0-9]{7}$"
  MICRO_APP="criptojackpot-microservices"
  WEB_APP="criptojackpot-web"
elif [ "$ENV" = "qa" ]; then
  TAG_PATTERN="regexp:^qa-[a-f0-9]{7}$"
  MICRO_APP="criptojackpot-microservices-qa"
  WEB_APP="criptojackpot-web-qa"
else
  echo "Uso: $0 [prod|qa]"
  exit 1
fi

echo "=== Anotando ArgoCD Applications para ambiente: ${ENV} ==="

# --- Microservices Application ---
echo ""
echo "--- Anotando ${MICRO_APP} (9 imagenes) ---"

SERVICES="migrator bff identity lottery order wallet winner notification audit"
IMAGE_LIST=""
for svc in $SERVICES; do
  case $svc in
    bff)          IMAGE_NAME="bff-gateway" ;;
    identity)     IMAGE_NAME="identity-api" ;;
    lottery)      IMAGE_NAME="lottery-api" ;;
    order)        IMAGE_NAME="order-api" ;;
    wallet)       IMAGE_NAME="wallet-api" ;;
    winner)       IMAGE_NAME="winner-api" ;;
    notification) IMAGE_NAME="notification-api" ;;
    audit)        IMAGE_NAME="audit-api" ;;
    migrator)     IMAGE_NAME="migrator" ;;
  esac

  if [ -n "$IMAGE_LIST" ]; then
    IMAGE_LIST="${IMAGE_LIST}, "
  fi
  IMAGE_LIST="${IMAGE_LIST}${svc}=${REGISTRY}/${IMAGE_NAME}"
done

# Aplicar anotaciones al app de microservices
kubectl -n argocd annotate application "${MICRO_APP}" \
  "argocd-image-updater.argoproj.io/image-list=${IMAGE_LIST}" \
  "argocd-image-updater.argoproj.io/write-back-method=argocd" \
  --overwrite

for svc in $SERVICES; do
  case $svc in
    bff)          KUST_NAME="cryptojackpot/bff-gateway" ;;
    identity)     KUST_NAME="cryptojackpot/identity-api" ;;
    lottery)      KUST_NAME="cryptojackpot/lottery-api" ;;
    order)        KUST_NAME="cryptojackpot/order-api" ;;
    wallet)       KUST_NAME="cryptojackpot/wallet-api" ;;
    winner)       KUST_NAME="cryptojackpot/winner-api" ;;
    notification) KUST_NAME="cryptojackpot/notification-api" ;;
    audit)        KUST_NAME="cryptojackpot/audit-api" ;;
    migrator)     KUST_NAME="cryptojackpot/migrator" ;;
  esac

  kubectl -n argocd annotate application "${MICRO_APP}" \
    "argocd-image-updater.argoproj.io/${svc}.update-strategy=newest-build" \
    "argocd-image-updater.argoproj.io/${svc}.allow-tags=${TAG_PATTERN}" \
    "argocd-image-updater.argoproj.io/${svc}.kustomize.image-name=${KUST_NAME}" \
    --overwrite
done

echo "OK: ${MICRO_APP} anotado con ${#SERVICES} imagenes."

# --- Frontend Application ---
echo ""
echo "--- Anotando ${WEB_APP} (1 imagen) ---"

kubectl -n argocd annotate application "${WEB_APP}" \
  "argocd-image-updater.argoproj.io/image-list=frontend=${REGISTRY}/cryptojackpot-app" \
  "argocd-image-updater.argoproj.io/write-back-method=argocd" \
  "argocd-image-updater.argoproj.io/frontend.update-strategy=newest-build" \
  "argocd-image-updater.argoproj.io/frontend.allow-tags=${TAG_PATTERN}" \
  "argocd-image-updater.argoproj.io/frontend.kustomize.image-name=cryptojackpot/cryptojackpot-app" \
  --overwrite

echo "OK: ${WEB_APP} anotado."

echo ""
echo "=== Listo! Image Updater comenzara a monitorear las imagenes en el proximo ciclo de polling. ==="
echo "    Verificar logs: kubectl logs -n argocd -l app.kubernetes.io/name=argocd-image-updater -f"
