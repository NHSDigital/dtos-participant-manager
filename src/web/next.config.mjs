/** @type {import('next').NextConfig} */
const nextConfig = {
  output: "standalone",
  serverExternalPackages: ["pino", "pino-pretty"],
  sassOptions: {
    quietDeps: true,
    includePaths: ["./node_modules/nhsuk-frontend"],
    silenceDeprecations: ["legacy-js-api"],
  },
};

export default nextConfig;
