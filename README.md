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
Pull the SDK into your External Logic project via NuGet:

```bash
dotnet add package ODC.FlightRecorder.SDK
```

---

### Quick Start
To start recording, wrap your logic in a `FlightRecorder` instance. If you are using ODC's native `ILogger` (injected via the constructor), the SDK will handle the heavy lifting of linking the traces.

```csharp
public void ProcessOrder(string orderId, out string telemetryJson, ILogger logger = null)
{
    // Start the recorder with a correlation ID
    var recorder = new FlightRecorder("ProcessOrder", $"ORD-{orderId}", null, logger);

    try 
    {
        recorder.AddStep("Validation", "Checking stock levels...");
        // Your logic here...
        
        telemetryJson = recorder.FinalizeBatchAsJson();
    }
    catch (Exception ex)
    {
        // Capture the failure details before the process terminates
        recorder.AddStep("Terminal Failure", "ERROR", ex.Message);
        telemetryJson = recorder.FinalizeBatchAsJson(hasError: true);
        throw; 
    }
}
```

---

### Key Features
* **Native Trace Synchronization**: Automatically captures `Activity.Current.Id` to link business logic steps directly to ODC infrastructure traces.
* **Terminal Failure Capture**: Designed to successfully log "Hard Exceptions" that would otherwise cause a total loss of telemetry.
* **Clean Architecture**: Decouples logging infrastructure from business logic, making it ideal for enterprise-grade OutSystems projects.

### Repository Structure
* **`/src`**: Contains the core **ODC.FlightRecorder.SDK**.
* **`/samples`**: Includes implementation examples, such as the **OrderManagement.Test** project.
* **`/docs`**: Architectural guides and usage documentation.

---

**Built for the OutSystems Community.** *Maintained by Michael de Guzman, Senior Technical Consultant at DB Results.*