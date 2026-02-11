package com.cryptojackpot.keycloak.spi;

import org.keycloak.Config;
import org.keycloak.events.EventListenerProvider;
import org.keycloak.events.EventListenerProviderFactory;
import org.keycloak.models.KeycloakSession;
import org.keycloak.models.KeycloakSessionFactory;
import org.jboss.logging.Logger;

/**
 * Factory for creating KafkaEventListenerProvider instances.
 * Registered via SPI mechanism in META-INF/services.
 */
public class KafkaEventListenerProviderFactory implements EventListenerProviderFactory {

    private static final Logger LOG = Logger.getLogger(KafkaEventListenerProviderFactory.class);
    
    public static final String PROVIDER_ID = "kafka-event-listener";

    @Override
    public EventListenerProvider create(KeycloakSession session) {
        return new KafkaEventListenerProvider(session);
    }

    @Override
    public void init(Config.Scope config) {
        LOG.info("Initializing Kafka Event Listener Provider Factory");
    }

    @Override
    public void postInit(KeycloakSessionFactory factory) {
        LOG.info("Kafka Event Listener Provider Factory initialized");
    }

    @Override
    public void close() {
        LOG.info("Closing Kafka Event Listener Provider Factory");
        KafkaEventProducer.getInstance().close();
    }

    @Override
    public String getId() {
        return PROVIDER_ID;
    }
}

