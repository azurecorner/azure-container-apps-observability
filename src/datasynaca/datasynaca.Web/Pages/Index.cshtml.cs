using Healthtrackr.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;

namespace Healthtrackr.Web.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<IndexModel> _logger;
        public List<Weight?> WeightList { get; set; }

        public IndexModel(IHttpClientFactory httpClientFactory, ILogger<IndexModel> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<IActionResult> OnGet()
        {
            _logger.LogInformation("Getting weight list in UI");
            var httpClient = _httpClientFactory.CreateClient("BackendApi");
            WeightList = await httpClient.GetFromJsonAsync<List<Weight>>($"api/weight");
            return Page();
        }
    }
}
