import type { FastifyInstance } from 'fastify';
import type { AppConfig } from '../config.js';
import type pg from 'pg';
import { createStarsInvoiceLink } from '../telegram/botApi.js';
import { makeInvoicePayload } from '../telegram/invoicePayload.js';
import { validateInitData } from '../telegram/validateInitData.js';
import { upsertUser } from '../users.js';

const MIN_STARS = 1;
const MAX_STARS = 10_000;

export async function registerPaymentRoutes(
  app: FastifyInstance,
  deps: { cfg: AppConfig; pool: pg.Pool }
): Promise<void> {
  const { cfg, pool } = deps;

  app.post('/api/payments/invoice', async (request, reply) => {
    const body = request.body as { initData?: string; amountStars?: number; itemLabel?: string };
    if (!cfg.botToken) {
      return reply.code(500).send({ ok: false, error: 'server_misconfigured' });
    }
    const user = validateInitData(body.initData ?? '', cfg.botToken);
    if (!user) {
      return reply.code(401).send({ ok: false, error: 'invalid_init_data' });
    }

    const amountStars = Math.floor(Number(body.amountStars ?? 0));
    if (!Number.isFinite(amountStars) || amountStars < MIN_STARS || amountStars > MAX_STARS) {
      return reply.code(400).send({ ok: false, error: 'invalid_amount' });
    }

    await upsertUser(pool, user);

    const label = (body.itemLabel || 'Support').slice(0, 64);
    const payload = makeInvoicePayload(user.id);

    try {
      const invoiceUrl = await createStarsInvoiceLink(cfg.botToken, {
        title: label,
        description: 'Tap Mini App support',
        payload,
        prices: [{ label, amount: amountStars }]
      });
      return { ok: true, invoiceUrl };
    } catch (e) {
      request.log.error(e);
      return reply.code(502).send({ ok: false, error: 'invoice_failed' });
    }
  });
}
