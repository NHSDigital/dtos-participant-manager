#!/bin/bash

# Playwright tests for the Next.js project
cd src/web
echo -e "\nInstalling dependencies"
npm ci
echo -e "\nInstalling Playwright Browsers"
npx playwright install --with-deps
echo -e "\nRunning Playwright tests for Nextjs"
npm run test:e2e
