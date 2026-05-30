using ComoGastoMinhaGrana.Domain.Entities;

namespace ComoGastoMinhaGrana.Application.Tests.Common.Mothers;

public static class CategoryMother
{
    public static Category Default(Guid? userId = null, Guid? id = null) => new()
    {
        Id = id ?? Guid.NewGuid(),
        UserId = userId ?? Guid.NewGuid(),
        Name = "Alimentação",
        Color = "#FF5733"
    };

    public static Category OwnedBy(Guid userId, Guid? id = null) => Default(userId, id);

    public static Category WithName(string name, Guid? userId = null) => new()
    {
        Id = Guid.NewGuid(),
        UserId = userId ?? Guid.NewGuid(),
        Name = name,
        Color = "#000000"
    };
}
