import type { Metadata } from "next";
import type { Session } from "next-auth";
import type { PathwayItem } from "@/app/types";
import { getAuthConfig } from "@/app/lib/auth";
import { fetchPathwayAssignment } from "@/app/lib/fetchPatientData";
import Breadcrumb from "@/app/components/breadcrumb";
import Card from "@/app/components/card";
import InsetText from "@/app/components/insetText";
import UserProfile from "@/app/components/userProfile";

interface ScreeningPageProps {
  screeningType: string;
  metadataTitle: string;
}

export const generateMetadata = (screeningType: string): Metadata => ({
  title: `${screeningType} screening - ${process.env.SERVICE_NAME} - NHS`,
});

const getPathwayAssignment = async (
  session: Session | null,
  assignmentId: string
): Promise<PathwayItem | null> => {
  if (!session?.user?.accessToken) {
    console.log("No access token found for pathway assignment");
    return null;
  }

  try {
    return await fetchPathwayAssignment(session.user.accessToken, assignmentId);
  } catch (error) {
    console.error("Failed to get pathway assignment data:", error);
    return null;
  }
};

export default async function ScreeningPage({
  screeningType,
  params,
}: ScreeningPageProps & { params: Promise<{ assignmentId: string }> }) {
  const { auth } = await getAuthConfig();
  const session = await auth();
  const breadcrumbItems = [{ label: "Home", url: "/screening" }];
  const resolvedParams = await params;
  const assignmentId = resolvedParams.assignmentId;
  const pathwayAssignment = session?.user
    ? await getPathwayAssignment(session, assignmentId)
    : null;

  return (
    <>
      <Breadcrumb items={breadcrumbItems} />
      <main className="nhsuk-main-wrapper" id="maincontent" role="main">
        <div className="nhsuk-grid-row">
          <div className="nhsuk-grid-column-two-thirds">
            <h1>{screeningType} screening</h1>
            {pathwayAssignment?.nextActionDate ? (
              <InsetText
                text={`Your next ${pathwayAssignment.screeningName} is due by`}
                date={pathwayAssignment.nextActionDate}
              />
            ) : (
              <InsetText text="You have no upcoming invitations." />
            )}
            {pathwayAssignment?.infoUrl && (
              <Card
                title={`About ${pathwayAssignment.screeningName}`}
                url={pathwayAssignment.infoUrl}
              />
            )}
            {session?.user && (
              <UserProfile
                firstName={session.user?.firstName}
                lastName={session.user?.lastName}
                nhsNumber={session.user?.nhsNumber}
              />
            )}
          </div>
        </div>
      </main>
    </>
  );
}
