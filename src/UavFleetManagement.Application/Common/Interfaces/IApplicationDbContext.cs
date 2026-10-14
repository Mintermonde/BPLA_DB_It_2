using Microsoft.EntityFrameworkCore;
using UavFleetManagement.Domain.Entities.Catalog;
using UavFleetManagement.Domain.Entities.Engineers;
using UavFleetManagement.Domain.Entities.Fleet;
using UavFleetManagement.Domain.Entities.Identity;
using UavFleetManagement.Domain.Entities.Maintenance;
using UavFleetManagement.Domain.Entities.Pilots;

namespace UavFleetManagement.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Manufacturer> Manufacturers { get; }
    DbSet<UavOwner> UavOwners { get; }
    DbSet<UavUser> UavUsers { get; }
    DbSet<UavModel> UavModels { get; }
    DbSet<UserModelPermission> UserModelPermissions { get; }
    DbSet<UavInstance> UavInstances { get; }
    DbSet<ComponentCatalog> ComponentsCatalog { get; }
    DbSet<InstanceComponent> InstanceComponents { get; }
    DbSet<MaintenanceLog> MaintenanceLogs { get; }
    DbSet<MaintenanceComponentReplacement> MaintenanceComponentReplacements { get; }
    DbSet<Engineer> Engineers { get; }
    DbSet<UserCredential> UserCredentials { get; }
    DbSet<SuperuserCredential> SuperuserCredentials { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
