using Microsoft.EntityFrameworkCore;
using UavFleetManagement.Application.Common.Interfaces;
using UavFleetManagement.Domain.Entities.Catalog;
using UavFleetManagement.Domain.Entities.Engineers;
using UavFleetManagement.Domain.Entities.Fleet;
using UavFleetManagement.Domain.Entities.Identity;
using UavFleetManagement.Domain.Entities.Maintenance;
using UavFleetManagement.Domain.Entities.Pilots;
using UavFleetManagement.Domain.Enums;

namespace UavFleetManagement.Infrastructure.Persistence.Contexts;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Manufacturer> Manufacturers => Set<Manufacturer>();
    public DbSet<UavOwner> UavOwners => Set<UavOwner>();
    public DbSet<UavUser> UavUsers => Set<UavUser>();
    public DbSet<UavModel> UavModels => Set<UavModel>();
    public DbSet<UserModelPermission> UserModelPermissions => Set<UserModelPermission>();
    public DbSet<UavInstance> UavInstances => Set<UavInstance>();
    public DbSet<ComponentCatalog> ComponentsCatalog => Set<ComponentCatalog>();
    public DbSet<InstanceComponent> InstanceComponents => Set<InstanceComponent>();
    public DbSet<MaintenanceLog> MaintenanceLogs => Set<MaintenanceLog>();
    public DbSet<MaintenanceComponentReplacement> MaintenanceComponentReplacements => Set<MaintenanceComponentReplacement>();
    public DbSet<Engineer> Engineers => Set<Engineer>();
    public DbSet<UserCredential> UserCredentials => Set<UserCredential>();
    public DbSet<SuperuserCredential> SuperuserCredentials => Set<SuperuserCredential>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Регистрация кастомных перечислений PostgreSQL
        modelBuilder.HasPostgresEnum<UavAerodynamicSchema>("uav_aerodynamic_schema");
        modelBuilder.HasPostgresEnum<UavWeightCategory>("uav_weight_category");
        modelBuilder.HasPostgresEnum<UavInstanceStatus>("uav_instance_status");
        modelBuilder.HasPostgresEnum<MaintenanceType>("maintenance_type");
        modelBuilder.HasPostgresEnum<OwnerLegalType>("owner_legal_type");
        modelBuilder.HasPostgresEnum<UavControlType>("uav_control_type");
        modelBuilder.HasPostgresEnum<ComponentCondition>("component_condition");

        // Manufacturers
        modelBuilder.Entity<Manufacturer>(b =>
        {
            b.ToTable("manufacturers");
            b.HasKey(x => x.ManufacturerId);
            b.Property(x => x.ManufacturerId).HasColumnName("manufacturer_id");
            b.Property(x => x.Name).HasColumnName("name").HasMaxLength(150).IsRequired();
            b.HasIndex(x => x.Name).IsUnique();
            b.Property(x => x.Country).HasColumnName("country").HasMaxLength(100).HasDefaultValue("Россия");
            b.Property(x => x.Inn).HasColumnName("inn").HasMaxLength(12);
            b.Property(x => x.Website).HasColumnName("website").HasMaxLength(255);
            b.Property(x => x.ContactPhone).HasColumnName("contact_phone").HasMaxLength(20);
            b.Property(x => x.CreatedAt).HasColumnName("created_at");
        });

        // UavOwners
        modelBuilder.Entity<UavOwner>(b =>
        {
            b.ToTable("uav_owners");
            b.HasKey(x => x.OwnerId);
            b.Property(x => x.OwnerId).HasColumnName("owner_id");
            b.Property(x => x.LegalType).HasColumnName("legal_type");
            b.Property(x => x.Name).HasColumnName("name").HasMaxLength(255).IsRequired();
            b.Property(x => x.Inn).HasColumnName("inn").HasMaxLength(12);
            b.Property(x => x.Ogrn).HasColumnName("ogrn").HasMaxLength(15);
            b.Property(x => x.Snils).HasColumnName("snils").HasMaxLength(11);
            b.Property(x => x.ContactEmail).HasColumnName("contact_email").HasMaxLength(100);
            b.Property(x => x.ContactPhone).HasColumnName("contact_phone").HasMaxLength(20);
            b.Property(x => x.LegalAddress).HasColumnName("legal_address");
            b.Property(x => x.CreatedAt).HasColumnName("created_at");
        });

        // UavUsers
        modelBuilder.Entity<UavUser>(b =>
        {
            b.ToTable("uav_users");
            b.HasKey(x => x.UserId);
            b.Property(x => x.UserId).HasColumnName("user_id");
            b.Property(x => x.FirstName).HasColumnName("first_name").HasMaxLength(50).IsRequired();
            b.Property(x => x.LastName).HasColumnName("last_name").HasMaxLength(50).IsRequired();
            b.Property(x => x.MiddleName).HasColumnName("middle_name").HasMaxLength(50);
            b.Property(x => x.PilotLicenseNumber).HasColumnName("pilot_license_number").HasMaxLength(50);
            b.Property(x => x.LicenseIssueDate).HasColumnName("license_issue_date");
            b.Property(x => x.LicenseExpiryDate).HasColumnName("license_expiry_date");
            b.Property(x => x.MedicalCertificateNumber).HasColumnName("medical_certificate_number").HasMaxLength(50);
            b.Property(x => x.Phone).HasColumnName("phone").HasMaxLength(20).IsRequired();
            b.HasIndex(x => x.Phone).IsUnique();
            b.Property(x => x.Email).HasColumnName("email").HasMaxLength(100);
            b.HasIndex(x => x.Email).IsUnique();
            b.Property(x => x.IsActive).HasColumnName("is_active").HasDefaultValue(true);
        });

        // UavModels
        modelBuilder.Entity<UavModel>(b =>
        {
            b.ToTable("uav_models");
            b.HasKey(x => x.ModelId);
            b.Property(x => x.ModelId).HasColumnName("model_id");
            b.Property(x => x.ManufacturerId).HasColumnName("manufacturer_id");
            b.Property(x => x.ModelName).HasColumnName("model_name").HasMaxLength(100).IsRequired();
            b.Property(x => x.AerodynamicSchema).HasColumnName("aerodynamic_schema");
            b.Property(x => x.WeightCategory).HasColumnName("weight_category");
            b.Property(x => x.ControlType).HasColumnName("control_type");
            b.Property(x => x.MtowKg).HasColumnName("mtow_kg").HasPrecision(6, 2).IsRequired();
            b.Property(x => x.MaxFlightTimeMins).HasColumnName("max_flight_time_mins");
            b.Property(x => x.MaxRangeKm).HasColumnName("max_range_km").HasPrecision(6, 2);
            b.Property(x => x.CruisingSpeedKmh).HasColumnName("cruising_speed_kmh");
            b.Property(x => x.MaxAltitudeM).HasColumnName("max_altitude_m");
            b.Property(x => x.MaxWindSpeedMs).HasColumnName("max_wind_speed_ms").HasPrecision(4, 1);
            b.Property(x => x.MinTempCelsius).HasColumnName("min_temp_celsius").HasDefaultValue(-20);
            b.Property(x => x.MaxTempCelsius).HasColumnName("max_temp_celsius").HasDefaultValue(40);
            b.Property(x => x.LengthMm).HasColumnName("length_mm");
            b.Property(x => x.WidthWingspanMm).HasColumnName("width_wingspan_mm");
            b.Property(x => x.HeightMm).HasColumnName("height_mm");
            b.Property(x => x.BasePriceRub).HasColumnName("base_price_rub").HasPrecision(12, 2);

            b.HasIndex(x => new { x.ManufacturerId, x.ModelName }).IsUnique();

            b.HasOne(x => x.Manufacturer)
                .WithMany(m => m.Models)
                .HasForeignKey(x => x.ManufacturerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // UserModelPermissions
        modelBuilder.Entity<UserModelPermission>(b =>
        {
            b.ToTable("user_model_permissions");
            b.HasKey(x => x.PermissionId);
            b.Property(x => x.PermissionId).HasColumnName("permission_id");
            b.Property(x => x.UserId).HasColumnName("user_id");
            b.Property(x => x.ModelId).HasColumnName("model_id");
            b.Property(x => x.GrantedDate).HasColumnName("granted_date");
            b.Property(x => x.InsurancePolicyNumber).HasColumnName("insurance_policy_number").HasMaxLength(100);

            b.HasIndex(x => new { x.UserId, x.ModelId }).IsUnique();

            b.HasOne(x => x.User)
                .WithMany(u => u.Permissions)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(x => x.Model)
                .WithMany(m => m.Permissions)
                .HasForeignKey(x => x.ModelId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // UavInstances
        modelBuilder.Entity<UavInstance>(b =>
        {
            b.ToTable("uav_instances");
            b.HasKey(x => x.InstanceId);
            b.Property(x => x.InstanceId).HasColumnName("instance_id");
            b.Property(x => x.ModelId).HasColumnName("model_id");
            b.Property(x => x.OwnerId).HasColumnName("owner_id");
            b.Property(x => x.SerialNumber).HasColumnName("serial_number").HasMaxLength(100).IsRequired();
            b.HasIndex(x => x.SerialNumber).IsUnique();
            b.Property(x => x.RosaviatsiaNumber).HasColumnName("rosaviatsia_number").HasMaxLength(50);
            b.Property(x => x.ProductionDate).HasColumnName("production_date");
            b.Property(x => x.TotalFlightTimeHours).HasColumnName("total_flight_time_hours").HasPrecision(7, 2).HasDefaultValue(0.00m);
            b.Property(x => x.CurrentStatus).HasColumnName("current_status");
            b.Property(x => x.HullInsurancePolicy).HasColumnName("hull_insurance_policy").HasMaxLength(100);
            b.Property(x => x.InsuranceExpiryDate).HasColumnName("insurance_expiry_date");
            b.Property(x => x.PrimaryColor).HasColumnName("primary_color").HasMaxLength(50);
            b.Property(x => x.LastUpdate).HasColumnName("last_update");

            b.HasOne(x => x.Model)
                .WithMany(m => m.Instances)
                .HasForeignKey(x => x.ModelId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(x => x.Owner)
                .WithMany(o => o.Instances)
                .HasForeignKey(x => x.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ComponentsCatalog
        modelBuilder.Entity<ComponentCatalog>(b =>
        {
            b.ToTable("components_catalog");
            b.HasKey(x => x.ComponentTypeId);
            b.Property(x => x.ComponentTypeId).HasColumnName("component_type_id");
            b.Property(x => x.ManufacturerId).HasColumnName("manufacturer_id");
            b.Property(x => x.PartNumber).HasColumnName("part_number").HasMaxLength(100).IsRequired();
            b.HasIndex(x => x.PartNumber).IsUnique();
            b.Property(x => x.ComponentName).HasColumnName("component_name").HasMaxLength(150).IsRequired();
            b.Property(x => x.Category).HasColumnName("category").HasMaxLength(100).IsRequired();
            b.Property(x => x.MtbfHours).HasColumnName("mtbf_hours");
            b.Property(x => x.WeightG).HasColumnName("weight_g");
            b.Property(x => x.SpecificationsJson).HasColumnName("specifications").HasColumnType("jsonb");

            b.HasOne(x => x.Manufacturer)
                .WithMany(m => m.Components)
                .HasForeignKey(x => x.ManufacturerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // InstanceComponents
        modelBuilder.Entity<InstanceComponent>(b =>
        {
            b.ToTable("instance_components");
            b.HasKey(x => x.InstalledComponentId);
            b.Property(x => x.InstalledComponentId).HasColumnName("installed_component_id");
            b.Property(x => x.InstanceId).HasColumnName("instance_id");
            b.Property(x => x.ComponentTypeId).HasColumnName("component_type_id");
            b.Property(x => x.ComponentSerialNumber).HasColumnName("component_serial_number").HasMaxLength(100).IsRequired();
            b.HasIndex(x => x.ComponentSerialNumber).IsUnique();
            b.Property(x => x.InstallationDate).HasColumnName("installation_date").IsRequired();
            b.Property(x => x.ComponentOperatingHours).HasColumnName("component_operating_hours").HasPrecision(7, 2).HasDefaultValue(0.00m);
            b.Property(x => x.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            b.Property(x => x.Condition).HasColumnName("condition");
            b.Property(x => x.BatteryChargeCyclesInitial).HasColumnName("battery_charge_cycles_initial").HasDefaultValue(0);

            b.HasOne(x => x.Instance)
                .WithMany(i => i.InstalledComponents)
                .HasForeignKey(x => x.InstanceId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(x => x.ComponentType)
                .WithMany(c => c.InstanceComponents)
                .HasForeignKey(x => x.ComponentTypeId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // MaintenanceLogs
        modelBuilder.Entity<MaintenanceLog>(b =>
        {
            b.ToTable("maintenance_logs");
            b.HasKey(x => x.LogId);
            b.Property(x => x.LogId).HasColumnName("log_id");
            b.Property(x => x.InstanceId).HasColumnName("instance_id");
            b.Property(x => x.TypeOfMaintenance).HasColumnName("type_of_maintenance");
            b.Property(x => x.MaintenanceDate).HasColumnName("maintenance_date").IsRequired();
            b.Property(x => x.UavOperatingHoursAtMaintenance).HasColumnName("uav_operating_hours_at_maintenance").HasPrecision(7, 2).IsRequired();
            b.Property(x => x.Description).HasColumnName("description");
            b.Property(x => x.EngineerNameFallback).HasColumnName("engineer_name").HasMaxLength(150);
            b.Property(x => x.EngineerId).HasColumnName("engineer_id");
            b.Property(x => x.NextMaintenanceDate).HasColumnName("next_maintenance_date");

            b.HasOne(x => x.Instance)
                .WithMany(i => i.MaintenanceLogs)
                .HasForeignKey(x => x.InstanceId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(x => x.Engineer)
                .WithMany(e => e.MaintenanceLogs)
                .HasForeignKey(x => x.EngineerId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // MaintenanceComponentReplacements
        modelBuilder.Entity<MaintenanceComponentReplacement>(b =>
        {
            b.ToTable("maintenance_component_replacements");
            b.HasKey(x => x.ReplacementId);
            b.Property(x => x.ReplacementId).HasColumnName("replacement_id");
            b.Property(x => x.LogId).HasColumnName("log_id");
            b.Property(x => x.RemovedComponentId).HasColumnName("removed_component_id");
            b.Property(x => x.InstalledComponentId).HasColumnName("installed_component_id");
            b.Property(x => x.ReasonForReplacement).HasColumnName("reason_for_replacement");

            b.HasOne(x => x.MaintenanceLog)
                .WithMany(m => m.Replacements)
                .HasForeignKey(x => x.LogId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(x => x.RemovedComponent)
                .WithMany()
                .HasForeignKey(x => x.RemovedComponentId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(x => x.InstalledComponent)
                .WithMany()
                .HasForeignKey(x => x.InstalledComponentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Engineers
        modelBuilder.Entity<Engineer>(b =>
        {
            b.ToTable("engineers");
            b.HasKey(x => x.EngineerId);
            b.Property(x => x.EngineerId).HasColumnName("engineer_id");
            b.Property(x => x.FirstName).HasColumnName("first_name").HasMaxLength(50).IsRequired();
            b.Property(x => x.LastName).HasColumnName("last_name").HasMaxLength(50).IsRequired();
            b.Property(x => x.MiddleName).HasColumnName("middle_name").HasMaxLength(50);
            b.Property(x => x.QualificationCategory).HasColumnName("qualification_category").HasMaxLength(100).IsRequired();
            b.Property(x => x.CertificationNumber).HasColumnName("certification_number").HasMaxLength(100).IsRequired();
            b.Property(x => x.CertificationExpiryDate).HasColumnName("certification_expiry_date").IsRequired();
            b.Property(x => x.Phone).HasColumnName("phone").HasMaxLength(20).IsRequired();
            b.Property(x => x.Email).HasColumnName("email").HasMaxLength(100);
            b.Property(x => x.IsActive).HasColumnName("is_active").HasDefaultValue(true);
        });

        // UserCredentials
        modelBuilder.Entity<UserCredential>(b =>
        {
            b.ToTable("user_credentials");
            b.HasKey(x => x.UserCredentialId);
            b.Property(x => x.UserCredentialId).HasColumnName("user_credential_id");
            b.Property(x => x.Username).HasColumnName("username").HasMaxLength(100).IsRequired();
            b.HasIndex(x => x.Username).IsUnique();
            b.Property(x => x.Email).HasColumnName("email").HasMaxLength(100).IsRequired();
            b.Property(x => x.PasswordHash).HasColumnName("password_hash").IsRequired();
            b.Property(x => x.PasswordSalt).HasColumnName("password_salt").IsRequired();
            b.Property(x => x.LinkedUserId).HasColumnName("linked_user_id");
            b.Property(x => x.LinkedEngineerId).HasColumnName("linked_engineer_id");
            b.Property(x => x.IsBlocked).HasColumnName("is_blocked").HasDefaultValue(false);
            b.Property(x => x.CreatedAt).HasColumnName("created_at");
            b.Property(x => x.LastLoginAt).HasColumnName("last_login_at");

            b.HasOne(x => x.LinkedUser)
                .WithOne(u => u.UserCredential)
                .HasForeignKey<UserCredential>(x => x.LinkedUserId)
                .OnDelete(DeleteBehavior.SetNull);

            b.HasOne(x => x.LinkedEngineer)
                .WithOne(e => e.UserCredential)
                .HasForeignKey<UserCredential>(x => x.LinkedEngineerId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // SuperuserCredentials
        modelBuilder.Entity<SuperuserCredential>(b =>
        {
            b.ToTable("superuser_credentials");
            b.HasKey(x => x.SuperuserId);
            b.Property(x => x.SuperuserId).HasColumnName("superuser_id");
            b.Property(x => x.Username).HasColumnName("username").HasMaxLength(100).IsRequired();
            b.HasIndex(x => x.Username).IsUnique();
            b.Property(x => x.PasswordHash).HasColumnName("password_hash").IsRequired();
            b.Property(x => x.PasswordSalt).HasColumnName("password_salt").IsRequired();
            b.Property(x => x.SecurityStamp).HasColumnName("security_stamp").HasMaxLength(100).IsRequired();
            b.Property(x => x.CreatedAt).HasColumnName("created_at");
            b.Property(x => x.LastLoginAt).HasColumnName("last_login_at");
        });
    }
}
