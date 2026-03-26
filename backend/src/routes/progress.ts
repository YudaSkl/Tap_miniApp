import type { FastifyInstance } from 'fastify';
import type { AppConfig } from '../config.js';
import type pg from 'pg';
import { applyAntiCheat } from '../antiCheat.js';
import { validateInitData } from '../telegram/validateInitData.js';
import { upsertUser } from '../users.js';

export async function registerProgressRoutes(
  app: FastifyInstance,
  deps: { cfg: AppConfig; pool: pg.Pool }
): Promise<void> {
  const { cfg, pool } = deps;

  app.post('/api/progress', async (request, reply) => {
    const body = request.body as { initData?: string; taps?: number; coins?: number };
    if (!cfg.botToken) {
      return reply.code(500).send({ ok: false, error: 'server_misconfigured' });
    }
    const user = validateInitData(body.initData ?? '', cfg.botToken);
    if (!user) {
      return reply.code(401).send({ ok: false, error: 'invalid_init_data' });
    }

    const clientTaps = Number(body.taps ?? 0);
    const clientCoins = Number(body.coins ?? 0);
    if (!Number.isFinite(clientTaps) || !Number.isFinite(clientCoins)) {
      return reply.code(400).send({ ok: false, error: 'invalid_numbers' });
    }

    await upsertUser(pool, user);

    const prev = await pool.query(
      `SELECT tap_count, coins, updated_at FROM user_progress WHERE telegram_user_id = $1`,
      [user.id]
    );
    const row = prev.rows[0] as
      | { tap_count: number; coins: number; updated_at: Date }
      | undefined;
    const serverTaps = row?.tap_count ?? 0;
    const serverCoins = row?.coins ?? 0;
    const lastUpdatedAt = row?.updated_at ?? null;

    const result = applyAntiCheat({
      serverTaps,
      serverCoins,
      clientTaps,
      clientCoins,
      lastUpdatedAt,
      now: new Date()
    });

    if (!result.ok) {
      return {
        ok: true,
        saved: { taps: result.taps, coins: result.coins },
        warning: result.reason
      };
    }

    await pool.query(
      `INSERT INTO user_progress (telegram_user_id, tap_count, coins, updated_at)
       VALUES ($1, $2, $3, NOW())
       ON CONFLICT (telegram_user_id) DO UPDATE SET
         tap_count = EXCLUDED.tap_count,
         coins = EXCLUDED.coins,
         updated_at = NOW()`,
      [user.id, result.taps, result.coins]
    );

    return { ok: true, saved: { taps: result.taps, coins: result.coins } };
  });
}
