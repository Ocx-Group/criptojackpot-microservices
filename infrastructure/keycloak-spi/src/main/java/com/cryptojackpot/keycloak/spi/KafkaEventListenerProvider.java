package com.cryptojackpot.keycloak.spi;

import org.keycloak.events.Event;
import org.keycloak.events.EventListenerProvider;
import org.keycloak.events.EventType;
import org.keycloak.events.admin.AdminEvent;
import org.keycloak.models.KeycloakSession;
import org.keycloak.models.UserModel;
import org.jboss.logging.Logger;

import java.util.HashMap;
import java.util.Map;

/**
 * Keycloak Event Listener that publishes user registration events to Kafka.
 * Triggered when users self-register through Keycloak.
 */
public class KafkaEventListenerProvider implements EventListenerProvider {

    private static final Logger LOG = Logger.getLogger(KafkaEventListenerProvider.class);
    
    private final KeycloakSession session;
    private final KafkaEventProducer kafkaProducer;

    public KafkaEventListenerProvider(KeycloakSession session) {
        this.session = session;
        this.kafkaProducer = KafkaEventProducer.getInstance();
    }

    @Override
    public void onEvent(Event event) {
        // Only handle REGISTER events
        if (event.getType() != EventType.REGISTER) {
            return;
        }

        LOG.infof("Processing REGISTER event for user: %s", event.getUserId());

        try {
            // Get user details from Keycloak
            UserModel user = session.users().getUserById(
                session.getContext().getRealm(), 
                event.getUserId()
            );

            if (user == null) {
                LOG.warnf("User not found for ID: %s", event.getUserId());
                return;
            }

            // Extract custom attributes
            Map<String, String> attributes = extractAttributes(user);

            // Build and publish event
            KeycloakUserCreatedEvent userEvent = KeycloakUserCreatedEvent.builder()
                .keycloakId(user.getId())
                .email(user.getEmail())
                .firstName(user.getFirstName())
                .lastName(user.getLastName())
                .emailVerified(user.isEmailVerified())
                .attributes(attributes)
                .build();

            kafkaProducer.publishUserCreatedEvent(userEvent);

            LOG.infof("Published user created event to Kafka for: %s", user.getEmail());

        } catch (Exception e) {
            LOG.errorf(e, "Error processing REGISTER event for user: %s", event.getUserId());
        }
    }

    @Override
    public void onEvent(AdminEvent event, boolean includeRepresentation) {
        // Handle admin-created users if needed in the future
        // For now, we only handle self-registration via onEvent(Event)
    }

    @Override
    public void close() {
        // Producer is a singleton, don't close it here
    }

    /**
     * Extracts relevant user attributes for the event.
     */
    private Map<String, String> extractAttributes(UserModel user) {
        Map<String, String> attributes = new HashMap<>();
        
        // Extract known attributes used by Identity service
        String[] knownAttributes = {"country", "statePlace", "city", "address", "phone"};
        
        for (String attr : knownAttributes) {
            var values = user.getAttributeStream(attr).findFirst();
            values.ifPresent(value -> attributes.put(attr, value));
        }

        return attributes;
    }
}

