export const dynamic = "force-dynamic";

import type { Metadata } from "next";
import type { Session } from "next-auth";
import { getAuthConfig } from "@/app/lib/auth";
import Card from "@/app/components/card";
import InsetText from "@/app/components/insetText";
import SignOutButton from "@/app/components/signOutButton";
import Unauthorised from "@/app/components/unauthorised";
import { fetchPatientScreeningEligibility } from "@/app/lib/fetchPatientData";
import { createUrlSlug } from "@/app/lib/utils"
import type { EligibilityItem, EligibilityResponse } from "@/app/types";



export async function generateMetadata(): Promise<Metadata> {
  const { auth } = await getAuthConfig();
  const session = await auth();

  if (session?.user) {
    return {
      title: `My screening - ${process.env.SERVICE_NAME} - NHS`,
    };
  }

  return {
    title: `You are not authorised to view this page - ${process.env.SERVICE_NAME} - NHS`,
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

export default async function Page(props: {
  params: Promise<{ assignmentId: string }>;
}) {
  const { auth } = await getAuthConfig();
  const session = await auth();

  if (session?.error === "RefreshTokenError") {
    const { signIn } = await getAuthConfig();
    //await signIn("nhs-login");
  }

  if (!session?.user) return <Unauthorised />;

  const eligibility = session?.user ? await getEligibility(session) : null;


  return (
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
      )}
