using System;
using Microsoft.Extensions.Logging; // Required for Hybrid logging
using OutSystems.ExternalLibraries.SDK; 
using ODCFlightRecorder.SDK;

namespace OrderManagement.Test {

    [OSInterface(Name = "OrderManagement", Description = "Test app for Flight Recorder")]
    public interface IOrderProcessor {
        [OSAction(Description = "Simulates processing an order with telemetry returned as JSON")]
        void ProcessOrder(string orderId, out string telemetryJson, out bool success);
        
        [OSAction(Description = "Strategy 2: Uses pre-allocated ID and throws a hard exception")]
        void ProcessOrderWithHardException(string orderId, string sessionId);
    }

    public class OrderProcessor : IOrderProcessor
    {
        private readonly ILogger _logger; // Private field to hold the injected logger

        // 🚀 ODC will automatically inject the logger into this constructor
        public OrderProcessor(ILogger logger)
        {
            _logger = logger;
        }

        public void ProcessOrder(string orderId, out string telemetryJson, out bool success)
        {
            // 🚀 Pass the injected _logger to the FlightRecorder constructor
            var recorder = new FlightRecorder("ProcessOrder", $"REQ-{orderId}", null, _logger);

            try
            {
        // 🚀 Move the simulation inside the try block
                if (orderId == "FAIL_TEST")
                {
                    throw new Exception("Simulated Payment Gateway Timeout");
                }

                recorder.AddStep("Validation", "INFO", $"Validating Order ID: {orderId}");
                System.Threading.Thread.Sleep(150);
                recorder.AddStep("Payment Check", "INFO", "Contacting Payment Gateway");

                telemetryJson = recorder.FinalizeBatchAsJson();
                success = true;
            }
            catch (Exception ex)
            {
                // Now this catch block actually gets to do its job!
                recorder.AddStep("Critical Failure", "ERROR", ex.Message);
                telemetryJson = recorder.FinalizeBatchAsJson(hasError: true);
                success = false;
            }
        }
        public void ProcessOrderWithHardException(string orderId, string sessionId)
        {

            // 🚀 Wired: Pass the injected _logger here too
            var recorder = new FlightRecorder("ProcessOrderWithHardException", $"REQ-{orderId}", sessionId, _logger);

            try
            {
                recorder.AddStep("Validation", "INFO", "Validating order...");

                System.Threading.Thread.Sleep(100);

                throw new Exception("CRITICAL: Database Connection Lost during processing.");
            }
            catch (Exception ex)
            {
                // Note: Re-throwing means ODC won't receive 'out' variables, 
                // but your logger will still have sent the steps to the Portal.
                recorder.AddStep("Terminal Failure", "ERROR", ex.Message);
                throw;
            }
        }
    }
}