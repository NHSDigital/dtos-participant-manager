export const dynamic = "force-dynamic";

import type { Metadata } from "next";
import type { Session } from "next-auth";
import type { PathwayItem } from "@/app/types";
import { getAuthConfig } from "@/app/lib/auth";
import { fetchPathwayEnrolment } from "@/app/lib/fetchPatientData";
import Breadcrumb from "@/app/components/breadcrumb";
import Card from "@/app/components/card";
import InsetText from "@/app/components/insetText";
import SignOutButton from "@/app/components/signOutButton";
import Unauthorised from "@/app/components/unauthorised";

export async function generateMetadata(): Promise<Metadata> {
  const { auth } = await getAuthConfig();
  const session = await auth();

  if (session?.user) {
    return {
      title: `Cervical screening - ${process.env.SERVICE_NAME}`,
    };
  }

  return {
    title: `You are not authorised to view this page - ${process.env.SERVICE_NAME}`,
  };
}

const getPathwayEnrolment = async (
  session: Session | null,
  enrolmentId: string
): Promise<PathwayItem | null> => {
  if (!session?.user?.accessToken) {
    console.log("No access token found for pathway enrolment");
    return null;
  }

  try {
    return await fetchPathwayEnrolment(session.user.accessToken, enrolmentId);
  } catch (error) {
    console.error("Failed to get pathway enrolment data:", error);
    return null;
  }
};

export default async function Page(props: {
  params: Promise<{ enrolmentId: string }>;
}) {
  const { auth } = await getAuthConfig();
  const session = await auth();

  if (!session?.user) return <Unauthorised />;

  const breadcrumbItems = [{ label: "Home", url: "/" }];
  const params = await props.params;
  const enrolmentId = params.enrolmentId;
  const pathwayEnrolment = session?.user
    ? await getPathwayEnrolment(session, enrolmentId)
    : null;

  return (
    <>
      <Breadcrumb items={breadcrumbItems} />
      <main className="nhsuk-main-wrapper" id="maincontent" role="main">
        <div className="nhsuk-grid-row">
          <div className="nhsuk-grid-column-two-thirds">
            <h1>Cervical screening</h1>
            {pathwayEnrolment?.nextActionDate ? (
              <InsetText
                text={`Your next ${pathwayEnrolment.screeningName} invitation will be approximately`}
                date={pathwayEnrolment.nextActionDate}
              />
            ) : (
              <InsetText text="You have no upcoming breast screening invitations." />
            )}
            {pathwayEnrolment?.infoUrl && (
              <Card
                title={`About ${pathwayEnrolment.screeningName}`}
                url={pathwayEnrolment.infoUrl}
              />
            )}
            {session.user && (
              <>
                <hr />
                <p>
                  Logged in as {session.user?.firstName}{" "}
                  {session.user?.lastName} ({session.user?.nhsNumber})
                </p>
                <SignOutButton />
              </>
            )}
          </div>
        </div>
      </main>
    </>
  );
}
