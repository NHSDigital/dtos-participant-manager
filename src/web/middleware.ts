import { NextResponse, NextRequest } from "next/server";
import { getAuthConfig } from "@/app/lib/auth";

export async function middleware(request: NextRequest) {
  const { auth } = await getAuthConfig();
  const session = await auth();

  if (!session) {
    //Prevents people accessing protected pages without a session
    //As well as those where the access token refresh failed
    return NextResponse.redirect(new URL("/", request.url));
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
