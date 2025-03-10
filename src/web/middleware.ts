import { NextResponse, NextRequest } from "next/server";
import { getAuthConfig } from "@/app/lib/auth";

export async function middleware(request: NextRequest) {
  const { auth } = await getAuthConfig();
  const session = await auth();
  const isAuthenticated = session?.user;

  // If the user is authenticated, continue as normal
  if (isAuthenticated) {
    return NextResponse.redirect(new URL("/screening", request.url));
  }

  // Redirect to login page if not authenticated
  return NextResponse.next();
}

export const config = {
  matcher: "/",
};
