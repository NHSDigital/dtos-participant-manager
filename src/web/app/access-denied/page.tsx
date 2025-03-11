import type { Metadata } from "next";

export const metadata: Metadata = {
  title: `Prove who you are to manage your screening - ${process.env.SERVICE_NAME} - NHS`,
};

export default async function Page() {
  return (
    <main className="nhsuk-main-wrapper" id="maincontent" role="main">
      <div className="nhsuk-grid-row">
        <div className="nhsuk-grid-column-two-thirds">
          <p>You’ll need to prove who you are to see:</p>
          <ul>
            <li>what screening you are eligible for</li>
            <li>when your next screening is due</li>
          </ul>
          <p>
            Find you about{" "}
            <a href="https://help.login.nhs.uk/provewhoyouare">
              how to prove who you are
            </a>{" "}
            .
          </p>
        </div>
      </div>
    </main>
  );
}
