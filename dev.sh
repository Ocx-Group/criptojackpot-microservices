#!/bin/bash
# Script para desarrollo local - CryptoJackpot
# Reconstruye y levanta todos los contenedores con cambios frescos

set -e

# Colores
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
GRAY='\033[0;37m'
NC='\033[0m' # No Color

echo ""
echo -e "${CYAN}========================================"
echo -e "  CryptoJackpot - Desarrollo Local"
echo -e "========================================${NC}"
echo ""

# Variables
INFRA_ONLY=false
FULL=false
CLEAN=false

# Parsear argumentos
while [[ $# -gt 0 ]]; do
    case $1 in
        --infra-only|-i)
            INFRA_ONLY=true
            shift
            ;;
        --full|-f)
            FULL=true
            shift
            ;;
        --clean|-c)
            CLEAN=true
            shift
            ;;
        --help|-h)
            echo -e "${YELLOW}Uso: ./dev.sh [opciones]${NC}"
            echo ""
            echo -e "${YELLOW}Opciones:${NC}"
            echo -e "${GRAY}  --infra-only, -i    Solo infraestructura (Postgres, Redpanda, MinIO)"
            echo -e "                      Útil para correr APIs desde Visual Studio/Rider"
            echo ""
            echo -e "  --full, -f          Infraestructura + Microservicios en Docker"
            echo -e "                      Reconstruye imágenes sin caché"
            echo ""
            echo -e "  --clean, -c         Limpieza profunda antes de levantar"
            echo -e "                      Elimina volúmenes (¡BORRA DATOS!)${NC}"
            echo ""
            echo -e "${YELLOW}Ejemplos:${NC}"
            echo "  ./dev.sh --infra-only          # Solo BD y Kafka"
            echo "  ./dev.sh --full                # Todo en Docker"
            echo "  ./dev.sh --full --clean        # Reset completo"
            echo ""
            exit 0
            ;;
        *)
            echo -e "${RED}Opción desconocida: $1${NC}"
            echo "Usa --help para ver las opciones disponibles"
            exit 1
            ;;
    esac
done

# Si no se especifica ninguna opción, usar InfraOnly por defecto
if [ "$INFRA_ONLY" = false ] && [ "$FULL" = false ]; then
    echo -e "${YELLOW}Por defecto se ejecutará: --infra-only${NC}"
    echo ""
    INFRA_ONLY=true
fi

# ============================================
# MODO: Solo Infraestructura
# ============================================
if [ "$INFRA_ONLY" = true ]; then
    echo -e "${GREEN}🏗️  Modo: Solo Infraestructura${NC}"
    echo -e "${GRAY}   (APIs se ejecutan desde IDE)${NC}"
    echo ""

    # Detener contenedores existentes
    echo -e "${YELLOW}🛑 Deteniendo contenedores existentes...${NC}"
    docker compose down 2>/dev/null || true
    docker compose -f docker-compose.infra.yaml down 2>/dev/null || true

    if [ "$CLEAN" = true ]; then
        echo -e "${RED}🧹 Limpieza profunda: Eliminando volúmenes...${NC}"
        docker compose -f docker-compose.infra.yaml down -v
    fi

    # Levantar solo infraestructura
    echo -e "${GREEN}🚀 Levantando infraestructura...${NC}"
    docker compose -f docker-compose.infra.yaml up -d

    echo ""
    echo -e "${GREEN}✅ Infraestructura lista!${NC}"
    echo ""
    echo -e "${CYAN}📋 Servicios disponibles:${NC}"
    echo -e "${GRAY}   PostgreSQL:        localhost:5433"
    echo -e "   Redpanda (Kafka):  localhost:29092"
    echo -e "   Redpanda Console:  http://localhost:8080"
    echo -e "   MinIO:             http://localhost:9000"
    echo -e "   MinIO Console:     http://localhost:9001${NC}"
    echo ""
    echo -e "${YELLOW}💡 Ahora puedes ejecutar los APIs desde tu IDE${NC}"
    echo ""
    exit 0
fi

# ============================================
# MODO: Full (Infraestructura + Microservicios)
# ============================================
if [ "$FULL" = true ]; then
    echo -e "${GREEN}🐳 Modo: Full (Infraestructura + Microservicios)${NC}"
    echo ""

    # Paso 1: Detener todo
    echo -e "${YELLOW}🛑 Deteniendo contenedores existentes...${NC}"
    docker compose down 2>/dev/null || true
    docker compose -f docker-compose.infra.yaml down 2>/dev/null || true

    if [ "$CLEAN" = true ]; then
        echo -e "${RED}🧹 Limpieza profunda: Eliminando volúmenes...${NC}"
        docker compose down -v
    fi

    # Paso 2: Eliminar imágenes de microservicios para forzar rebuild
    echo -e "${YELLOW}🗑️  Eliminando imágenes antiguas de microservicios...${NC}"
    
    IMAGES=(
        "cryptojackpotdistributed-identity-api"
        "cryptojackpotdistributed-lottery-api"
        "cryptojackpotdistributed-order-api"
        "cryptojackpotdistributed-wallet-api"
        "cryptojackpotdistributed-winner-api"
        "cryptojackpotdistributed-notification-api"
        "cryptojackpotdistributed-api-gateway"
    )
    
    deleted_count=0
    for image in "${IMAGES[@]}"; do
        if docker images -q "$image" 2>/dev/null | grep -q .; then
            echo -e "${GRAY}   ✓ Eliminando: $image${NC}"
            docker rmi -f "$image" 2>/dev/null || true
            ((deleted_count++))
        fi
    done
    
    if [ $deleted_count -eq 0 ]; then
        echo -e "${GRAY}   (No había imágenes antiguas)${NC}"
    else
        echo -e "${GRAY}   $deleted_count imagen(es) eliminada(s)${NC}"
    fi

    # Paso 3: Reconstruir sin caché (con progreso visible)
    echo ""
    echo -e "${YELLOW}🔨 Reconstruyendo imágenes (sin caché)...${NC}"
    echo -e "${GRAY}   Esto puede tomar varios minutos...${NC}"
    echo -e "${GRAY}   ─────────────────────────────────────${NC}"
    echo ""
    
    # --progress=plain muestra el progreso detallado
    docker compose build --no-cache --pull --progress=plain
    
    if [ $? -ne 0 ]; then
        echo ""
        echo -e "${RED}❌ Error durante el build. Revisa los mensajes anteriores.${NC}"
        exit 1
    fi

    # Paso 4: Levantar microservicios
    echo ""
    echo -e "${GRAY}   ─────────────────────────────────────${NC}"
    echo -e "${GREEN}🚀 Levantando microservicios...${NC}"
    docker compose up -d

    # Paso 5: Levantar infraestructura (último para que todo conecte)
    echo -e "${GREEN}🏗️  Levantando infraestructura...${NC}"
    docker compose -f docker-compose.infra.yaml up -d

    # Paso 6: Mostrar estado
    echo ""
    echo -e "${GREEN}✅ Despliegue local completado!${NC}"
    echo ""
    echo -e "${CYAN}📋 Servicios disponibles:${NC}"
    echo -e "${GRAY}   API Gateway:       http://localhost:5000"
    echo -e "   PostgreSQL:        localhost:5432"
    echo -e "   Redpanda Console:  http://localhost:8080${NC}"
    echo ""
    echo -e "${CYAN}📊 Estado de los contenedores:${NC}"
    docker compose ps
    echo ""
fi
