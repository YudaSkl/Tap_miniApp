import crypto from 'crypto';

/** Префикс payload инвойса для разбора в webhook (лимит Telegram 128 байт). */
const PREFIX = 'tm';

/**
 * Создаёт короткий payload: tm|<userId>|<hex> для сопоставления successful_payment с пользователем.
 */
export function makeInvoicePayload(telegramUserId: number): string {
  const suffix = crypto.randomBytes(6).toString('hex');
  return `${PREFIX}|${telegramUserId}|${suffix}`;
}

export function parseInvoicePayload(payload: string): { telegramUserId: number } | null {
  const parts = payload.split('|');
  if (parts.length !== 3 || parts[0] !== PREFIX) return null;
  const id = Number(parts[1]);
  if (!Number.isFinite(id) || id <= 0) return null;
  return { telegramUserId: id };
}
