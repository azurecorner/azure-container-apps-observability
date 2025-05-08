using Healthtrackr.Api.Models;
using Healthtrackr.Api.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Healthtrackr.Api.EndpointHandlers
{
    public static class WeightHandlers
    {
        public static async Task<Ok<List<Weight>>> GetWeights(
            IWeightManager weightManager)
        {
            return TypedResults.Ok(await weightManager.GetWeights());
        }

        public static async Task<Results<NotFound, Ok<Weight>>> GetWeightById(
            IWeightManager weightManager,
                       Guid weightId)
        {
            var weight = await weightManager.GetWeightById(weightId);

            if (weight != null)
            {
                return TypedResults.Ok(weight);
            }

            return TypedResults.NotFound();
        }
    }
}
