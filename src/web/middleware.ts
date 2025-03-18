import { NextResponse, NextRequest } from "next/server";
import { getAuthConfig } from "@/app/lib/auth";

export async function middleware(request: NextRequest) {
  const { auth } = await getAuthConfig();
  const session = await auth();
  const isRootPath = request.nextUrl.pathname === "/";

  // Allow unauthenticated access to root path
  if (isRootPath) {
    if (session?.user) {
      return NextResponse.redirect(new URL("/screening", request.url));
    }
    return NextResponse.next();
  }

  // Protected routes require authentication
  if (!session?.user) {
    return NextResponse.redirect(new URL("/", request.url));
  }

  return NextResponse.next();
}

export const config = {
  matcher: [
    "/",
    "/bowel-screening/:path*",
    "/breast-screening/:path*",
    "/cervical-screening/:path*",
    "/screening/:path*",
  ],
};
