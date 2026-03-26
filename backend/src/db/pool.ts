import pg from 'pg';
import type { AppConfig } from '../config.js';

/**
 * Пул на одно serverless-окружение: мало соединений, короткая жизнь инстанса.
 */
export function createPool(cfg: AppConfig) {
  return new pg.Pool({
    connectionString: cfg.databaseUrl,
    max: 4,
    idleTimeoutMillis: 10_000,
    connectionTimeoutMillis: 5000
  });
}
