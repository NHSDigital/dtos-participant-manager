import { NextResponse, NextRequest } from "next/server";
import { getAuthConfig } from "@/app/lib/auth";
import { cookies } from "next/headers";

export async function middleware(request: NextRequest) {
  const { auth, signOut } = await getAuthConfig();
  const session = await auth();

  if (!session) {
    return NextResponse.redirect(new URL("/access-denied", request.url));
  }

  // if (session.error === "RefreshTokenError") {
  //   await signOut();
  //   return NextResponse.redirect(new URL("/", request.url));
  // }

  if (session.error === "RefreshTokenError") {
    try {
      // const cookieStore = await cookies();
      // const csrfToken = cookieStore.get("__Host-authjs.csrf-token");

      const csrfResponse = await fetch(new URL("/api/auth/csrf", request.url));
      const { csrfToken } = await csrfResponse.json(); //WP - value is different from csrfToken

      // const params = new URLSearchParams();
      // params.append("csrfToken", JSON.stringify({ csrfToken }));

      await fetch(new URL("/api/auth/signout", request.url), {
        method: "POST",
        headers: {
          "Content-Type": "application/x-www-form-urlencoded",
          "X-Auth-Return-Redirect": "1",
        },
        body: new URLSearchParams({
          csrfToken,
          callback: "https://localhost:3000",
        }), //WP - MissingCSRF: CSRF token was missing during an action signout. Read more at https://errors.authjs.dev#missingcsrf
      });

      return NextResponse.redirect(new URL("/", request.url));
    } catch (error) {
      return NextResponse.redirect(new URL("/access-denied", request.url));
    }
  }

  return NextResponse.next();
}

export const config = {
  matcher: [
    "/bowel-screening/:path*",
    "/breast-screening/:path*",
    "/cervical-screening/:path*",
    "/screening/:path*",
  ], // Define protected routes,
};
