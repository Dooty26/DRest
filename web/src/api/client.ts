import createClient from 'openapi-fetch';
import type { paths } from './schema';
import { createIdempotencyKey } from '../lib/idempotency';

const apiBase = import.meta.env.VITE_API_BASE || '/api/v1';

export const apiClient = createClient<paths>({
  baseUrl: apiBase,
});

/**
 * Trình trợ giúp tạo options kèm header Idempotency-Key
 */
export function withIdempotency(existingKey?: string) {
  const key = existingKey || createIdempotencyKey();
  return {
    headers: {
      'Idempotency-Key': key,
    },
    key,
  };
}
