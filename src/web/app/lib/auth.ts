import NextAuth, { Profile } from "next-auth";
import { jwtDecode } from "jwt-decode";
import { OAuthConfig } from "next-auth/providers";
import { DecodedToken } from "@/app/types/auth";

// Function to convert PEM to CryptoKey
async function pemToPrivateKey(): Promise<CryptoKey | null> {
  const pem = process.env.AUTH_NHSLOGIN_CLIENT_SECRET;

  if (!pem) {
    console.warn("Could not get secret from Azure Key Vault");
    return null;
  }

  // Remove headers and convert to binary
  const pemContents = pem.replace(/[\r\n\s]+|-{5}[A-Z\s]+?-{5}/g, "").trim();

  // Convert base64 to buffer
  const keyBuffer = Buffer.from(pemContents, "base64");

  // Import as CryptoKey
  return await crypto.subtle.importKey(
    "pkcs8",
    keyBuffer,
    {
      name: "RSASSA-PKCS1-v1_5",
      hash: "SHA-512",
    },
    true,
    ["sign"]
  );
}

// Function to retrieve the client private key dynamically
async function getClientPrivateKey(): Promise<CryptoKey | null> {
  return await pemToPrivateKey();
}

// Function to configure NHS Login dynamically
async function getNhsLoginConfig(): Promise<OAuthConfig<Profile>> {
  const clientPrivateKey = await getClientPrivateKey();

  return {
    id: "nhs-login",
    name: "NHS login authentication",
    type: "oidc",
    issuer: `${process.env.AUTH_NHSLOGIN_ISSUER_URL}`,
    wellKnown: `${process.env.AUTH_NHSLOGIN_ISSUER_URL}/.well-known/openid-configuration`,
    clientId: process.env.AUTH_NHSLOGIN_CLIENT_ID,
    authorization: {
      params: {
        scope: "openid profile profile_extended",
      },
    },
    idToken: true,
    client: {
      token_endpoint_auth_method: "private_key_jwt",
      userinfo_signed_response_alg: "RS512",
    },
    token: {
      clientPrivateKey: clientPrivateKey,
    },
    checks: [],
  };
}

// Function to initialize NextAuth dynamically
export async function getAuthConfig() {
  const nhsLoginConfig = await getNhsLoginConfig();

  return NextAuth({
    debug: true,
    providers: [nhsLoginConfig],
    session: {
      strategy: "jwt",
      maxAge: 1800, // 30 minutes [Recommended by NHS login]
    },
    callbacks: {
      async signIn({ account }) {
        if (!account || typeof account.id_token !== "string") {
          return false;
        }

        const decodedToken = jwtDecode<DecodedToken>(account.id_token);
        const AUTH_ISSUER_URL = process.env.AUTH_NHSLOGIN_ISSUER_URL;
        const AUTH_CLIENT_ID = process.env.AUTH_NHSLOGIN_CLIENT_ID;

        const { iss, aud, identity_proofing_level } = decodedToken;

        const isValidToken =
          iss === AUTH_ISSUER_URL &&
          aud === AUTH_CLIENT_ID &&
          identity_proofing_level === "P9";

        return isValidToken;
      },
      async jwt({ token, account, profile }) {
        if (account && profile) {
           return {
            ...token,
            accessToken: account.access_token,
            expiresAt: account.expires_at,
            refreshToken: account.refresh_token,
            firstName: profile.given_name,
            lastName: profile.family_name,
            nhsNumber: profile.nhs_number,
            identityLevel: profile.identity_proofing_level,
          };
        } else if (Date.now() < token.expiresAt * 1000) {
          console.log(`Token is still valid`);
          console.log(token);
          return token;
        } else {
          try {
            const response = await fetch(
              `${process.env.AUTH_NHSLOGIN_ISSUER_URL}/token`,
              {
                method: "POST",
                body: new URLSearchParams({
                  client_id: process.env.AUTH_NHSLOGIN_CLIENT_ID!,
                  client_secret: process.env.AUTH_NHSLOGIN_CLIENT_SECRET!,
                  grant_type: "refresh_token",
                  refresh_token: token.refresh_token as string,
                }),
              }
            );

            const tokensOrError = await response.json();

            if (!response.ok) throw tokensOrError;

            const newTokens = tokensOrError as {
              accessToken: string;
              expiresIn: number;
              refreshToken?: string;
            };

            return {
              ...token,
              accessToken: newTokens.accessToken,
              expiresAt: Math.floor(Date.now() / 1000 + newTokens.expiresIn),
              refreshToken: newTokens.refreshToken
                ? newTokens.refreshToken
                : token.refreshToken,
            };
          } catch (error) {
            console.error("Error refreshing access_token", error);
            token.error = "RefreshTokenError";
            return token;
          }
        }
      },
      async session({ session, token }) {
        if (session.user) {
          const {
            firstName,
            lastName,
            nhsNumber,
            identityLevel,
            accessToken,
            refreshToken,
            expiresAt,
          } = token;

          Object.assign(session.user, {
            firstName,
            lastName,
            nhsNumber,
            identityLevel,
            accessToken,
            refreshToken,
            expiresAt,
          });
        }
        session.error = token.error;
        return session;
      },
    },
    events: {
      async session({ session, token }) {
        const maxAge = 1800; // 30 minutes [Recommended by NHS login]
        const now = Math.floor(Date.now() / 1000);
        session.expires = new Date((now + maxAge) * 1000).toISOString();
        console.log("Session - ", session);
        console.log("Session expires at:", session.user?.expiresAt);
      },
    },
    pages: {
      signIn: "/",
      error: "/access-denied",
    },
  });
}
