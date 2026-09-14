# ClinicSol 2026.09.14 offline deployment

This folder contains the COMPLETE compressed deployment package in 9 parts, at most 48 MiB each. Commit this entire folder to GitHub using Git. Git LFS and third-party archive tools are not required. Do not commit the reconstructed ZIP or expanded folder.

1. Clone/download this folder, keeping all parts, scripts and archive-manifest.json together. If downloading GitHub's repository ZIP, extract it first.
2. Double-click Extract-Deploy.cmd. It verifies every part, rebuilds the ZIP, extracts it into expanded/artifacts-deploy, and validates the deployment files. Allow roughly 1.5 GB of free disk space for reconstruction and extraction, plus installation/database space.
3. Inside expanded/artifacts-deploy, read README.md and double-click Deploy.cmd. Approve administrator elevation, enter the HTTP port, and select the default local SQL Express option if SQL Server is absent.
4. Open the printed URL and create the first administrator with the generated setup token.

Included: published backend, Angular in wwwroot, SQL Server Express offline installer, .NET 8 IIS Hosting Bundle, deployment scripts, maintenance scripts, English/Arabic guides, and SHA256 manifests. No application build, Node, .NET SDK or dependency download is needed on the target PC. Windows x64 with IIS/WAS/Management Console is required; enabling Windows features may require Windows media or network access.

Every Deploy.cmd run creates a NEW site and database. It does not upgrade an existing instance or preserve its data in a new instance. Reuse the extracted folder for additional fresh deployments; do not rerun the extractor over an existing expanded folder.

This is a portable installer package, not an application that runs without IIS/SQL installation. Intranet HTTP access follows the deployment README. Checksums detect accidental corruption, not tampering with both data and manifests.

To verify parts only: powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\Extract-Deploy.ps1 -ValidateOnly

Full ZIP SHA256: C509088B8B198020847E9B100F4A7B481C92DAD98FD4FB9E506FDF0E5FFAEA7A

For a future version, refresh artifacts-deploy, then run scripts/New-PortableDeploy.ps1 from the repository with a new -Version. Keep the parts from different versions in separate folders. Frequent binary versions enlarge Git history; GitHub Releases are suitable for retaining full ZIPs outside normal Git history.
