-- Postgres initialization for local docker-compose development
-- Creates all databases needed by CryptoJackpot microservices

CREATE DATABASE cryptojackpot_identity_db;
CREATE DATABASE cryptojackpot_lottery_db;
CREATE DATABASE cryptojackpot_order_db;
CREATE DATABASE cryptojackpot_wallet_db;
CREATE DATABASE cryptojackpot_winner_db;
CREATE DATABASE cryptojackpot_notification_db;
CREATE DATABASE cryptojackpot_content_db;
