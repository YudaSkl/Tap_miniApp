import type pg from 'pg';
import type { TelegramUser } from './telegram/validateInitData.js';

export async function upsertUser(pool: pg.Pool, user: TelegramUser): Promise<void> {
  await pool.query(
    `INSERT INTO users (telegram_user_id, username, first_name, last_seen_at)
     VALUES ($1, $2, $3, NOW())
     ON CONFLICT (telegram_user_id) DO UPDATE SET
       username = EXCLUDED.username,
       first_name = EXCLUDED.first_name,
       last_seen_at = NOW()`,
    [user.id, user.username ?? null, user.first_name ?? null]
  );
}
