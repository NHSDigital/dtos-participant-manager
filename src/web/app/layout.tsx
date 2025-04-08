import Header from "@/app/components/header";
import Footer from "@/app/components/footer";
import "@/app/globals.scss";

if (process.env.NEXT_PUBLIC_API_MOCKING == "enabled") {
  require("../oidc-mock/mocks"); // Corrected path
}

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  const serviceName = process.env.SERVICE_NAME;
  return (
    <html lang="en">
      <body>
        <Header serviceName={serviceName} />
        <div className="nhsuk-width-container">{children}</div>
        <Footer />
      </body>
    </html>
  );
}
