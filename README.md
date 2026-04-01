# ODC Flight Recorder

![NuGet Version](https://img.shields.io/nuget/v/ODC.FlightRecorder.SDK)

**ODC Flight Recorder** is a high-performance C# telemetry SDK designed specifically for **OutSystems Developer Cloud (ODC)** External Logic. It bridges the gap between high-level business audit trails and deep-system infrastructure logs, providing "black box" observability for complex cloud logic.

---

### The Problem: The "Black Box"
Standard External Logic often operates as a black box. When an action fails or complex logic (like an AI RAG engine) returns an unexpected result, developers are often left guessing what happened inside the C# code because the logs are disconnected from the platform.

### The Solution: Hybrid Observability
This SDK implements a **Hybrid Logging Strategy**:
1. **The Flight Path (JSON)**: Generates a structured JSON string containing every logic step, which can be saved to an OutSystems entity for long-term auditing and in-app dashboards.
2. **The Golden Thread (ODC Portal)**: Simultaneously broadcasts those same steps to the native **ODC Monitoring** tab, automatically linking them to the **NativeTrace ID** for real-time DevOps debugging and distributed tracing.

---

### Installation
Pull the latest stable version of the SDK into your External Logic project via NuGet:

```bash
dotnet add package ODC.FlightRecorder.SDK --version 1.0.2
```

---

### Quick Start
To start recording, wrap your logic in a `FlightRecorder` instance. If you are using ODC's native `ILogger` (injected via the constructor), the SDK will handle the heavy lifting of linking the traces. 

**Important:** To ensure your telemetry is not lost during a crash, avoid re-throwing hard exceptions. ODC discards all output parameters when an unhandled exception propagates out of C#.

```csharp
public void ProcessOrder(string orderId, out string telemetryJson, out bool success, ILogger logger = null)
{
    // Start the recorder with a correlation ID
    var recorder = new FlightRecorder("ProcessOrder", $"ORD-{orderId}", null, logger);

    try 
    {
        recorder.AddStep("Validation", "Checking stock levels...");
        // Your logic here...
        
        success = true;
        telemetryJson = recorder.FinalizeBatchAsJson();
    }
    catch (Exception ex)
    {
        // Capture failure details and return them safely to ODC
        recorder.AddStep("Terminal Failure", "ERROR", ex.Message);
        
        success = false;
        telemetryJson = recorder.FinalizeBatchAsJson(hasError: true);
        // Do not use 'throw;' here if you want to keep the JSON in your database
    }
}
```

---

### Key Features
* **Native Trace Synchronization**: Automatically captures `Activity.Current.Id` at session start to link business logic steps directly to ODC infrastructure traces.
* **Terminal Failure Capture**: Specifically designed to retain the execution story even when the business logic hits a critical error.
* **Clean Architecture**: Decouples your logging from your business logic, making it easier to maintain enterprise-grade OutSystems projects.

### Breaking Changes (v1.0.2)
* **Property Rename**: The property `Mehodname` in the `TraceSession_ST` structure was renamed to `MethodName` for better clarity.

### Repository Structure
* **`/src`**: Contains the core **ODC.FlightRecorder.SDK**.
* **`/samples`**: Includes implementation examples, such as the **OrderManagement.Test** project.
* **`/docs`**: Architectural guides and usage documentation.

### Note for Local Builds
This repository includes a root-level **NuGet.Config** file. If you are building the SDK or samples locally on your machine, ensure this file remains in the root folder. It is configured to ignore private package feeds and pull all dependencies directly from the public NuGet gallery to prevent authentication errors during the build process.

---

**Built for the OutSystems Community.** *Maintained by Michael de Guzman, OutSystems Champion*