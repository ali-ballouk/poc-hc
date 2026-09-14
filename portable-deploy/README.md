# Portable ClinicSol deployment packages

Each version folder contains the complete offline deployment ZIP split into files smaller than GitHub's normal Git file limit. Commit the entire version folder, including every part, both extractor scripts, and the checksum manifest. Use Git to push these files; the parts exceed the browser upload limit.

To deploy, clone or download the version folder, then double-click **Extract-Deploy.cmd**. After extraction, open **expanded/artifacts-deploy/Deploy.cmd** and choose the HTTP port. The original full ZIP is also available locally under **artifacts/portable/**; that oversized ZIP is intentionally excluded from Git.

The package includes SQL Express and the .NET IIS Hosting Bundle installers. Windows x64, IIS, Windows Process Activation Service and the IIS Management Console are required. It creates a new instance and a fresh database; it does not update an existing instance.

To package another version after refreshing `artifacts-deploy`:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/New-PortableDeploy.ps1 -Version 2026.09.15
```

Use a unique version name. The builder refuses to overwrite an existing version. Reconstructed ZIPs and extracted files are ignored by Git. Keep offline installers unchanged unless intentionally updating their versions and package checksums.

GitHub blocks normal Git files larger than 100 MiB; each archive part is at most 48 MiB. For many historical binary versions, prefer GitHub Releases to avoid growing the repository indefinitely. See [GitHub's large-file documentation](https://docs.github.com/en/repositories/working-with-files/managing-large-files/about-large-files-on-github).
