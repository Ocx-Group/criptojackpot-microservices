// MongoDB initialization for local docker-compose development
// Creates audit database with schema validation

db = db.getSiblingDB('cryptojackpot_audit');

db.createCollection('audit_logs', {
  validator: {
    $jsonSchema: {
      bsonType: 'object',
      required: ['timestamp', 'eventType', 'source'],
      properties: {
        timestamp: { bsonType: 'date' },
        eventType: { bsonType: 'string' },
        source: { bsonType: 'string' },
        userId: { bsonType: ['string', 'null'] },
        correlationId: { bsonType: ['string', 'null'] },
        resourceType: { bsonType: ['string', 'null'] },
        resourceId: { bsonType: ['string', 'null'] },
        data: { bsonType: 'object' }
      }
    }
  }
});
