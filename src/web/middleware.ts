import { NextResponse, NextRequest } from "next/server";
import { getAuthConfig } from "@/app/lib/auth";

export async function middleware(request: NextRequest) {
  const { auth } = await getAuthConfig();
  const session = await auth();
  const isAuthenticated = session?.user;

  // If the user is authenticated, continue as normal
  if (!session) {
    return NextResponse.redirect(new URL("/access-denied", request.url));
  }

  return NextResponse.next(); // Allow request to continue

}

export const config = {
  matcher: ["/bowel-screening/:path*", "/breast-screening/:path*", "/cervical-screening/:path*" , "/screening/:path*"], // Define protected routes,
};
