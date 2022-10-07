import react from "@vitejs/plugin-react";
import { spawnSync } from "child_process";
import fs from "fs";
import path from "path";
import { defineConfig } from "vite";

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    https: {
      pfx: fs.readFileSync(getAspNetCorePfxFilename()),
      passphrase: "P@$$w0rd"
    },
    open: "https://localhost:8080",
    port: 8080,
    proxy: {
      "/api": {
        secure: false,
        target: "https://localhost:5001"
      }
    }
  }
});

function getAspNetCorePfxFilename() {
  const nameArg = process.argv
    .map(x => x.match(/--name=(?<value>.+)/i))
    .filter(Boolean)[0];
  const pfxName = nameArg ? nameArg.groups.value : process.env.npm_package_name;

  if (!pfxName) {
    console.error(
      "Run this script in the context of an npm/yarn script or pass --name=<<app>> explicitly."
    );
    process.exit(-1);
  }

  const pfxFilename = path.join(
    process.env.APPDATA
      ? `${process.env.APPDATA}/ASP.NET/https`
      : `${process.env.HOME}/.aspnet/https`,
    pfxName + ".pfx"
  );

  if (
    !fs.existsSync(pfxFilename) &&
    spawnSync(
      "dotnet",
      [
        "dev-certs",
        "https",
        "--export-path",
        pfxFilename,
        "--password",
        "P@$$w0rd"
      ],
      {
        stdio: "inherit"
      }
    ).status !== 0
  ) {
    process.exit(-1);
  }

  return pfxFilename;
}
