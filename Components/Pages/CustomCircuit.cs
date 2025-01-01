using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

public class CustomCircuitHandler : CircuitHandler
{
    private readonly ILogger<CustomCircuitHandler> logger;

    public CustomCircuitHandler(ILogger<CustomCircuitHandler> logger)
    {
        this.logger = logger;
    }

    public override Task OnCircuitOpenedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        logger.LogInformation("Circuit opened: {CircuitId}", circuit.Id);
        return Task.CompletedTask;
    }

    public override Task OnCircuitClosedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        logger.LogInformation("Circuit closed: {CircuitId}", circuit.Id);
        return Task.CompletedTask;
    }

    public override Task OnConnectionUpAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        logger.LogInformation("Connection up: {CircuitId}", circuit.Id);
        return Task.CompletedTask;
    }

    public override Task OnConnectionDownAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        logger.LogInformation("Connection down: {CircuitId}", circuit.Id);
        return Task.CompletedTask;
    }
}
