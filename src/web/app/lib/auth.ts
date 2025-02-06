import NextAuth, { Profile, User as NextAuthUser } from "next-auth";
import { OAuthConfig } from "next-auth/providers";

// Function to convert PEM to CryptoKey
async function pemToPrivateKey(): Promise<CryptoKey> {
  const pem = process.env.AUTH_NHSLOGIN_PRIVATE_KEY;

  if (!pem) {
    throw new Error("AUTH_NHSLOGIN_PRIVATE_KEY env variable is not defined");
  }

  // Remove headers and convert to binary
  const pemContents = pem
    .trim()
    .split("\n")
    .filter((line) => !line.includes("-----"))
    .join("");

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
    async jwt({ token, user, profile }) {
      if (user && profile) {
        token.name = `${profile.given_name} ${profile.family_name}`;
        token.firstName = profile.family_name;
        token.lastName = profile.surname;
        token.dob = profile.birthdate;
        token.nhsNumber = profile.nhs_number;
        token.identityLevel = profile.identity_proofing_level;
      }
      return token;
    },
    async session({ session, token }) {
      if (session.user) {
        const { name, firstName, lastName, dob, nhsNumber, identityLevel } =
          token;

        Object.assign(session.user, {
          name,
          firstName,
          lastName,
          dob,
          nhsNumber,
          identityLevel,
        });
      }
      return session;
    },
  },
});

declare module "next-auth" {
  interface User {
    firstName?: string;
    lastName?: string;
    dob?: string;
    nhsNumber?: number;
    identityLevel?: string;
  }
}
