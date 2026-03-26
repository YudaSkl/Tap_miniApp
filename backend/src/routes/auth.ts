import type { FastifyInstance } from 'fastify';
import type { AppConfig } from '../config.js';
import type pg from 'pg';
import { validateInitData } from '../telegram/validateInitData.js';
import { upsertUser } from '../users.js';

export async function registerAuthRoutes(
  app: FastifyInstance,
  deps: { cfg: AppConfig; pool: pg.Pool }
): Promise<void> {
  const { cfg, pool } = deps;

  app.post('/api/auth/session', async (request, reply) => {
    const body = request.body as { initData?: string };
    if (!cfg.botToken) {
      return reply.code(500).send({ ok: false, error: 'server_misconfigured' });
    }
    const user = validateInitData(body.initData ?? '', cfg.botToken);
    if (!user) {
      return reply.code(401).send({ ok: false, error: 'invalid_init_data' });
    }
    await upsertUser(pool, user);
    return {
      ok: true,
      user: {
        id: user.id,
        username: user.username ?? '',
        first_name: user.first_name ?? ''
      }
    };
  });
}
