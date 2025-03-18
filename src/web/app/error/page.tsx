import type { Metadata } from "next";

export const metadata: Metadata = {
  title: `There’s a problem with the service - ${process.env.SERVICE_NAME} - NHS`,
};

export default async function Page() {
  return (
    <main className="nhsuk-main-wrapper" id="maincontent" role="main">
      <div className="nhsuk-grid-row">
        <div className="nhsuk-grid-column-two-thirds">
          <h1>There’s a problem with the service</h1>
          <p>Try again later.</p>
        </div>
      </div>
    </main>
  );
}
