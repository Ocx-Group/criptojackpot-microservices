# Keycloak with Kafka Event Listener SPI
# This Dockerfile extends the official Keycloak image and adds the custom SPI

FROM quay.io/keycloak/keycloak:26.0.0 AS builder

# Copy the SPI JAR into the providers directory
COPY target/keycloak-kafka-event-listener-1.0.0.jar /opt/keycloak/providers/

# Build the optimized Keycloak distribution
RUN /opt/keycloak/bin/kc.sh build

FROM quay.io/keycloak/keycloak:26.0.0

# Copy the built providers from builder stage
COPY --from=builder /opt/keycloak/providers/ /opt/keycloak/providers/
COPY --from=builder /opt/keycloak/lib/quarkus/ /opt/keycloak/lib/quarkus/

ENTRYPOINT ["/opt/keycloak/bin/kc.sh"]

