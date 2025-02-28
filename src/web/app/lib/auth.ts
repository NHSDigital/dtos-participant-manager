import NextAuth, { Profile, User as NextAuthUser } from "next-auth";
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
      async jwt({ token, user, profile, account }) {
        if (user && profile) {
          token.name = `${profile.given_name} ${profile.family_name}`;
          token.firstName = profile.given_name;
          token.lastName = profile.family_name;
          token.dob = profile.birthdate;
          token.nhsNumber = profile.nhs_number;
          token.identityLevel = profile.identity_proofing_level;
          token.accessToken = account?.access_token;
        }
        return token;
      },
      async session({ session, token }) {
        if (session.user) {
          const {
            name,
            firstName,
            lastName,
            dob,
            nhsNumber,
            identityLevel,
            accessToken,
          } = token;

          Object.assign(session.user, {
            name,
            firstName,
            lastName,
            dob,
            nhsNumber,
            identityLevel,
            accessToken,
          });
        }
        return session;
      },
    },
    events: {
      async session({ session }) {
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
