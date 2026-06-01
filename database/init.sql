CREATE TABLE IF NOT EXISTS tickets
(
    id SERIAL PRIMARY KEY,
    event_id INT NOT NULL,
    customer TEXT NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS outbox_messages
(
    id SERIAL PRIMARY KEY,
    type TEXT NOT NULL,
    payload TEXT NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    processed_at TIMESTAMP NULL
);