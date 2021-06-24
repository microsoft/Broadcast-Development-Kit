using Domain.Constants;
using MediatR;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Threading.Tasks;
using static Application.Service.Commands.HandleEventGridServiceInfrastructureEvent;
using static Application.Service.Commands.StartServiceInfrastructure;
using static Application.Service.Commands.StopServiceInfrastructure;

namespace BotOrchestrator
{
    public class OrchestratorFunctions
    {
        private readonly IMediator _mediator;
        private readonly ILogger<OrchestratorFunctions> _logger;

        public OrchestratorFunctions(IMediator mediator,
            ILogger<OrchestratorFunctions> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [FunctionName("start-virtual-machine")]
        public async Task StartVirtualMachineAsync([QueueTrigger(Constants.AzureQueueNames.StartVirtualMachineQueue, Connection = Constants.StorageAccountSettingName)] StartServiceInfrastructureCommand command)
        {
            _logger.LogInformation("C# Queue trigger function processed: {command}", JsonConvert.SerializeObject(command));
            
            var response = await _mediator.Send(command);

            _logger.LogInformation("Response: {response}", JsonConvert.SerializeObject(response));
        }

        [FunctionName("stop-virtual-machine")]
        public async Task StopVirtualMachineAsync([QueueTrigger(Constants.AzureQueueNames.StopVirtualMachineQueue, Connection = Constants.StorageAccountSettingName)] StopServiceInfrastructureCommand command)
        {
            _logger.LogInformation("C# Queue trigger function processed: {command}", JsonConvert.SerializeObject(command));
            
            var response = await _mediator.Send(command);

            _logger.LogInformation("Response: {response}", JsonConvert.SerializeObject(response));
        }

        [FunctionName("virtual-machine-event-grid-handler")]
        public async Task VirtualMachineEventGridHandler([EventGridTrigger] EventGridEvent eventGrid)
        {

            var command = new HandleEventGridServiceInfrastructureEventCommand
            {
                EventType = eventGrid.EventType,
                Data = eventGrid.Data,
                ServiceInfrastructureId = eventGrid.Subject.ToLower().Trim()
            };

            await _mediator.Send(command);
        }
    }
}
