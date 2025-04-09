/** @type {import('next').NextConfig} */
const nextConfig = {
  output: "standalone",
  serverExternalPackages: ["pino", "pino-pretty"],
  sassOptions: {
    quietDeps: true,
    includePaths: ["./node_modules/nhsuk-frontend"],
  },
  webpack: (config) => {
    config.externals = [
      ...(config.externals || []),
      "require-in-the-middle",
      "@azure/functions-core",
    ];
    return config;
  },
};

export default nextConfig;
