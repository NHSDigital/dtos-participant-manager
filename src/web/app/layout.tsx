import Header from "@/app/components/header";
import Footer from "@/app/components/footer";
import "@/app/globals.scss";

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
