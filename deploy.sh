#!/bin/bash
# Script para desplegar CryptoJackpot en DigitalOcean Kubernetes

set -e

echo "🚀 Iniciando despliegue de CryptoJackpot..."

# Variables
REGISTRY="registry.digitalocean.com/cryptojackpot"
VERSION=${1:-"v1.0.0"}

echo "📦 Construyendo imágenes Docker con tag: $VERSION..."

# Build de cada microservicio
docker build -t $REGISTRY/identity-api:$VERSION -f Microservices/Identity/Api/Dockerfile .
docker build -t $REGISTRY/lottery-api:$VERSION -f Microservices/Lottery/Api/Dockerfile .
docker build -t $REGISTRY/order-api:$VERSION -f Microservices/Order/Api/Dockerfile .
docker build -t $REGISTRY/wallet-api:$VERSION -f Microservices/Wallet/Api/Dockerfile .
docker build -t $REGISTRY/winner-api:$VERSION -f Microservices/Winner/Api/Dockerfile .
docker build -t $REGISTRY/notification-api:$VERSION -f Microservices/Notification/Api/Dockerfile .

echo "📤 Subiendo imágenes a DigitalOcean Container Registry..."

docker push $REGISTRY/identity-api:$VERSION
docker push $REGISTRY/lottery-api:$VERSION
docker push $REGISTRY/order-api:$VERSION
docker push $REGISTRY/wallet-api:$VERSION
docker push $REGISTRY/winner-api:$VERSION
docker push $REGISTRY/notification-api:$VERSION

echo "☸️ Aplicando configuraciones de Kubernetes..."

# Aplicar en orden
kubectl apply -f k8s/base/namespace.yaml
kubectl apply -f k8s/base/configmap.yaml
kubectl apply -f k8s/base/secrets.yaml

# NetworkPolicies (seguridad de red)
kubectl apply -f k8s/network/

# Kafka/Redpanda
kubectl apply -f k8s/kafka/redpanda.yaml

# Esperar a que Redpanda esté listo
echo "⏳ Esperando a que Redpanda esté listo..."
kubectl wait --for=condition=ready pod -l app=redpanda -n cryptojackpot --timeout=120s

# Microservicios
kubectl apply -f k8s/microservices/identity/
kubectl apply -f k8s/microservices/lottery/
kubectl apply -f k8s/microservices/order/
kubectl apply -f k8s/microservices/wallet/
kubectl apply -f k8s/microservices/winner/
kubectl apply -f k8s/microservices/notification/

# Ingress namespace y configuración
kubectl apply -f k8s/ingress/namespace.yaml
kubectl label namespace ingress-nginx name=ingress-nginx --overwrite 2>/dev/null || true
kubectl apply -f k8s/ingress/ingress.yaml

echo "✅ Despliegue completado!"
echo ""
echo "📊 Estado de los pods:"
kubectl get pods -n cryptojackpot
echo ""
echo "🌐 Servicios:"
kubectl get svc -n cryptojackpot

