import type { Metadata } from "next";

export const metadata: Metadata = {
  title: `Access denied - ${process.env.SERVICE_NAME}`,
};

export default async function Page() {
  return (
    <>
      <main className="nhsuk-main-wrapper" id="maincontent" role="main">
        <div className="nhsuk-grid-row">
          <div className="nhsuk-grid-column-two-thirds">
            <h1>Access denied</h1>
            <p>Sorry you don't have access to this service.</p>
            <p>This may be for the following reasons:</p>
            <ul>
              <li>You do not have P9 identity level</li>
              <li>...</li>
              <li>...</li>
            </ul>
          </div>
        </div>
      </main>
    </>
  );
}
