export type AppConfig = {
  botToken: string;
  databaseUrl: string;
  webhookSecret: string;
  port: number;
  allowedOrigins: string;
};

/**
 * Читает конфигурацию из process.env.
 * Для serverless (Yandex) задайте переменные в консоли облака.
 */
export function loadConfig(): AppConfig {
  const botToken = process.env.BOT_TOKEN ?? '';
  const databaseUrl = process.env.DATABASE_URL ?? '';
  const webhookSecret = process.env.WEBHOOK_SECRET ?? '';
  const port = Number(process.env.PORT || 3000);
  const allowedOrigins = process.env.ALLOWED_ORIGINS ?? '*';

  return {
    botToken,
    databaseUrl,
    webhookSecret,
    port,
    allowedOrigins
  };
}
