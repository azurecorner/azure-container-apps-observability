using Healthtrackr.Api.EndpointHandlers;

namespace Healthtrackr.Api.Extensions
{
    public static class EndpointRouteBuilderExtensions
    {
        public static void RegisterWeightEndpoints(this IEndpointRouteBuilder endpoints)
        {
            var weightEndpoints = endpoints.MapGroup("api/weight");

            weightEndpoints.MapGet("", WeightHandlers.GetWeights)
                .WithName("GetWeights");

            weightEndpoints.MapGet("{weightId:guid}", WeightHandlers.GetWeightById)
                .WithName("GetWeightById");
        }
    }
}
