import Fastify from 'fastify';
import cors from '@fastify/cors';
import { loadConfig } from './config.js';
import { createPool } from './db/pool.js';
import { registerAuthRoutes } from './routes/auth.js';
import { registerProgressRoutes } from './routes/progress.js';
import { registerLeaderboardRoutes } from './routes/leaderboard.js';
import { registerPaymentRoutes } from './routes/payments.js';
import { handleTelegramUpdate } from './telegram/webhookHandlers.js';

export async function buildApp() {
  const cfg = loadConfig();
  const pool = createPool(cfg);

  const app = Fastify({
    logger: true
  });

  const origin =
    cfg.allowedOrigins === '*'
      ? true
      : cfg.allowedOrigins.split(',').map((s) => s.trim()).filter(Boolean);

  await app.register(cors, { origin });

  app.get('/health', async () => ({ ok: true }));

  await registerAuthRoutes(app, { cfg, pool });
  await registerProgressRoutes(app, { cfg, pool });
  await registerLeaderboardRoutes(app, { cfg, pool });
  await registerPaymentRoutes(app, { cfg, pool });

  app.post('/telegram/webhook', async (request, reply) => {
    const secret = (request.query as { secret?: string }).secret;
    if (!cfg.webhookSecret || secret !== cfg.webhookSecret) {
      return reply.code(403).send({ ok: false });
    }
    if (!cfg.botToken) {
      return reply.code(500).send({ ok: false });
    }
    try {
      await handleTelegramUpdate(pool, cfg.botToken, request.body);
      return { ok: true };
    } catch (e) {
      request.log.error(e);
      return reply.code(500).send({ ok: false });
    }
  });

  app.addHook('onClose', async () => {
    await pool.end();
  });

  return app;
}
