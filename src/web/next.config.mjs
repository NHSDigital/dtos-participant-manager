/** @type {import('next').NextConfig} */
const allowedHosts = process.env.ALLOWED_SERVER_ACTIONS_HOSTS?.split(',').map(h => h.trim()) || [];

const nextConfig = {
  output: "standalone",
  serverExternalPackages: ["pino", "pino-pretty"],
  sassOptions: {
    quietDeps: true,
    includePaths: ["./node_modules/nhsuk-frontend"],
  },
  experimental: {
    allowedOrigins: allowedHosts,
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
