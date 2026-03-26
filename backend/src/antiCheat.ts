/** Максимум прироста тапов за один запрос (наивная защита). */
export const MAX_DELTA = 500;

/** Минимальный интервал между сохранениями с одного пользователя (мс). */
export const MIN_INTERVAL_MS = 2000;

/** Жёсткий потолок тапов на сервере. */
export const MAX_TAPS = 1_000_000;

export type AntiCheatInput = {
  serverTaps: number;
  serverCoins: number;
  clientTaps: number;
  clientCoins: number;
  lastUpdatedAt: Date | null;
  now: Date;
};

export type AntiCheatResult =
  | { ok: true; taps: number; coins: number }
  | { ok: false; reason: string; taps: number; coins: number };

/**
 * Политика MVP: не уменьшаем счётчик с сервера; за один запрос не больше MAX_DELTA;
 * не чаще MIN_INTERVAL_MS; потолок MAX_TAPS. Монеты следуют за тапами (минимум из двух ограничений).
 */
export function applyAntiCheat(input: AntiCheatInput): AntiCheatResult {
  const { serverTaps, serverCoins, clientTaps, clientCoins, lastUpdatedAt, now } = input;

  if (lastUpdatedAt && now.getTime() - lastUpdatedAt.getTime() < MIN_INTERVAL_MS) {
    return { ok: false, reason: 'rate_limit', taps: serverTaps, coins: serverCoins };
  }

  const cappedClientTaps = Math.min(Math.max(0, clientTaps), MAX_TAPS);
  const cappedClientCoins = Math.min(Math.max(0, clientCoins), MAX_TAPS);

  const maxAllowedTaps = Math.min(serverTaps + MAX_DELTA, MAX_TAPS);
  const nextTaps = Math.min(cappedClientTaps, maxAllowedTaps);
  const finalTaps = Math.max(serverTaps, nextTaps);

  const maxAllowedCoins = Math.min(serverCoins + MAX_DELTA, MAX_TAPS);
  const nextCoins = Math.min(cappedClientCoins, maxAllowedCoins);
  const finalCoins = Math.max(serverCoins, nextCoins);

  return { ok: true, taps: finalTaps, coins: finalCoins };
}
