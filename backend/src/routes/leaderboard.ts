import type { FastifyInstance } from 'fastify';
import type { AppConfig } from '../config.js';
import type pg from 'pg';

export async function registerLeaderboardRoutes(
  app: FastifyInstance,
  deps: { cfg: AppConfig; pool: pg.Pool }
): Promise<void> {
  const { pool, cfg } = deps;

  app.get('/api/leaderboard', async (request, reply) => {
    const q = request.query as { limit?: string };
    const limit = Math.min(100, Math.max(1, parseInt(q.limit ?? '20', 10) || 20));

    if (!cfg.databaseUrl) {
      return reply.code(500).send({ ok: false, error: 'server_misconfigured' });
    }

    const { rows } = await pool.query(
      `SELECT COALESCE(u.username, u.first_name, 'player') AS display_name, p.tap_count AS taps
       FROM user_progress p
       JOIN users u ON u.telegram_user_id = p.telegram_user_id
       ORDER BY p.tap_count DESC
       LIMIT $1`,
      [limit]
    );

    return {
      ok: true,
      rows: rows.map((r: { display_name: string; taps: number }) => ({
        username: r.display_name,
        taps: r.taps
      }))
    };
  });
}
