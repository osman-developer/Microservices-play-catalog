using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Play.Inventory.Service.Dtos;
namespace Play.Inventory.Service.Clients {
  public class CatalogClient {
    private readonly HttpClient _httpClient;
    public CatalogClient (HttpClient httpClient) {
      _httpClient = httpClient;
    }

    public async Task<List<CatalogItemDto>> GetCatalogItemsAsync () {
      HttpResponseMessage response = await _httpClient.GetAsync ("http://localhost:5000/api/Items");
      var result = response.Content.ReadAsStringAsync ().Result;
      return JsonConvert.DeserializeObject<List<CatalogItemDto>> (result);
    }
  }
}