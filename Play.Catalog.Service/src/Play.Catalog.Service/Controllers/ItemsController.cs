using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Play.Catalog.Contracts;
using Play.Catalog.Service.Dtos;
using Play.Catalog.Service.Entities;
using Play.Common;

namespace Play.Catalog.Service.Controllers {
    [Route ("api/[controller]")]
    [ApiController]
    public class ItemsController : ControllerBase {
        //private static int requestCounter = 0;

        private readonly IPublishEndpoint _publishEndpoint;
        private readonly IRepository<Item> _itemsRepository;
        public ItemsController (IRepository<Item> itemsRepository, IPublishEndpoint publishEndpoint) {
            _itemsRepository = itemsRepository;
            _publishEndpoint = publishEndpoint;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ItemDto>>> GetAsync () {
            //The commented code was used to simulate a delay or failure to test the exponential trial and circuit breaker
            // requestCounter++;
            // Console.WriteLine ($"Request {requestCounter}: starting ..");
            // if (requestCounter <= 2) {
            //     Console.WriteLine ($"Request {requestCounter}: Delaying ..");
            //     await Task.Delay (TimeSpan.FromSeconds (5));
            // }
            // if (requestCounter <= 4) {
            //     Console.WriteLine ($"Request {requestCounter}: 500 (internal server error) ..");
            //     return StatusCode (500);
            // }

            //using the asDto enxtension to map between the entity to the DTO
            var items = (await _itemsRepository.GetAllAsync ()).Select (item => item.AsDto ());
            // Console.WriteLine ($"Request {requestCounter}: 200 (Ok) ..");
            return Ok (items);
        }

        [HttpGet ("{id}")]
        public async Task<ActionResult<ItemDto>> GetByIdAsync (Guid id) {
            var item = await _itemsRepository.GetAsync (id);
            if (item == null) {
                return NotFound ();
            }

            return item.AsDto ();
        }

        [HttpPost]
        public async Task<ActionResult<ItemDto>> PostItemAsync (CreateItemDto createItem) {
            var item = new Item {
                Name = createItem.Name,
                Description = createItem.Description,
                Price = createItem.Price,
                CreatedDate = DateTimeOffset.UtcNow
            };
            await _itemsRepository.CreateAsync (item);
            //after creating the item, we need to publish to the message broker that the item created( and the services will listen to the change so they will be notified)
            await _publishEndpoint.Publish (new CatalogItemCreated (item.Id, item.Name, item.Description));
            return CreatedAtAction (nameof (GetByIdAsync), new { id = item.Id }, item);
        }

        [HttpPut ("{id}")]
        public async Task<IActionResult> PutAsync (Guid id, UpdateItemDto item) {
            var existingItem = await _itemsRepository.GetAsync (id);
            if (existingItem == null) {
                return NotFound ();
            }
            existingItem.Name = item.Name;
            existingItem.Description = item.Description;
            existingItem.Price = item.Price;
            await _itemsRepository.UpdateAsync (existingItem);

            //after updating the item, we need to publish to the message broker that the item created( and the services will listen to the change so they will be notified)
            await _publishEndpoint.Publish (new CatalogItemUpdated (existingItem.Id, existingItem.Name, existingItem.Description));
            return NoContent ();
        }

        [HttpDelete ("{id}")]
        public async Task<IActionResult> DeleteAsync (Guid id) {
            var existingItem = await _itemsRepository.GetAsync (id);
            if (existingItem == null) {
                return NotFound ();
            }

            await _itemsRepository.RemoveAsync (existingItem.Id);
            //after deleting the item, we need to publish to the message broker that the item created( and the services will listen to the change so they will be notified)
            await _publishEndpoint.Publish (new CatalogItemDeleted (existingItem.Id));
            return NoContent ();
        }
    }
}