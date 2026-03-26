import type pg from 'pg';
import { answerPreCheckoutQuery } from './botApi.js';
import { parseInvoicePayload } from './invoicePayload.js';

type TelegramUpdate = {
  pre_checkout_query?: {
    id: string;
    from?: { id: number };
    invoice_payload?: string;
    currency?: string;
    total_amount?: number;
  };
  message?: {
    from?: { id: number };
    successful_payment?: {
      currency: string;
      total_amount: number;
      invoice_payload: string;
      telegram_payment_charge_id: string;
    };
  };
};

/**
 * Обработка входящих update от Telegram (pre_checkout_query, successful_payment).
 */
export async function handleTelegramUpdate(
  pool: pg.Pool,
  botToken: string,
  body: unknown
): Promise<void> {
  const update = body as TelegramUpdate;

  if (update.pre_checkout_query) {
    const q = update.pre_checkout_query;
    const payload = q.invoice_payload ?? '';
    const parsed = parseInvoicePayload(payload);
    if (!parsed) {
      await answerPreCheckoutQuery(botToken, q.id, false, 'Invalid invoice');
      return;
    }
    if (q.currency !== 'XTR') {
      await answerPreCheckoutQuery(botToken, q.id, false, 'Invalid currency');
      return;
    }
    await answerPreCheckoutQuery(botToken, q.id, true);
    return;
  }

  const sp = update.message?.successful_payment;
  if (sp) {
    const parsed = parseInvoicePayload(sp.invoice_payload);
    if (!parsed) return;

    const chargeId = sp.telegram_payment_charge_id;
    const starsAmount = sp.total_amount;

    await pool.query(
      `INSERT INTO payments (telegram_user_id, payload, stars_amount, currency, telegram_payment_charge_id, status)
       VALUES ($1, $2, $3, $4, $5, 'completed')
       ON CONFLICT (telegram_payment_charge_id) DO NOTHING`,
      [parsed.telegramUserId, sp.invoice_payload, starsAmount, sp.currency, chargeId]
    );
  }
}
