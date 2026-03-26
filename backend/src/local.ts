import 'dotenv/config';
import { buildApp } from './app.js';
import { loadConfig } from './config.js';

async function main() {
  const cfg = loadConfig();
  const app = await buildApp();
  await app.listen({ port: cfg.port, host: '0.0.0.0' });
  app.log.info(`Listening on http://0.0.0.0:${cfg.port}`);
}

main().catch((err) => {
  console.error(err);
  process.exit(1);
});
