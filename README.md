# RabbitDemo

RabbitDemo is a sample distributed system built with .NET 10, RabbitMQ, PostgreSQL and Docker Compose.

The project demonstrates asynchronous communication between services using a message broker and a background worker. The API publishes messages to RabbitMQ, while a separate worker consumes those messages and stores the data in PostgreSQL.

## Architecture

```text
Client
  |
  v
OrderApi
  |
  v
RabbitMQ
  |
  v
OrderWorker
  |
  v
PostgreSQL
```

## Solution Structure

```text
RabbitDemo
│
├── Contracts
├── Data
├── OrderApi
└── OrderWorker
```

### Contracts

Contains shared contracts used by multiple services.

Examples:

- TicketCreated
- RabbitMqOptions

### Data

Contains the data access layer.

Technologies:

- Dapper
- Npgsql

Components:

- ITicketRepository
- TicketRepository

### OrderApi

ASP.NET Core Web API responsible for receiving requests and publishing messages to RabbitMQ.

Responsibilities:

- Accept HTTP requests
- Publish messages to RabbitMQ
- Return responses to clients

### OrderWorker

Background worker responsible for consuming messages from RabbitMQ and persisting data into PostgreSQL.

Responsibilities:

- Listen to RabbitMQ queues
- Process messages
- Save data into PostgreSQL

## Technologies

- .NET 10
- ASP.NET Core
- Worker Services
- RabbitMQ
- PostgreSQL
- Dapper
- Docker
- Docker Compose

## Message Flow

```text
POST /api/tickets
       |
       v
RabbitMQ Queue (ticket-orders)
       |
       v
OrderWorker
       |
       v
PostgreSQL
```

## API

### Create Ticket

```http
POST /api/tickets
```

Request:

```json
{
  "eventId": 123,
  "customer": "John Doe"
}
```

Response:

```json
{
  "message": "Ticket published"
}
```

## Running the Application

Start all services:

```bash
docker compose up --build
```

Stop all services:

```bash
docker compose down
```

The compose file starts:

- RabbitMQ
- PostgreSQL
- OrderApi
- OrderWorker

## Database

Database:

```text
ticketdemo
```

Table:

```sql
CREATE TABLE tickets
(
    id SERIAL PRIMARY KEY,
    event_id INTEGER NOT NULL,
    customer VARCHAR(255) NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT NOW()
);
```

## Concepts Demonstrated

- Asynchronous messaging
- Producer / Consumer pattern
- Background processing
- Repository pattern
- Service decoupling
- Docker containerization
- Docker Compose orchestration
- Distributed systems fundamentals

## Future Improvements

- Database migrations
- Retry policies
- Dead Letter Queues
- Redis caching
- MassTransit
- Kafka
- Health checks
- OpenTelemetry
- Kubernetes
