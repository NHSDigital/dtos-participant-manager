import type { Metadata } from "next";
import { auth } from "@/app/lib/auth";
import SignInButton from "@/app/components/signInButton";
import SignOutButton from "./components/signOutButton";
import { formatNhsNumber, formatDate } from "@/app/lib/utils";
import { fetchPatientData, fetchPatientScreeningEligibility } from "@/app/lib/fetchPatientData";

export const metadata: Metadata = {
  title: `${process.env.SERVICE_NAME}`,
};

export default async function Home() {
  const session = await auth();

  let participantId, name, dob, nhsNumber, pathwayTypeAssignments;
  let eligibility;

  if (session?.user) {
    try {
      if (session.user.nhsNumber) {
        const patient = await fetchPatientData(session.user.nhsNumber);
        ({ participantId, name, dob, nhsNumber, pathwayTypeAssignments } =
          patient);
      } else {
        console.log("No NHS number found");
      }
    } catch(error) {
      console.error("Failed to fetch patient data:", error);
    }

    try {
      if (session.user.accessToken) {
        const eligibilityResponse = await fetchPatientScreeningEligibility(session.user.accessToken);
        eligibility = eligibilityResponse;
      } else {
        console.log("No access token found");
      }
    } catch(error) {
      console.error("Failed to fetch eligibility data:", error);
    }
  }

  return (
    <main className="nhsuk-main-wrapper" id="maincontent" role="main">
      {!session?.user && (
        <>
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
        </>
      )}

      {session?.user && (
        <>
          <h1>
            Welcome {session.user.firstName} {session.user.lastName}
          </h1>
          <h2>Data from NHS login</h2>
          <p>
            Name: {session.user.firstName} {session.user.lastName}
          </p>
          <p>
            NHS number:{" "}
            {session.user.nhsNumber
              ? formatNhsNumber(session.user.nhsNumber)
              : ""}
          </p>
          <p>Date of birth: {formatDate(session.user.dob ?? "")}</p>
          <p>Identity level: {session.user.identityLevel}</p>
          <hr />
          <h2>Data from API</h2>
          <p>Participant ID: {participantId}</p>
          <p>Name: {name}</p>
          <p>Date of birth: {formatDate(dob ?? "")}</p>
          <p>NHS number: {formatNhsNumber(nhsNumber ?? "")}</p>
          <p>Pathway type assignments: {pathwayTypeAssignments ?? ""}</p>
          <hr />
          <h2>Data from Eligibility API</h2>
          <p>Eligibility: {eligibility ?? ""}</p>
          <SignOutButton />
        </>
      )}
    </main>
  );
}
