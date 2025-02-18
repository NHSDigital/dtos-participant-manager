import Link from "next/link";

export default async function Unauthorised() {
  return (
    <main className="nhsuk-main-wrapper" id="maincontent" role="main">
      <div className="nhsuk-grid-row">
        <div className="nhsuk-grid-column-two-thirds">
          <h1>You are not authorised to view this page</h1>
          <p>
            <Link href="/">Return to the homepage</Link>
          </p>
        </div>
      </div>
    </main>
  );
}
