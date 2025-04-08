// mocks/browser.ts
import { setupWorker } from 'msw/browser';
import { http, HttpResponse } from 'msw';

export const worker = setupWorker(
  http.get('https://auth.sandpit.signin.nhs.uk/authorize', async () => {
    console.log('🛑 Intercepted /authorize fetch');

    // Simulate browser navigation
    window.location.href = 'https://localhost:3001/api/auth/callback/nhs-login?code=fb66c0ac-38ec-4edb-b204-90623968a3e8';

    // Block the fetch from resolving
    return new Promise(() => {});
  })
);
