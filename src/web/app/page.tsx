import type { Metadata } from "next";
import { auth } from "@/app/lib/auth";
import SignInButton from "@/app/components/signInButton";
import SignOutButton from "@/app/components/signOutButton";
import BackLink from "@/app/components/backLink";
import Card from "@/app/components/card";

export async function generateMetadata(): Promise<Metadata> {
  const session = await auth();

  if (session?.user) {
    return {
      title: `My screening - ${process.env.SERVICE_NAME}`,
    };
  }

  return {
    title: `${process.env.SERVICE_NAME}`,
  };
}

export default async function Home() {
  const session = await auth();

  return (
    <>
      {!session?.user && (
        <main className="nhsuk-main-wrapper" id="maincontent" role="main">
          <div className="nhsuk-grid-row">
            <div className="nhsuk-grid-column-two-thirds">
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
            </div>
          </div>
        </main>
      )}

      {session?.user && (
        <>
          <BackLink url="/" />
          <main className="nhsuk-main-wrapper" id="maincontent" role="main">
            <div className="nhsuk-grid-row">
              <div className="nhsuk-grid-column-two-thirds">
                <h1>My screening</h1>
                <Card title="Breast screening" url="/breast-screening" />
                <p>
                  Find out more information about{" "}
                  <a href="https://www.nhs.uk/conditions/nhs-screening/">
                    other screening done by the NHS
                  </a>
                  .
                </p>
                <hr />
                <SignOutButton />
              </div>
            </div>
          </main>
        </>
      )}
    </>
  );
}
