import type { Metadata } from "next";
import type { Session } from "next-auth";
import type { EligibilityItem, EligibilityResponse } from "@/app/types";
import { auth } from "@/app/lib/auth";
import { createUrlSlug } from "@/app/lib/utils";
import { fetchPatientScreeningEligibility } from "@/app/lib/fetchPatientData";
import BackLink from "@/app/components/backLink";
import Card from "@/app/components/card";
import InsetText from "@/app/components/insetText";
import SignInButton from "@/app/components/signInButton";
import SignOutButton from "@/app/components/signOutButton";

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

const getEligibility = async (
  session: Session | null
): Promise<EligibilityResponse | null> => {
  if (!session?.user?.accessToken) {
    console.log("No access token found for eligibility");
    return null;
  }

  try {
    return await fetchPatientScreeningEligibility(session.user.accessToken);
  } catch (error) {
    console.error("Failed to get eligibility data:", error);
    return null;
  }
};

export default async function Home() {
  const session = await auth();
  const eligibility = await getEligibility(session);

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

                {eligibility?.length ? (
                  eligibility.map((item: EligibilityItem) => {
                    const url = `${createUrlSlug(item.screeningName)}/${
                      item.assignmentId
                    }`;
                    return (
                      <Card
                        key={item.assignmentId}
                        title={item.screeningName}
                        url={url}
                      />
                    );
                  })
                ) : (
                  <InsetText text="You have no screening assignments." />
                )}

                <p>
                  Find out more information about{" "}
                  <a href="https://www.nhs.uk/conditions/nhs-screening/">
                    other screening done by the NHS
                  </a>
                  .
                </p>
                <hr />
                <p>
                  Logged in as {session.user.firstName} {session.user.lastName}{" "}
                  ({session.user.nhsNumber})
                </p>
                <SignOutButton />
              </div>
            </div>
          </main>
        </>
      )}
    </>
  );
}
