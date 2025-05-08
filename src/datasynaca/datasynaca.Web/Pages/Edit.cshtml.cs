using Healthtrackr.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Healthtrackr.Web.Pages
{
    public class EditModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<EditModel> _logger;

        [BindProperty]
        public Weight Weight { get; set; }

        public EditModel(IHttpClientFactory httpClientFactory, ILogger<EditModel> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;

        }

        public async Task<IActionResult> OnGet(Guid? id)
        {
            _logger.LogInformation($"Attempting to retrieve weight ID: {id}");
            if (id == null)
            {
                _logger.LogWarning($"Weight ID {id} not found!");
                return NotFound();
            }

            // direct svc to svc http request
            var httpClient = _httpClientFactory.CreateClient("BackendApi");
            Weight = await httpClient.GetFromJsonAsync<Weight>($"api/weight/{id}");

            if (Weight == null)
            {
                _logger.LogWarning($"Weight is empty!");
                return NotFound();
            }

            return Page();
        }
    }
}
