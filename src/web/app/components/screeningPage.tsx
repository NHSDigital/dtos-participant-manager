import type { Metadata } from "next";
import type { Session } from "next-auth";
import type { PathwayItem } from "@/app/types";
import { getAuthConfig } from "@/app/lib/auth";
import { fetchPathwayTypeEnrolment } from "@/app/lib/fetchPatientData";
import { logger } from "@/app/lib/logger";
import Breadcrumb from "@/app/components/breadcrumb";
import Card from "@/app/components/card";
import InsetText from "@/app/components/insetText";

interface ScreeningPageProps {
  screeningType: string;
}

export const generateMetadata = (screeningType: string): Metadata => ({
  title: `${screeningType} screening - ${process.env.SERVICE_NAME} - NHS`,
});

const getPathwayTypeEnrolment = async (
  session: Session | null,
  enrolmentId: string
): Promise<PathwayItem | null> => {
  if (!session?.user?.accessToken) {
    logger.warn(`No access token found for pathway type enrolment: ${enrolmentId}`);
    return null;
  }

  try {
    return await fetchPathwayTypeEnrolment(session, enrolmentId);
  } catch (error) {
    logger.error(
      ` Failed to get pathway type enrolment data for: ${enrolmentId}`,
      error
    );
    return null;
  }
};

export default async function ScreeningPage({
  screeningType,
  params,
}: ScreeningPageProps & { params: Promise<{ enrolmentId: string }> }) {
  const { auth } = await getAuthConfig();
  const session = await auth();
  const breadcrumbItems = [{ label: "Home", url: "/screening" }];
  const resolvedParams = await params;
  const enrolmentId = resolvedParams.enrolmentId;
  const pathwayTypeEnrolment = session?.user
    ? await getPathwayTypeEnrolment(session, enrolmentId)
    : null;

  return (
    <>
      <Breadcrumb items={breadcrumbItems} />
      <main className="nhsuk-main-wrapper" id="maincontent" role="main">
        <div className="nhsuk-grid-row">
          <div className="nhsuk-grid-column-two-thirds">
            <h1>{screeningType} screening</h1>
            {pathwayTypeEnrolment?.nextActionDate ? (
              <InsetText
                text={`Your next ${pathwayTypeEnrolment.screeningName} is due by`}
                date={pathwayTypeEnrolment.nextActionDate}
              />
            ) : (
              <InsetText text="You have no upcoming invitations." />
            )}
            {pathwayTypeEnrolment?.infoUrl && (
              <Card
                title={`About ${pathwayTypeEnrolment.screeningName}`}
                url={pathwayTypeEnrolment.infoUrl}
              />
            )}
          </div>
        </div>
      </main>
    </>
  );
}
