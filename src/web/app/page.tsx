export const dynamic = "force-dynamic";

import type { Metadata } from "next";
import type { Session } from "next-auth";
import { getAuthConfig } from "@/app/lib/auth";
import SignInButton from "@/app/components/signInButton";

export async function generateMetadata(): Promise<Metadata> {
   return {
    title: `${process.env.SERVICE_NAME} - NHS`,
  };
}

export default async function Home() {
  const { auth } = await getAuthConfig();
  const session = await auth();

  return (
    <>
      {!session?.user && (
        <main className="nhsuk-main-wrapper" id="maincontent" role="main">
          <div className="nhsuk-grid-row">
            <div className="nhsuk-grid-column-two-thirds">
              <h1>{process.env.SERVICE_NAME}</h1>
              <p>Use this service to see:</p>
              <ul>
                <li>what screening you are eligible for</li>
                <li>when your next appointment is due</li>
              </ul>

              <SignInButton />

              <p>
                By using this service you are agreeing to our{" "}
                <a href="#">terms of use</a> and <a href="#">privacy policy</a>.
              </p>
            </div>
          </div>
        </main>
      )}
    </>
  );
}
