import { http, HttpResponse } from "msw";
import user from "../Data/user.json";
import accessToken from "../Data/access_token.json";
import authConfig from "../Data/auth_config.json";

export const handlers = [
  http.get(
    "https://auth.sandpit.signin.nhs.uk/.well-known/openid-configuration",
    () => {
      console.log("🟢 MSW handler hit: stubbed openid configuration endpoint");
      return HttpResponse.json(authConfig);
    }
  ),

  http.post("https://auth.sandpit.signin.nhs.uk/token", async () => {
    console.log("🟢 MSW handler hit: stubbed token endpoint");
    return HttpResponse.json(accessToken);
  }),

  http.get("https://auth.sandpit.signin.nhs.uk/userinfo", async () => {
    console.log("🟢 MSW handler hit: stubbed userinfo endpoint");
    return HttpResponse.json(user);
  }),
];
