# Security Policy

## Design

ClipSaver is designed to be fully local:

- No network requests
- No telemetry
- No accounts or cloud services
- Clipboard history, settings, and images are stored only in `%LOCALAPPDATA%\ClipSaver`

## Reporting a vulnerability

If you find a security issue, please do not open a public issue with sensitive details.

Use GitHub's private vulnerability reporting for this repository, or contact the maintainer through GitHub.

Please include:

- Affected version
- Description of the issue
- Reproduction steps
- Potential impact
- Suggested fix, if available

## Scope

Typical security concerns for this project include:

- Clipboard data being written outside the local data folder
- Network access being introduced
- Unsafe parsing of local history/settings files
- Installer or uninstaller deleting files outside the expected directories
- Privilege escalation
