import type { Metadata } from "next";
import { auth } from "@/app/lib/auth";
import SignOutButton from "@/app/components/signOutButton";
import BackLink from "@/app/components/backLink";
import Card from "@/app/components/card";
import InsetText from "@/app/components/insetText";

export const metadata: Metadata = {
  title: `Breast screening - ${process.env.SERVICE_NAME}`,
};

export default async function Home() {
  const session = await auth();

  return (
    <>
      {session?.user && (
        <>
          <BackLink url="/" />
          <main className="nhsuk-main-wrapper" id="maincontent" role="main">
            <div className="nhsuk-grid-row">
              <div className="nhsuk-grid-column-two-thirds">
                <h1>Breast screening</h1>
                <InsetText
                  text="Your next breast screening invitation will be approximately"
                  date="October 2026"
                />
                <Card
                  title="About breast screening"
                  url="https://www.nhs.uk/conditions/breast-screening-mammogram/"
                />
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
