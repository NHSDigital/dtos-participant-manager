import { NextRequest } from "next/server";
import { getAuthConfig } from "@/app/lib/auth";

export async function GET(req: NextRequest): Promise<Response> {
  const { handlers } = await getAuthConfig();
  return handlers.GET(req);
}

export async function POST(req: NextRequest): Promise<Response> {
  const { handlers } = await getAuthConfig();
  return handlers.POST(req);
}
