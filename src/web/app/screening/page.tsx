export const dynamic = "force-dynamic";

import type { Metadata } from "next";
import type { Session } from "next-auth";
import type { EligibilityItem, EligibilityResponse } from "@/app/types";
import { getAuthConfig } from "@/app/lib/auth";
import { fetchPatientScreeningEligibility } from "@/app/lib/fetchPatientData";
import { createUrlSlug } from "@/app/lib/utils";
import Card from "@/app/components/card";
import InsetText from "@/app/components/insetText";

export async function generateMetadata(): Promise<Metadata> {
  return {
    title: `My screening - ${process.env.SERVICE_NAME} - NHS`,
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
    return await fetchPatientScreeningEligibility(session);
  } catch (error) {
    console.error("Failed to get eligibility data:", error);
    return null;
  }
};

export default async function Page() {
  const { auth } = await getAuthConfig();
  const session = await auth();

  const eligibility = session?.user ? await getEligibility(session) : null;

  return (
    <main className="nhsuk-main-wrapper" id="maincontent" role="main">
      <div className="nhsuk-grid-row">
        <div className="nhsuk-grid-column-two-thirds">
          <h1>My screening</h1>

          {eligibility?.length ? (
            eligibility.map((item: EligibilityItem) => {
              const url = `${createUrlSlug(item.screeningName)}/${
                item.enrolmentId
              }`;
              return (
                <Card
                  key={item.enrolmentId}
                  title={item.screeningName}
                  url={url}
                />
              );
            })
          ) : (
            <InsetText text="You have no screening enrolments." />
          )}

          <p>
            Find out more information about{" "}
            <a href="https://www.nhs.uk/conditions/nhs-screening/">
              other screening done by the NHS
            </a>{" "}
            .
          </p>
        </div>
      </div>
    </main>
  );
}
