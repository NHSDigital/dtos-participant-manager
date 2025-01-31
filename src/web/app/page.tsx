import type { Metadata } from "next";
import { auth } from "@/app/lib/auth";
import SignInButton from "@/app/components/signInButton";
import SignOutButton from "./components/signOutButton";
import { formatNhsNumber, formatDate } from "@/app/lib/utils";

export const metadata: Metadata = {
  title: `Overview - ${process.env.SERVICE_NAME}`,
};

export default async function Home() {
  const session = await auth();

  return (
    <main className="nhsuk-main-wrapper" id="maincontent" role="main">
      {!session?.user && (
        <>
          <h1>{process.env.SERVICE_NAME}</h1>
          <p>Use this service to do something.</p>
          <p>You can use this service if you:</p>
          <ul>
            <li>live in England</li>
            <li>need to get a thing</li>
            <li>need to change a thing</li>
          </ul>

          <h2>Before you start</h2>

          <p>We'll ask you for: ...</p>

          <SignInButton />

          <p>
            By using this service you are agreeing to our{" "}
            <a href="#">terms of use</a> and <a href="#">privacy policy</a>.
          </p>
        </>
      )}

      {session?.user && (
        <>
          <h1>
            Welcome {session.user.firstName} {session.user.lastName}
          </h1>
          <p>
            NHS number:{" "}
            {session.user.nhsNumber
              ? formatNhsNumber(session.user.nhsNumber)
              : ""}
          </p>
          <p>Date of birth: {formatDate(session.user.dob ?? "")}</p>
          <p>Identity level: {session.user.identityLevel}</p>
          <SignOutButton />
        </>
      )}
    </main>
  );
}
