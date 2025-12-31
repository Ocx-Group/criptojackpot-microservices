# =============================================================================
# Script de Despliegue CryptoJackpot - Windows
# Integrado con Terraform IaC
# =============================================================================

param(
    [string]$Version = "v1.0.0",
    [switch]$SkipBuild,
    [switch]$UseGeneratedSecrets
)

$ErrorActionPreference = "Stop"

Write-Host "🚀 Iniciando despliegue de CryptoJackpot..." -ForegroundColor Cyan

# -----------------------------------------------------------------------------
# Configuración desde Terraform (si existe)
# -----------------------------------------------------------------------------
$configPath = "deploy-config.json"
$Registry = "registry.digitalocean.com/cryptojackpot"

if (Test-Path $configPath) {
    Write-Host "📄 Usando configuración de Terraform..." -ForegroundColor Yellow
    $config = Get-Content $configPath | ConvertFrom-Json
    $Registry = $config.registry_url
    Write-Host "   Registry: $Registry" -ForegroundColor Gray
    Write-Host "   Cluster: $($config.cluster_name)" -ForegroundColor Gray
    Write-Host "   Environment: $($config.environment)" -ForegroundColor Gray
}
else {
    Write-Host "⚠️ deploy-config.json no encontrado, usando valores por defecto" -ForegroundColor Yellow
    Write-Host "   Para configuración automatizada, ejecuta primero:" -ForegroundColor Gray
    Write-Host "   cd terraform && terraform apply" -ForegroundColor Gray
}

# -----------------------------------------------------------------------------
# Build de Imágenes Docker
# -----------------------------------------------------------------------------
if (-not $SkipBuild) {
    Write-Host "📦 Construyendo imágenes Docker con tag: $Version..." -ForegroundColor Yellow

    # Build de cada microservicio
    docker build -t "$Registry/identity-api:$Version" -f Microservices/Identity/Api/Dockerfile .
    docker build -t "$Registry/lottery-api:$Version" -f Microservices/Lottery/Api/Dockerfile .
    docker build -t "$Registry/order-api:$Version" -f Microservices/Order/Api/Dockerfile .
    docker build -t "$Registry/wallet-api:$Version" -f Microservices/Wallet/Api/Dockerfile .
    docker build -t "$Registry/winner-api:$Version" -f Microservices/Winner/Api/Dockerfile .
    docker build -t "$Registry/notification-api:$Version" -f Microservices/Notification/Api/Dockerfile .

    Write-Host "📤 Subiendo imágenes a DigitalOcean Container Registry..." -ForegroundColor Yellow

    docker push "$Registry/identity-api:$Version"
    docker push "$Registry/lottery-api:$Version"
    docker push "$Registry/order-api:$Version"
    docker push "$Registry/wallet-api:$Version"
    docker push "$Registry/winner-api:$Version"
    docker push "$Registry/notification-api:$Version"
}
else {
    Write-Host "⏭️ Saltando build de imágenes (--SkipBuild)" -ForegroundColor Yellow
}

# -----------------------------------------------------------------------------
# Actualizar tags de imágenes en deployments
# -----------------------------------------------------------------------------
Write-Host "🔄 Actualizando tags de imágenes en manifests..." -ForegroundColor Yellow

$microservices = @("identity", "lottery", "order", "wallet", "winner", "notification")
foreach ($svc in $microservices) {
    $deploymentPath = "k8s/microservices/$svc/deployment.yaml"
    if (Test-Path $deploymentPath) {
        $content = Get-Content $deploymentPath -Raw
        $content = $content -replace "registry\.digitalocean\.com/cryptojackpot/$svc-api:v[\d\.]+", "$Registry/$svc-api:$Version"
        Set-Content $deploymentPath $content
    }
}

# -----------------------------------------------------------------------------
# Aplicar Kubernetes Manifests
# -----------------------------------------------------------------------------
Write-Host "☸️ Aplicando configuraciones de Kubernetes..." -ForegroundColor Yellow

# Aplicar en orden
kubectl apply -f k8s/base/namespace.yaml
kubectl apply -f k8s/base/configmap.yaml

# Usar secrets generados por Terraform si están disponibles
$secretsPath = "k8s/base/secrets.yaml"
if ($UseGeneratedSecrets -and (Test-Path "k8s/base/secrets.generated.yaml")) {
    Write-Host "🔐 Usando secrets generados por Terraform..." -ForegroundColor Green
    $secretsPath = "k8s/base/secrets.generated.yaml"
}
else {
    Write-Host "⚠️ Usando secrets manuales (k8s/base/secrets.yaml)" -ForegroundColor Yellow
    Write-Host "   Para usar secrets de Terraform: .\deploy.ps1 -UseGeneratedSecrets" -ForegroundColor Gray
}
kubectl apply -f $secretsPath

# NetworkPolicies (seguridad de red)
kubectl apply -f k8s/network/

# Kafka/Redpanda
kubectl apply -f k8s/kafka/redpanda.yaml

# Esperar a que Redpanda esté listo
Write-Host "⏳ Esperando a que Redpanda esté listo..." -ForegroundColor Yellow
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
kubectl label namespace ingress-nginx name=ingress-nginx --overwrite 2>$null
kubectl apply -f k8s/ingress/ingress.yaml

# -----------------------------------------------------------------------------
# Resumen del Despliegue
# -----------------------------------------------------------------------------
Write-Host ""
Write-Host "============================================" -ForegroundColor Green
Write-Host "✅ Despliegue completado!" -ForegroundColor Green
Write-Host "============================================" -ForegroundColor Green
Write-Host ""
Write-Host "📊 Estado de los pods:" -ForegroundColor Cyan
kubectl get pods -n cryptojackpot
Write-Host ""
Write-Host "🌐 Servicios:" -ForegroundColor Cyan
kubectl get svc -n cryptojackpot
Write-Host ""
Write-Host "🔗 Ingress:" -ForegroundColor Cyan
kubectl get ingress -n cryptojackpot
Write-Host ""

# Mostrar IP del Load Balancer si está disponible
$lbIP = kubectl get svc -n ingress-nginx ingress-nginx-controller -o jsonpath='{.status.loadBalancer.ingress[0].ip}' 2>$null
if ($lbIP) {
    Write-Host "🌍 Load Balancer IP: $lbIP" -ForegroundColor Green
    Write-Host "   Configura tu DNS para apuntar a esta IP" -ForegroundColor Gray
}

