#!/bin/bash

# Lint the Next.js project
cd src/web
echo -e "\nInstalling dependencies"
npm ci
echo -e "\nRunning linting for Nextjs"
npm run lint
