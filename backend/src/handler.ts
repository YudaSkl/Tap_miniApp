/**
 * Точка входа для Yandex Cloud Function / AWS Lambda.
 * Укажите handler: index.handler (после сборки — dist/handler.js → handler.export).
 */
import awsLambdaFastify from '@fastify/aws-lambda';
import { buildApp } from './app.js';

let proxy: ReturnType<typeof awsLambdaFastify> | null = null;
let ready: Promise<ReturnType<typeof awsLambdaFastify>> | null = null;

async function getProxy() {
  if (proxy) return proxy;
  if (!ready) {
    ready = (async () => {
      const app = await buildApp();
      await app.ready();
      return awsLambdaFastify(app);
    })();
  }
  proxy = await ready;
  return proxy;
}

export const handler = async (event: unknown, context: unknown) => {
  const p = await getProxy();
  return p(event, context);
};
