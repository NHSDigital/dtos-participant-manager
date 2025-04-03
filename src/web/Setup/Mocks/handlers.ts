import { http, HttpResponse } from "msw";
import user from "../Data/user.json";
import accessToken from "../Data/access_token.json";

export const handlers = [
  // Authentication handler (mocking NHS token exchange)
  http.get("https://auth.sandpit.signin.nhs.uk/.well-known/openid-configuration", () => {
    console.log("🟢 MSW handler hit: GitHub user endpoint");
    return HttpResponse.json({"issuer": "https://auth.sandpit.signin.nhs.uk", "authorization_endpoint": "https://localhost:3000/api/auth/callback/nhs-login?code=fb66c0ac-38ec-4edb-b204-90623968a3e8", "token_endpoint": "https://auth.sandpit.signin.nhs.uk/token", "jwks_uri": "https://auth.sandpit.signin.nhs.uk/.well-known/jwks.json", "scopes_supported": ["basic_demographics", "client_metadata", "email", "gp_integration_credentials", "gp_registration_details", "landline", "nhs_app_credentials", "openid", "phone", "profile", "profile_extended"], "response_types_supported": ["code"], "subject_types_supported": ["public"], "id_token_signing_alg_values_supported": ["RS512"], "claims_supported": ["iss", "aud", "sub", "family_name", "given_name", "email", "email_verified", "phone_number", "phone_number_verified", "landline_number", "landline_number_verified", "birthdate", "nhs_number", "gp_integration_credentials", "delegations", "gp_registration_details", "im1_token", "auth_time", "jti", "nonce", "vot", "vtm", "exp", "ods_code", "user_id", "linkage_key", "surname", "iat", "gp_ods_code", "gp_user_id", "gp_linkage_key", "client_user_metadata", "identity_proofing_level"], "userinfo_endpoint": "https://auth.sandpit.signin.nhs.uk/userinfo", "token_endpoint_auth_signing_alg_values_supported": ["RS512"], "token_endpoint_auth_methods_supported": ["private_key_jwt"], "fido_uaf_authentication_request_endpoint": "https://uaf.sandpit.signin.nhs.uk/authRequest", "fido_uaf_registration_request_endpoint": "https://uaf.sandpit.signin.nhs.uk/regRequest", "fido_uaf_registration_response_endpoint": "https://uaf.sandpit.signin.nhs.uk/regResponse", "fido_uaf_deregistration_endpoint": "https://uaf.sandpit.signin.nhs.uk/dereg"}
    );
  }),

  http.post(
    "https://auth.sandpit.signin.nhs.uk/token",
    async ({ request }) => {
      console.log("🟢 MSW handler hit: NHS access_token endpoint");
      const body = await request.text();
      console.log("🔍 Request body:", body);

      return HttpResponse.json({
        "access_token": "eyJhbGciOiJSUzUxMiIsImF1ZCI6InNjcmVlbmluZyBwYXJ0aWNpcGFudCBtYW5hZ2VyIiwiZXhwIjoxNzQzNjEyMjQ0LCJpYXQiOjE3NDM2MTE5NDQsImlzcyI6Imh0dHBzOi8vYXV0aC5zYW5kcGl0LnNpZ25pbi5uaHMudWsiLCJqdGkiOiJhMGE1NTJiYi03Y2IxLTRjYjEtYWQxMS01ODMyY2IxM2NkZTEiLCJraWQiOiJhYzEwMjlkMmNiODFiNDUyN2EwYjYzZTJiNGYyODAzNDVmMDRkZDQyIiwic3ViIjoiNDlmNDcwYTEtY2M1Mi00OWI3LWJlYmEtMGY5Y2VjOTM3YzQ2IiwidHlwIjoiSldUIn0.eyJpc3MiOiJodHRwczovL2F1dGguc2FuZHBpdC5zaWduaW4ubmhzLnVrIiwic3ViIjoiNDlmNDcwYTEtY2M1Mi00OWI3LWJlYmEtMGY5Y2VjOTM3YzQ2IiwiYXVkIjoic2NyZWVuaW5nIHBhcnRpY2lwYW50IG1hbmFnZXIiLCJpYXQiOjE3NDM2MTE5NDQsInZ0bSI6Imh0dHBzOi8vYXV0aC5zYW5kcGl0LnNpZ25pbi5uaHMudWsvdHJ1c3RtYXJrL2F1dGguc2FuZHBpdC5zaWduaW4ubmhzLnVrIiwiYXV0aF90aW1lIjoxNzQzNjExOTM5LCJ2b3QiOiJQOS5DcC5DZCIsImV4cCI6MTc0MzYxMjI0NCwianRpIjoiYTBhNTUyYmItN2NiMS00Y2IxLWFkMTEtNTgzMmNiMTNjZGUxIiwibmhzX251bWJlciI6Ijk2ODYzNjg5NzMiLCJ2ZXJzaW9uIjowLCJjbGllbnRfaWQiOiJzY3JlZW5pbmcgcGFydGljaXBhbnQgbWFuYWdlciIsInJlYXNvbl9mb3JfcmVxdWVzdCI6InBhdGllbnRhY2Nlc3MiLCJ0b2tlbl91c2UiOiJhY2Nlc3MiLCJyZXF1ZXN0aW5nX3BhdGllbnQiOiI5Njg2MzY4OTczIiwic2NvcGUiOiJvcGVuaWQgcHJvZmlsZSBwcm9maWxlX2V4dGVuZGVkIn0.du74nl8JmQKPotbendsKvZvq-01-WkoEKSKkJftZBFRIS1XnYXIrJ6JfAxXEwCv3XuPeYyke3sWSRSzIx3LjMX_-XuEuI6-y1a6KhNVvk92wGLwzBP5ZhaLFrTe1P8dtfPO7Tr7_jMrGQwoy6Qa70tgofq67aLv2i_zk1uWaa3dQfddsg3lmbrAP4_DYBndWgATw1Xh5nppgjA3p1T_oTxubQqv5xls5c7q5gtOzofDlTy0zSdJDyHiimMvAN03oeHtZ9ONEnWow6d-hAewPB9sWETJeYHFYd4sxt9fFaWYlm71bwUUEfl4OYU6ulIkr11EbWs3d8_kgiSRhLXBD7w",
        "id_token": "eyJhbGciOiJSUzUxMiIsImF1ZCI6InNjcmVlbmluZyBwYXJ0aWNpcGFudCBtYW5hZ2VyIiwiZXhwIjoxNzQzNjE1NTQ0LCJpYXQiOjE3NDM2MTE5NDQsImlzcyI6Imh0dHBzOi8vYXV0aC5zYW5kcGl0LnNpZ25pbi5uaHMudWsiLCJqdGkiOiI0NGMwOWQwNC1iMzM5LTQ1YjMtOGNjMy04ZTg2OGNjZjMwNWUiLCJraWQiOiJhYzEwMjlkMmNiODFiNDUyN2EwYjYzZTJiNGYyODAzNDVmMDRkZDQyIiwic3ViIjoiNDlmNDcwYTEtY2M1Mi00OWI3LWJlYmEtMGY5Y2VjOTM3YzQ2IiwidHlwIjoiSldUIn0.eyJpc3MiOiJodHRwczovL2F1dGguc2FuZHBpdC5zaWduaW4ubmhzLnVrIiwic3ViIjoiNDlmNDcwYTEtY2M1Mi00OWI3LWJlYmEtMGY5Y2VjOTM3YzQ2IiwiYXVkIjoic2NyZWVuaW5nIHBhcnRpY2lwYW50IG1hbmFnZXIiLCJpYXQiOjE3NDM2MTE5NDQsInZ0bSI6Imh0dHBzOi8vYXV0aC5zYW5kcGl0LnNpZ25pbi5uaHMudWsvdHJ1c3RtYXJrL2F1dGguc2FuZHBpdC5zaWduaW4ubmhzLnVrIiwiYXV0aF90aW1lIjoxNzQzNjExOTM5LCJ2b3QiOiJQOS5DcC5DZCIsImV4cCI6MTc0MzYxNTU0NCwianRpIjoiNDRjMDlkMDQtYjMzOS00NWIzLThjYzMtOGU4NjhjY2YzMDVlIiwibmhzX251bWJlciI6Ijk2ODYzNjg5NzMiLCJpZGVudGl0eV9wcm9vZmluZ19sZXZlbCI6IlA5IiwiaWRfc3RhdHVzIjoidmVyaWZpZWQiLCJ0b2tlbl91c2UiOiJpZCIsInN1cm5hbWUiOiJNSUxMQVIiLCJmYW1pbHlfbmFtZSI6Ik1JTExBUiIsImJpcnRoZGF0ZSI6IjE5NjgtMDItMTIifQ.F_3YbSnLHaQrH2zfUJWP3cl-ovxKnYE-oRNblZnokLUmgW5hlda5SOOifvdmHi9j27b0RaVG0uWgeaOA51AY3up03HJUlW4ClCq6ZElRZU1OT3R-2dQYR8z9zbGm-t7z6uyRAqAs2DYNAhSOSKRwNxm7NZPm2XmCs27vEp5AQal9MLk1-obhVkxPIUDGjyzyawdIRttcFjP7F3drbP-j7w91tgqEJeYdDrvEci-7aBraee1N6XUhJqT24R21_fxdpksMxx4UTtCIS8Wsm3IbxVM6tckZQB7B6ldx-9rZiUFzb6jENyRdIWecsm05rUPklaLgcMJNsXIfSoz3Z0HsCQ",
        "scope": "openid profile profile_extended",
        "expires_in": 300,
        "token_type": "bearer",
        "refresh_token": "2d2a03bd-7d0e-4163-af76-19b9344fa8ae",
        "expires_at": 1743612244,
        "provider": "nhs-login",
        "type": "oidc",
        "providerAccountId": "49f470a1-cc52-49b7-beba-0f9cec937c46"
      });
    }
  ),

  http.get(
    "https://auth.sandpit.signin.nhs.uk/userinfo",
    async ({ request }) => {
      console.log("🟢 MSW handler hit: NHS access_token endpoint");
      const body = await request.text();
      console.log("🔍 Request body:", body);

      return HttpResponse.json({
        sub: '49f470a1-cc52-49b7-beba-0f9cec937c46',
        iss: 'https://auth.sandpit.signin.nhs.uk',
        aud: 'screening participant manager',
        nhs_number: '9686368973',
        birthdate: '1968-02-12',
        family_name: 'MILLAR',
        identity_proofing_level: 'P9',
        given_name: 'Mona'
      });
    }
  ),

  // GitHub user handler
  http.get("https://api.github.com/user", () => {
    console.log("🟢 MSW handler hit: GitHub user endpoint");
    return HttpResponse.json(user);
  }),
];

