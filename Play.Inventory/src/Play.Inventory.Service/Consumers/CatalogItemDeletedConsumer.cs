using MassTransit;
using Play.Catalog.Contracts;
using Play.Common;
using Play.Inventory.Service.Entities;

namespace Play.Inventory.Service.Consumers {
  //it is consuming the message that is being sent to the queue which is CatalogItemDeleted sent from catalogservice in controller api
  public class CatalogItemDeletedConsumer : IConsumer<CatalogItemDeleted> {
    private readonly IRepository<CatalogItem> _repository;

    public CatalogItemDeletedConsumer (IRepository<CatalogItem> repository) {
      _repository = repository;
    }
    public async Task Consume (ConsumeContext<CatalogItemDeleted> context) {
      var message = context.Message;
      var item = await _repository.GetAsync (message.ItemId);

      if (item == null) {
        return;
      }
      await _repository.RemoveAsync (message.ItemId);

    }
  }
}