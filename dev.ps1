# Script para desarrollo local - CryptoJackpot
# Reconstruye y levanta todos los contenedores con cambios frescos

param(
    [switch]$InfraOnly,      # Solo levantar infraestructura (para correr APIs desde IDE)
    [switch]$Full,           # Levantar todo (infraestructura + microservicios)
    [switch]$Clean           # Limpieza profunda (eliminar volúmenes también)
)

$ErrorActionPreference = "Stop"

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  CryptoJackpot - Desarrollo Local" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Función para mostrar ayuda
function Show-Help {
    Write-Host "Uso: .\dev.ps1 [opciones]" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Opciones:" -ForegroundColor Yellow
    Write-Host "  -InfraOnly    Solo infraestructura (Postgres, Redpanda, MinIO)" -ForegroundColor Gray
    Write-Host "                Útil para correr APIs desde Visual Studio/Rider"
    Write-Host ""
    Write-Host "  -Full         Infraestructura + Microservicios en Docker" -ForegroundColor Gray
    Write-Host "                Reconstruye imágenes sin caché"
    Write-Host ""
    Write-Host "  -Clean        Limpieza profunda antes de levantar" -ForegroundColor Gray
    Write-Host "                Elimina volúmenes (¡BORRA DATOS!)"
    Write-Host ""
    Write-Host "Ejemplos:" -ForegroundColor Yellow
    Write-Host "  .\dev.ps1 -InfraOnly          # Solo BD y Kafka"
    Write-Host "  .\dev.ps1 -Full               # Todo en Docker"
    Write-Host "  .\dev.ps1 -Full -Clean        # Reset completo"
    Write-Host ""
}

# Si no se especifica ninguna opción, mostrar ayuda
if (-not $InfraOnly -and -not $Full) {
    Show-Help
    Write-Host "Por defecto se ejecutará: -InfraOnly" -ForegroundColor Yellow
    Write-Host ""
    $InfraOnly = $true
}

# ============================================
# MODO: Solo Infraestructura
# ============================================
if ($InfraOnly) {
    Write-Host "🏗️  Modo: Solo Infraestructura" -ForegroundColor Green
    Write-Host "   (APIs se ejecutan desde IDE)" -ForegroundColor Gray
    Write-Host ""

    # Detener contenedores del compose principal si están corriendo
    Write-Host "🛑 Deteniendo contenedores existentes..." -ForegroundColor Yellow
    docker compose down 2>$null
    docker compose -f docker-compose.infra.yaml down 2>$null

    if ($Clean) {
        Write-Host "🧹 Limpieza profunda: Eliminando volúmenes..." -ForegroundColor Red
        docker compose -f docker-compose.infra.yaml down -v
    }

    # Levantar solo infraestructura
    Write-Host "🚀 Levantando infraestructura..." -ForegroundColor Green
    docker compose -f docker-compose.infra.yaml up -d

    Write-Host ""
    Write-Host "✅ Infraestructura lista!" -ForegroundColor Green
    Write-Host ""
    Write-Host "📋 Servicios disponibles:" -ForegroundColor Cyan
    Write-Host "   PostgreSQL:        localhost:5433" -ForegroundColor Gray
    Write-Host "   Redpanda (Kafka):  localhost:29092" -ForegroundColor Gray
    Write-Host "   Redpanda Console:  http://localhost:8080" -ForegroundColor Gray
    Write-Host "   MinIO:             http://localhost:9000" -ForegroundColor Gray
    Write-Host "   MinIO Console:     http://localhost:9001" -ForegroundColor Gray
    Write-Host ""
    Write-Host "💡 Ahora puedes ejecutar los APIs desde tu IDE" -ForegroundColor Yellow
    Write-Host ""
    exit 0
}

# ============================================
# MODO: Full (Infraestructura + Microservicios)
# ============================================
if ($Full) {
    Write-Host "🐳 Modo: Full (Infraestructura + Microservicios)" -ForegroundColor Green
    Write-Host ""

    # Paso 1: Detener todo
    Write-Host "🛑 Deteniendo contenedores existentes..." -ForegroundColor Yellow
    docker compose down 2>$null
    docker compose -f docker-compose.infra.yaml down 2>$null

    if ($Clean) {
        Write-Host "🧹 Limpieza profunda: Eliminando volúmenes..." -ForegroundColor Red
        docker compose down -v
    }

    # Paso 2: Eliminar imágenes de microservicios para forzar rebuild
    Write-Host "🗑️  Eliminando imágenes antiguas de microservicios..." -ForegroundColor Yellow
    $images = @(
        "cryptojackpotdistributed-identity-api",
        "cryptojackpotdistributed-lottery-api",
        "cryptojackpotdistributed-order-api",
        "cryptojackpotdistributed-wallet-api",
        "cryptojackpotdistributed-winner-api",
        "cryptojackpotdistributed-notification-api",
        "cryptojackpotdistributed-api-gateway"
    )
    
    foreach ($image in $images) {
        $exists = docker images -q $image 2>$null
        if ($exists) {
            Write-Host "   Eliminando: $image" -ForegroundColor Gray
            docker rmi -f $image 2>$null
        }
    }

    # Paso 3: Reconstruir sin caché
    Write-Host ""
    Write-Host "🔨 Reconstruyendo imágenes (sin caché)..." -ForegroundColor Yellow
    Write-Host "   Esto puede tomar varios minutos..." -ForegroundColor Gray
    Write-Host ""
    docker compose build --no-cache --pull

    # Paso 4: Levantar todo
    Write-Host ""
    Write-Host "🚀 Levantando todos los servicios..." -ForegroundColor Green
    docker compose up -d

    # Paso 5: Mostrar estado
    Write-Host ""
    Write-Host "✅ Despliegue local completado!" -ForegroundColor Green
    Write-Host ""
    Write-Host "📋 Servicios disponibles:" -ForegroundColor Cyan
    Write-Host "   API Gateway:       http://localhost:5000" -ForegroundColor Gray
    Write-Host "   PostgreSQL:        localhost:5432" -ForegroundColor Gray
    Write-Host "   Redpanda Console:  http://localhost:8080" -ForegroundColor Gray
    Write-Host ""
    Write-Host "📊 Estado de los contenedores:" -ForegroundColor Cyan
    docker compose ps
    Write-Host ""
}
