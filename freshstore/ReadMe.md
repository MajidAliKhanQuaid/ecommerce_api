### Commands

```ps

dotnet ef migrations add initial --project ..\freshstore.bll

dotnet ef migrations remove --project ..\freshstore.bll

dotnet ef database update

# undo all migrations
dotnet ef database update 0

```

### Seeding the Data

```cs

// use this inside OnModelCreating()
// then add a new migration `dotnet ef migrations add "added data" --project ..\freshstore.bll`
// finally update the database `dotnet ef database update`

string[] userPermissions = { "UP.AddToCart", "UP.RemoveFromCart", "UP.PlaceAnOrder", "UP.CancelAnOrder", "UP.ChangeEmail", "UP.ChangeMobile" };
for (long i = 0; i < userPermissions.Length; i++)
{
    modelBuilder.Entity<UserLevelPermission>().HasData(new UserLevelPermission
    {
        Id = i + 1,
        Name = userPermissions[i]
    });
}


string[] rolePermissions = { "RP.Create", "RP.View", "RP.Update", "RP.Delete" };
for (long i = 0; i < rolePermissions.Length; i++)
{
    modelBuilder.Entity<RoleLevelPermission>().HasData(new RoleLevelPermission
    {
        Id = i + 1,
        Name = rolePermissions[i]
    });
}
```