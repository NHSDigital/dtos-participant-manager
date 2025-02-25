import NextAuth, { Profile, User as NextAuthUser } from "next-auth";
import { OAuthConfig } from "next-auth/providers";

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
  const privateKey = await crypto.subtle.importKey(
    "pkcs8",
    keyBuffer,
    {
      name: "RSASSA-PKCS1-v1_5",
      hash: "SHA-512",
    },
    true,
    ["sign"]
  );

  return privateKey;
}

// Convert PEM to CryptoKey
const clientPrivateKey = await pemToPrivateKey();


const NHS_LOGIN: OAuthConfig<Profile> = {
  id: "nhs-login",
  name: "NHS login authentication",
  type: "oidc",
  issuer: `${process.env.AUTH_NHSLOGIN_ISSUER_URL}`,
  wellKnown: `${process.env.AUTH_NHSLOGIN_ISSUER_URL}/.well-known/openid-configuration`,
  clientId: process.env.AUTH_NHSLOGIN_CLIENT_ID,
  authorization: {
    params: {
      scope: "openid profile email basic_demographics profile_extended",
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

export const { handlers, signIn, signOut, auth } = NextAuth({
  providers: [NHS_LOGIN],
  callbacks: {
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
  pages: {
    signIn: "/",
  },
});

declare module "next-auth" {
  interface User {
    firstName?: string;
    lastName?: string;
    dob?: string;
    nhsNumber?: string;
    identityLevel?: string;
    accessToken?: string;
  }
}
