export interface DecodedToken {
  iss: string;
  aud: string;
  identity_proofing_level: string;
}

declare module "next-auth" {
  interface User {
    firstName?: string;
    lastName?: string;
    dob?: string;
    nhsNumber?: string;
    identityLevel?: string;
    accessToken?: string;
    expiresAt?: number;
    refreshToken?: string;
  }
  interface Session {
    error?: "RefreshTokenError"
  }
}
