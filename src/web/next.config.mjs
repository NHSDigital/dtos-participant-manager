/** @type {import('next').NextConfig} */
const nextConfig = {
  output: "standalone",
  experimental: {
    serverComponentsExternalPackages: ["pino", "pino-pretty"],
    instrumentationHook: true,
  },
};

export default nextConfig;
