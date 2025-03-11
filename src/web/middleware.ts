import { NextResponse, NextRequest } from "next/server";
import { getAuthConfig } from "@/app/lib/auth";

export async function middleware(request: NextRequest) {
  const { auth } = await getAuthConfig();
  const session = await auth();

  if (!session) {
    return NextResponse.redirect(new URL("/access-denied", request.url));
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
