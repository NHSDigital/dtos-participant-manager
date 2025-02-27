/** @type {import('next').NextConfig} */
const nextConfig = {
  output: "standalone",
  experimental: {
    serverComponentsExternalPackages: ["pino", "pino-pretty"],
    instrumentationHook: true,
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
