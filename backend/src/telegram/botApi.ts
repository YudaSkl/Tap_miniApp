/**
 * Минимальные вызовы Bot API (fetch).
 */

export async function answerPreCheckoutQuery(
  botToken: string,
  preCheckoutQueryId: string,
  ok: boolean,
  errorMessage?: string
): Promise<void> {
  const url = `https://api.telegram.org/bot${botToken}/answerPreCheckoutQuery`;
  const body: Record<string, unknown> = {
    pre_checkout_query_id: preCheckoutQueryId,
    ok
  };
  if (!ok && errorMessage) body.error_message = errorMessage;
  const res = await fetch(url, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(body)
  });
  const data = (await res.json()) as { ok: boolean; description?: string };
  if (!data.ok) throw new Error(data.description || 'answerPreCheckoutQuery failed');
}

export async function createStarsInvoiceLink(
  botToken: string,
  params: {
    title: string;
    description: string;
    payload: string;
    prices: { label: string; amount: number }[];
  }
): Promise<string> {
  const url = `https://api.telegram.org/bot${botToken}/createInvoiceLink`;
  // Для Telegram Stars (XTR) поле provider_token не передаём вовсе.
  const body = {
    title: params.title,
    description: params.description,
    payload: params.payload,
    currency: 'XTR',
    prices: params.prices.map((p) => ({ label: p.label, amount: p.amount }))
  };
  const res = await fetch(url, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(body)
  });
  const data = (await res.json()) as { ok: boolean; result?: string; description?: string };
  if (!data.ok || !data.result) throw new Error(data.description || 'createInvoiceLink failed');
  return data.result;
}
