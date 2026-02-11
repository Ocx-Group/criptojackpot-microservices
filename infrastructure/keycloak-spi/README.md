# Keycloak Kafka Event Listener SPI

This is a Keycloak Event Listener SPI that publishes user registration events to Kafka.

## Overview

When a user registers in Keycloak (self-registration), this SPI:
1. Captures the `REGISTER` event
2. Publishes a `KeycloakUserCreatedEvent` to Kafka topic `keycloak-user-created`
3. Identity microservice consumes the event and creates the user in the local database

## Building

### Prerequisites
- Java 17+
- Maven 3.8+

### Build Command
```bash
cd infrastructure/keycloak-spi
mvn clean package
```

This produces `target/keycloak-kafka-event-listener-1.0.0.jar`

## Deployment Options

### Option 1: Custom Docker Image (Recommended for Production)

Build a custom Keycloak image with the SPI:

```bash
cd infrastructure/keycloak-spi
mvn clean package
docker build -t cryptojackpot/keycloak:latest .
```

Then update the Keycloak deployment to use this image.

### Option 2: Mount JAR via Volume (Development)

1. Build the JAR
2. Create a ConfigMap from the JAR (for small JARs) or use a PersistentVolume
3. Mount into `/opt/keycloak/providers/`

### Kubernetes Deployment

The deployment expects these environment variables in Keycloak:

```yaml
env:
  - name: KAFKA_BOOTSTRAP_SERVERS
    value: "kafka:9092"
  - name: KAFKA_TOPIC_USER_CREATED
    value: "keycloak-user-created"
```

### Enabling the Event Listener in Keycloak

After deploying the SPI, you need to enable it in the Keycloak realm:

1. Go to Keycloak Admin Console
2. Navigate to **Realm Settings** → **Events**
3. Under **Event Listeners**, add `kafka-event-listener`
4. Save

Or add it to your `realm-export.json`:

```json
{
  "eventsListeners": ["jboss-logging", "kafka-event-listener"]
}
```

## Configuration

Set these environment variables in Keycloak:

| Variable | Description | Example |
|----------|-------------|---------|
| `KAFKA_BOOTSTRAP_SERVERS` | Kafka broker addresses | `kafka:9092` |
| `KAFKA_TOPIC_USER_CREATED` | Topic name for user events | `keycloak-user-created` |

## Event Format

The event published to Kafka follows this JSON structure:

```json
{
  "timestamp": "2024-01-15T10:30:00Z",
  "correlationId": "uuid-string",
  "keycloakId": "keycloak-user-uuid",
  "email": "user@example.com",
  "firstName": "John",
  "lastName": "Doe",
  "emailVerified": false,
  "attributes": {
    "country": "1",
    "phone": "+1234567890"
  }
}
```

## Supported Events

Currently handles:
- `REGISTER` - User self-registration

## Logging

Logs are visible in Keycloak's standard output:
```
INFO  [c.c.k.s.KafkaEventListenerProvider] Published user created event to Kafka: user@example.com
```

