using Microsoft.AspNetCore.Mvc;
using Play.Common;
using Play.Inventory.Service.Clients;
using Play.Inventory.Service.Dtos;
using Play.Inventory.Service.Entities;

namespace Play.Inventory.Service.Controllers {
  [ApiController]
  [Route ("api/item")]
  public class ItemController : ControllerBase {

    private readonly IRepository<InventoryItem> _inventoryItemsRepository;
    private readonly IRepository<CatalogItem> _catalogItemRepository;
    // private readonly CatalogClient _catalogClient;
    public ItemController (IRepository<InventoryItem> inventoryItemsRepository, IRepository<CatalogItem> catalogItemRepository, CatalogClient catalogClient) {
      _inventoryItemsRepository = inventoryItemsRepository;
      // _catalogClient = catalogClient;
      _catalogItemRepository = catalogItemRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<InventoryItemDto>>> GetAsync (Guid userId) {

      if (userId == Guid.Empty) {
        return BadRequest ();
      }
      //we are no longer using sync calls, we are using the catalogitems db inside the inventory
      // var catalogItems = await _catalogClient.GetCatalogItemsAsync ();
      var inventoryitemEntities = await _inventoryItemsRepository.GetAllAsync (item => item.UserId == userId);
      var catalogItemIds = inventoryitemEntities.Select (item => item.CatalogItemId);
      var catalogItems = await _catalogItemRepository.GetAllAsync (catalogItem => catalogItemIds.Contains (catalogItem.Id));

      var inventoryItemsDtos = inventoryitemEntities.Select (inventoryItem => {
        var catalogItem = catalogItems.Single (catalogItem => catalogItem.Id == inventoryItem.CatalogItemId);
        return inventoryItem.AsDto (catalogItem.Name, catalogItem.Description);
      });

      return Ok (inventoryItemsDtos);
    }

    [HttpPost]
    public async Task<ActionResult> PostAsync (GrantItemsDto grantItemsDto) {
      var inventoryItem = await _inventoryItemsRepository.GetAsync (
        item => item.UserId == grantItemsDto.UserId && item.CatalogItemId == grantItemsDto.CatalogItemId);

      if (inventoryItem == null) {
        inventoryItem = new InventoryItem {
        CatalogItemId = grantItemsDto.CatalogItemId,
        UserId = grantItemsDto.UserId,
        Quantity = grantItemsDto.Quantity,
        AccquiredDate = DateTimeOffset.UtcNow
        };
        await _inventoryItemsRepository.CreateAsync (inventoryItem);
      } else {
        inventoryItem.Quantity += grantItemsDto.Quantity;
        await _inventoryItemsRepository.UpdateAsync (inventoryItem);
      }

      return Ok ();
    }
  }
}