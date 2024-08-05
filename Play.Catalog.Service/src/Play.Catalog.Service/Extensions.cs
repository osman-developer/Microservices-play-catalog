using Play.Catalog.Service.Dtos;
using Play.Catalog.Service.Entities;

//it is used to map from entity to dto (simillar as automapper)
namespace Play.Catalog.Service {
  public static class Extensions {
    public static ItemDto AsDto (this Item item) {
      return new ItemDto (item.Id, item.Name, item.Description, item.Price, item.CreatedDate);
    }
  }
}