import type { Metadata } from "next";
import { getAuthConfig } from "@/app/lib/auth";
import { getEligibility } from "@/app/lib/getEligibility";
import ScreeningList from "@/app/components/screeningList";

export const metadata: Metadata = {
  title: `My screening - ${process.env.SERVICE_NAME} - NHS`,
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

          <ScreeningList eligibility={eligibility} />

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
