import NextAuth, { Profile } from "next-auth";
import { jwtDecode } from "jwt-decode";
import { OAuthConfig } from "next-auth/providers";
import { DecodedToken } from "@/app/types/auth";

// Function to convert PEM to CryptoKey
async function pemToPrivateKey(): Promise<CryptoKey | null> {
  console.log("pemToPrivateKey");
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

async function generateClientAssertion(): Promise<string> {
  const privateKey = await pemToPrivateKey();
  if (!privateKey) throw new Error("Failed to load private key");

  const now = Math.floor(Date.now() / 1000);
  const payload = {
    iss: process.env.AUTH_NHSLOGIN_CLIENT_ID,
    sub: process.env.AUTH_NHSLOGIN_CLIENT_ID,
    aud: `${process.env.AUTH_NHSLOGIN_ISSUER_URL}/token`,
    jti: crypto.randomUUID(),
    exp: now + 300, // 5 minutes from now
    iat: now,
  };

  // Create the JWT header
  const header = { alg: "RS512", typ: "JWT" };

  // Encode header and payload
  const encodedHeader = Buffer.from(JSON.stringify(header)).toString(
    "base64url"
  );
  const encodedPayload = Buffer.from(JSON.stringify(payload)).toString(
    "base64url"
  );

  // Create signature input
  const signatureInput = `${encodedHeader}.${encodedPayload}`;

  // Sign the input
  const signature = await crypto.subtle.sign(
    { name: "RSASSA-PKCS1-v1_5", hash: "SHA-512" },
    privateKey,
    new TextEncoder().encode(signatureInput)
  );

  // Encode signature and create final JWT
  const encodedSignature = Buffer.from(signature).toString("base64url");
  return `${signatureInput}.${encodedSignature}`;
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
        } else if (Date.now() == token.expiresAt * 1000) {
          return token;
        } else {
          try {
            const clientAssertion = await generateClientAssertion();

            const requestBody = {
              grant_type: "refresh_token",
              refresh_token: token.refreshToken as string,
              client_assertion_type:
                "urn:ietf:params:oauth:client-assertion-type:jwt-bearer",
              client_assertion: clientAssertion,
            };

            const response = await fetch(
              `${process.env.AUTH_NHSLOGIN_ISSUER_URL}/token`,
              {
                method: "POST",
                body: new URLSearchParams(requestBody),
              }
            );
            console.log(response);

            const tokensOrError = await response.json();
            console.log(tokensOrError);

            if (!response.ok) throw tokensOrError;

            const newTokens = tokensOrError as {
              access_token: string;
              expires_in: number;
              refreshToken?: string;
            };

            return {
              ...token,
              accessToken: newTokens.access_token,
              expiresAt: Math.floor(Date.now() / 1000 + newTokens.expires_in),
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
      },
    },
    pages: {
      signIn: "/",
      error: "/access-denied",
    },
  });
}
