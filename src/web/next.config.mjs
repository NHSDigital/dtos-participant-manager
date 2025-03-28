/** @type {import('next').NextConfig} */
const nextConfig = {
  output: "standalone",
  serverExternalPackages: ["pino", "pino-pretty"],
  sassOptions: {
    quietDeps: true,
    includePaths: ["./node_modules/nhsuk-frontend"],
  },
};

export default nextConfig;
