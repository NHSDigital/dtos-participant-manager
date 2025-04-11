import type { Metadata } from "next";

export const metadata: Metadata = {
  title: `Authentication error - ${process.env.SERVICE_NAME} - NHS`,
};

export default async function Page() {
  return (
    <main className="nhsuk-main-wrapper" id="maincontent" role="main">
      <div className="nhsuk-grid-row">
        <div className="nhsuk-grid-column-two-thirds">
          <h1>Authentication error</h1>
          <p>There was a problem when trying to authenticate.</p>
        </div>
      </div>
    </main>
  );
}
