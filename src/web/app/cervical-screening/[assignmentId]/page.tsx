import type { Metadata } from "next";
import type { Session } from "next-auth";
import type { PathwayItem } from "@/app/types";
import { auth } from "@/app/lib/auth";
import { fetchPathwayAssignment } from "@/app/lib/fetchPatientData";
import Breadcrumb from "@/app/components/breadcrumb";
import Card from "@/app/components/card";
import InsetText from "@/app/components/insetText";
import SignOutButton from "@/app/components/signOutButton";
import Unauthorised from "@/app/components/unauthorised";

export async function generateMetadata(): Promise<Metadata> {
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

export default async function Page(props: {
  params: Promise<{ assignmentId: string }>;
}) {
  const session = await auth();

  if (!session) return <Unauthorised />;

  const breadcrumbItems = [{ label: "Home", url: "/" }];
  const params = await props.params;
  const assignmentId = params.assignmentId;
  const eligibility = await getPathwayAssignment(session, assignmentId);

  return (
    <>
      <Breadcrumb items={breadcrumbItems} />
      <main className="nhsuk-main-wrapper" id="maincontent" role="main">
        <div className="nhsuk-grid-row">
          <div className="nhsuk-grid-column-two-thirds">
            <h1>Cervical screening</h1>
            {eligibility?.nextActionDate ? (
              <InsetText
                text={`Your next ${eligibility.screeningName} invitation will be approximately`}
                date={eligibility.nextActionDate}
              />
            ) : (
              <InsetText text="You have no upcoming breast screening invitations." />
            )}
            {eligibility?.infoUrl && (
              <Card
                title={`About ${eligibility.screeningName}`}
                url={eligibility.infoUrl}
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
