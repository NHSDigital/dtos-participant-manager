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
        "access_token": "XeFKw71aKiVMTAml8MNbk_ZLiqk",
        "refresh_token":"GujHL12JL2wk86xNdf1McD3nty4",
        "scope":"openid profile",
        "id_token":"eyJ0eXAiOiJKV1QiLCJraWQiOiJiL082T3ZWdjEreStXZ3JINVVpOVdUaW9MdDA9IiwiYWxnIjoiUlMyNTYifQ.ewogICJhdF9oYXNoIjogIjZUcEZsS3BRd1ZJT3RVTTZqcUJtWWciLAogICJzdWIiOiAiOTk5OTk5OTk5OTk5IiwKICAiYXVkaXRUcmFja2luZ0lkIjogImIwYzhiNWJhLWE0OWItNDhmYi1hZGJmLTVjYmU1NmUzM2YwZS0zMTY4MjgiLAogICJpc3MiOiAiaHR0cHM6Ly9hbS5uaHNkZXYucHRsLm5oc2QtZXNhLm5ldDo0NDMvb3BlbmFtL29hdXRoMi9yZWFsbXMvcm9vdC9yZWFsbXMvb2lkYyIsCiAgInRva2VuTmFtZSI6ICJpZF90b2tlbiIsCiAgImF1ZCI6ICI5OTk5OTk5OTk5OTkuYXBwcy5uYXRpb25hbCIsCiAgImNfaGFzaCI6ICIxdjZ1WUdjUU55OW5mU05KUnRPRjB3IiwKICAiYWNyIjogIkFBTDFfVVNFUlBBU1MiLAogICJvcmcuZm9yZ2Vyb2NrLm9wZW5pZGNvbm5lY3Qub3BzIjogInQyRnJaVEREXzU2RUJGZThidDB0QVpuLWQ5ZyIsCiAgInNfaGFzaCI6ICJ4M1hudDFmdDVqRE5DcUVSTzlFQ1pnIiwKICAiYXpwIjogIjk5OTk5OTk5OTk5OS5hcHBzLm5hdGlvbmFsIiwKICAiYXV0aF90aW1lIjogMTU4OTgxNjQ4OCwKICAicmVhbG0iOiAiL29pZGMiLAogICJleHAiOiAxNTg5ODIwODQ0LAogICJ0b2tlblR5cGUiOiAiSldUVG9rZW4iLAogICJpYXQiOiAxNTg5ODE3MjQ0Cn0=.iMllAqigJ2o1Iep2o65P8xESjEtNCid4j4bUNfxDcYkuXEkCjXXJScpyL80CEK3oOYCDZXy6vRCcYRn2gkglJz4_QFnP2l8SnIKsUUgL99uWTPqC7Rjtk6l0mrehSRCWqp3lpPLSzyThx484cGjgptkd_UvV5mi77VjvmBs3yVBdvW_l0iQXbrvvIsCjoCikTvK90OAhnPwF5aq36xKttXrgysdFiwkwJzVdBv_OrUYwuO9MJTvScbiDJpZj7P_DZ1e8xrhuGDHt9SpLLCLy_Ewq5ZAlb7cA7QdxTu9C3GtBQm3O-KgUgfouHHzMvcJ-mtbWNJMF-ZKGdbC4Pi7A",
         "token_type":"Bearer",
         "expires_in":3600
       });
    }
  ),

  http.post(
    "https://auth.sandpit.signin.nhs.uk/userinfo",
    async ({ request }) => {
      console.log("🟢 MSW handler hit: NHS access_token endpoint");
      const body = await request.text();
      console.log("🔍 Request body:", body);

      return HttpResponse.json({
        "sub": "999999999999",
        "name": "Smith Jane Ms",
        "given_name": "Jane",
        "family_name": "Smith"
       });
    }
  ),

  // GitHub user handler
  http.get("https://api.github.com/user", () => {
    console.log("🟢 MSW handler hit: GitHub user endpoint");
    return HttpResponse.json(user);
  }),
];

