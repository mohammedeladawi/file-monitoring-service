# FileMonitoringService

A Windows Service in C# that monitors a folder for new files, renames them with a GUID, moves them to a destination folder, and logs all key events. Ideal for automating file processing tasks.

---

## Features

- **Folder Monitoring**: Watches a source folder for new files.
- **File Processing**:
  - Renames files using a GUID.
  - Moves files to a destination folder.
  - Deletes the original file from the source folder.
- **Logging**: Logs service start/stop events, detected files, file moves, and errors with timestamps.
- **Console Mode**: Can run as a console application for debugging.
- **Automatic Deployment**: Includes a ProjectInstaller for service installation.

---

## Configuration

All settings are defined in `App.config`:

```xml
<appSettings>
    <add key="SourceFolder" value="C:\FileMonitoring\Source" />
    <add key="DestinationFolder" value="C:\FileMonitoring\Destination" />
    <add key="LogFolder" value="C:\FileMonitoring\Logs" />
</appSettings>