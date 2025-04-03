import { http, HttpResponse } from "msw";
import user from "../Data/user.json";
import accessToken from "../Data/access_token.json";

export const handlers = [
  // Authentication handler (mocking NHS token exchange)
  http.get("https://auth.sandpit.signin.nhs.uk/.well-known/openid-configuration", () => {
    console.log("🟢 MSW handler hit: GitHub user endpoint");
    return HttpResponse.json({"issuer": "https://auth.sandpit.signin.nhs.uk", "authorization_endpoint": "https://localhost:3001/api/auth/callback/nhs-login?code=fb66c0ac-38ec-4edb-b204-90623968a3e8", "token_endpoint": "https://auth.sandpit.signin.nhs.uk/token", "jwks_uri": "https://auth.sandpit.signin.nhs.uk/.well-known/jwks.json", "scopes_supported": ["basic_demographics", "client_metadata", "email", "gp_integration_credentials", "gp_registration_details", "landline", "nhs_app_credentials", "openid", "phone", "profile", "profile_extended"], "response_types_supported": ["code"], "subject_types_supported": ["public"], "id_token_signing_alg_values_supported": ["RS512"], "claims_supported": ["iss", "aud", "sub", "family_name", "given_name", "email", "email_verified", "phone_number", "phone_number_verified", "landline_number", "landline_number_verified", "birthdate", "nhs_number", "gp_integration_credentials", "delegations", "gp_registration_details", "im1_token", "auth_time", "jti", "nonce", "vot", "vtm", "exp", "ods_code", "user_id", "linkage_key", "surname", "iat", "gp_ods_code", "gp_user_id", "gp_linkage_key", "client_user_metadata", "identity_proofing_level"], "userinfo_endpoint": "https://auth.sandpit.signin.nhs.uk/userinfo", "token_endpoint_auth_signing_alg_values_supported": ["RS512"], "token_endpoint_auth_methods_supported": ["private_key_jwt"], "fido_uaf_authentication_request_endpoint": "https://uaf.sandpit.signin.nhs.uk/authRequest", "fido_uaf_registration_request_endpoint": "https://uaf.sandpit.signin.nhs.uk/regRequest", "fido_uaf_registration_response_endpoint": "https://uaf.sandpit.signin.nhs.uk/regResponse", "fido_uaf_deregistration_endpoint": "https://uaf.sandpit.signin.nhs.uk/dereg"}
    );
  }),

  http.post(
    "https://auth.sandpit.signin.nhs.uk/token",
    async ({ request }) => {
      console.log("🟢 MSW handler hit: NHS access_token endpoint");
      const body = await request.text();
      console.log("🔍 Request body:", body);

      return HttpResponse.json({
        "access_token": "eyJraWQiOiIyOWY2Yjg5NS1hZmM4LTQ4YzgtYjNmNC1hOTE2ZDA4ZjFhZjAiLCJhbGciOiJSUzUxMiJ9.ew0KICAiaXNzIjogImh0dHBzOi8vYXV0aC5zYW5kcGl0LnNpZ25pbi5uaHMudWsiLA0KICAic3ViIjogIjQ5ZjQ3MGExLWNjNTItNDliNy1iZWJhLTBmOWNlYzkzN2M0NiIsDQogICJhdWQiOiAic2NyZWVuaW5nIHBhcnRpY2lwYW50IG1hbmFnZXIiLA0KICAiaWF0IjogMzI1MDM2ODAwMDAsDQogICJ2dG0iOiAiaHR0cHM6Ly9hdXRoLnNhbmRwaXQuc2lnbmluLm5ocy51ay90cnVzdG1hcmsvYXV0aC5zYW5kcGl0LnNpZ25pbi5uaHMudWsiLA0KICAiYXV0aF90aW1lIjogMTc0MzY3NTAxOCwNCiAgInZvdCI6ICJQOS5DcC5DZCIsDQogICJleHAiOiAzMjUwMzY4MDAwMCwNCiAgImp0aSI6ICIzYjM2NzEyOC1jYWVhLTRhYmUtYTljOC03ZjAzZWFjYWMyNmMiLA0KICAibmhzX251bWJlciI6ICI5Njg2MzY4OTczIiwNCiAgInZlcnNpb24iOiAwLA0KICAiY2xpZW50X2lkIjogInNjcmVlbmluZyBwYXJ0aWNpcGFudCBtYW5hZ2VyIiwNCiAgInJlYXNvbl9mb3JfcmVxdWVzdCI6ICJwYXRpZW50YWNjZXNzIiwNCiAgInRva2VuX3VzZSI6ICJhY2Nlc3MiLA0KICAicmVxdWVzdGluZ19wYXRpZW50IjogIjk2ODYzNjg5NzMiLA0KICAic2NvcGUiOiAib3BlbmlkIHByb2ZpbGUgcHJvZmlsZV9leHRlbmRlZCINCn0.iLAeGYcheB0NwpUXqmm6wfE2118tF0okD2RsqFw9F-E79byMerFqI6p4fa5eyds1k0ALQ6Dj6Xvi_XGrX3_FVxMAjPvhAIwHi87CfLuui6qfZ7DbkaUC2kxr0Pft84SFay_-cI_zJ4YFedcie2mwVkfPfBurUHMW4qo6CLOPPHcbGmT3GhSV1_SBqPe9H4I7n3qQnxb0-JvRGl-FY4AQSwr6j9LNAtRIlACnwoqhviCA1e6K47ZxORFGwDj14G6iQwjP8FgIo_wkZv3uOkJzBWbBQ5hkmyfeqPgOgfEovg3YhAEuYdf8FguRcd9ixfTvng_pEeAtD0URT0eP-T_TtQ",
        "id_token": "eyJraWQiOiJhNTk1M2MwOC1iNjJjLTQ3ZmEtODJmNS1iMmYzYzI2OTBhNjYiLCJhbGciOiJSUzUxMiJ9.ew0KICAiaXNzIjogImh0dHBzOi8vYXV0aC5zYW5kcGl0LnNpZ25pbi5uaHMudWsiLA0KICAic3ViIjogIjQ5ZjQ3MGExLWNjNTItNDliNy1iZWJhLTBmOWNlYzkzN2M0NiIsDQogICJhdWQiOiAic2NyZWVuaW5nIHBhcnRpY2lwYW50IG1hbmFnZXIiLA0KICAiaWF0IjogMzI1MDM2ODAwMDAsDQogICJ2dG0iOiAiaHR0cHM6Ly9hdXRoLnNhbmRwaXQuc2lnbmluLm5ocy51ay90cnVzdG1hcmsvYXV0aC5zYW5kcGl0LnNpZ25pbi5uaHMudWsiLA0KICAiYXV0aF90aW1lIjogMTc0MzY3NTAxOCwNCiAgInZvdCI6ICJQOS5DcC5DZCIsDQogICJleHAiOiAzMjUwMzY4MDAwMCwNCiAgImp0aSI6ICJhNDVkMDlkMy03MGFmLTQ3NmYtOTc1Ni00YjEwZDc3MWFkYWIiLA0KICAibmhzX251bWJlciI6ICI5Njg2MzY4OTczIiwNCiAgImlkZW50aXR5X3Byb29maW5nX2xldmVsIjogIlA5IiwNCiAgImlkX3N0YXR1cyI6ICJ2ZXJpZmllZCIsDQogICJ0b2tlbl91c2UiOiAiaWQiLA0KICAic3VybmFtZSI6ICJNSUxMQVIiLA0KICAiZmFtaWx5X25hbWUiOiAiTUlMTEFSIiwNCiAgImJpcnRoZGF0ZSI6ICIxOTY4LTAyLTEyIg0KfQ.logabbwnEtrHZ-zcfBcz2dmMTGtrWFCJsGcyac_q_5PiaalBgXVLnw1_51gjsrY-mv9jyxsxPQSnUOB3u3nXM5LHApT4SMrgenjohPUg2rz4glaCs2MgBP1P5umMmyN3iSDAWBR9nJ03o7nTvra8qy7oKSUIVTXXwwifp0492UWAgtrCenCu0s-0cC0QmNJUdz8LY_xh9Ce392zNTYqw7nhbGqEDiVMr5ng2pe9Fv5tura7c2y8XdyNOF_6EnB7Z6GQGuJBcfIotT20v2ecCfuO7HAOiTDSzEypZ7JwDGymIJuR9jw0wCXuA6QcUlcNsyTbpGRZpI55HgPea5ta6hA",
        "scope": "openid profile profile_extended",
        "expires_in": 300,
        "token_type": "bearer",
        "refresh_token": "f336a4a7-d7ef-493f-ba7c-6fb6833f13a3",
        "expires_at": 1743675323,
        "provider": "nhs-login",
        "type": "oidc",
        "providerAccountId": "49f470a1-cc52-49b7-beba-0f9cec937c46"
      });
    }
  ),

  http.get(
    "https://auth.sandpit.signin.nhs.uk/userinfo",
    async ({ request }) => {
      console.log("🟢 MSW handler hit: NHS user info endpoint");
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

