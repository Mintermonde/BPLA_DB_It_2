using Microsoft.EntityFrameworkCore;
using UavFleetManagement.Application.Common.Interfaces;
using UavFleetManagement.Domain.Entities.Catalog;
using UavFleetManagement.Domain.Entities.Engineers;
using UavFleetManagement.Domain.Entities.Fleet;
using UavFleetManagement.Domain.Entities.Identity;
using UavFleetManagement.Domain.Enums;
using UavFleetManagement.Infrastructure.Persistence.Contexts;

namespace UavFleetManagement.Infrastructure.Persistence;

public static class DbInitializer
{
    public static async Task SeedAsync(ApplicationDbContext context, IPasswordHasher hasher)
    {
        // 1. Создаем необходимые таблицы расширения (если контейнер был запущен со старым томом)
        try
        {
            await context.Database.ExecuteSqlRawAsync(@"
CREATE TABLE IF NOT EXISTS engineers (
    engineer_id SERIAL PRIMARY KEY,
    first_name VARCHAR(50) NOT NULL,
    last_name VARCHAR(50) NOT NULL,
    middle_name VARCHAR(50),
    qualification_category VARCHAR(100) NOT NULL,
    certification_number VARCHAR(100) NOT NULL,
    certification_expiry_date DATE NOT NULL,
    phone VARCHAR(20) NOT NULL,
    email VARCHAR(100),
    is_active BOOLEAN DEFAULT TRUE
);

DO $$ 
BEGIN 
    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name='maintenance_logs') THEN
        IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='maintenance_logs' AND column_name='engineer_id') THEN
            ALTER TABLE maintenance_logs ADD COLUMN engineer_id INT REFERENCES engineers(engineer_id) ON DELETE SET NULL;
        END IF;
    END IF;
END $$;

CREATE TABLE IF NOT EXISTS user_credentials (
    user_credential_id SERIAL PRIMARY KEY,
    username VARCHAR(100) NOT NULL UNIQUE,
    email VARCHAR(100) NOT NULL,
    password_hash TEXT NOT NULL,
    password_salt TEXT NOT NULL,
    linked_user_id INT,
    linked_engineer_id INT,
    is_blocked BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    last_login_at TIMESTAMP WITH TIME ZONE
);

CREATE TABLE IF NOT EXISTS superuser_credentials (
    superuser_id SERIAL PRIMARY KEY,
    username VARCHAR(100) NOT NULL UNIQUE,
    password_hash TEXT NOT NULL,
    password_salt TEXT NOT NULL,
    security_stamp VARCHAR(100) NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    last_login_at TIMESTAMP WITH TIME ZONE
);
");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DbInitializer] Предупреждение при создании таблиц: {ex.Message}");
        }

        // 2. Создание дефолтного Superuser (admin / Admin123!)
        try
        {
            if (!await context.SuperuserCredentials.AnyAsync(u => u.Username == "admin"))
            {
                var (adminHash, adminSalt) = hasher.HashPassword("Admin123!");
                context.SuperuserCredentials.Add(new SuperuserCredential
                {
                    Username = "admin",
                    PasswordHash = adminHash,
                    PasswordSalt = adminSalt,
                    SecurityStamp = Guid.NewGuid().ToString(),
                    CreatedAt = DateTime.UtcNow
                });
                await context.SaveChangesAsync();
                Console.WriteLine("[DbInitializer] Суперпользователь 'admin' успешно создан.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DbInitializer] Ошибка создания суперпользователя: {ex.Message}");
        }

        // 3. Создание дефолтного обычного пользователя (pilot / Pilot123!)
        try
        {
            if (!await context.UserCredentials.AnyAsync(u => u.Username == "pilot"))
            {
                var (userHash, userSalt) = hasher.HashPassword("Pilot123!");
                context.UserCredentials.Add(new UserCredential
                {
                    Username = "pilot",
                    Email = "pilot@uav-civil.ru",
                    PasswordHash = userHash,
                    PasswordSalt = userSalt,
                    CreatedAt = DateTime.UtcNow
                });
                await context.SaveChangesAsync();
                Console.WriteLine("[DbInitializer] Обычный пользователь 'pilot' успешно создан.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DbInitializer] Ошибка создания пользователя 'pilot': {ex.Message}");
        }

        // 4. Тестовый производитель, модель, борт (если база пуста)
        try
        {
            if (!await context.Manufacturers.AnyAsync())
            {
                var manufacturer = new Manufacturer
                {
                    Name = "ООО \"Геоскан\"",
                    Country = "Россия",
                    Inn = "7814472390",
                    Website = "https://geoscan.aero",
                    ContactPhone = "+7 (812) 363-33-33",
                    CreatedAt = DateTime.UtcNow
                };
                context.Manufacturers.Add(manufacturer);
                await context.SaveChangesAsync();

                var model = new UavModel
                {
                    ManufacturerId = manufacturer.ManufacturerId,
                    ModelName = "Геоскан 401",
                    AerodynamicSchema = UavAerodynamicSchema.Multirotor,
                    WeightCategory = UavWeightCategory.Range150gTo30kg,
                    ControlType = UavControlType.SemiAutonomous,
                    MtowKg = 9.30m,
                    MaxFlightTimeMins = 60,
                    MaxRangeKm = 24.00m,
                    CruisingSpeedKmh = 40,
                    MaxAltitudeM = 1000,
                    MaxWindSpeedMs = 12.0m,
                    MinTempCelsius = -20,
                    MaxTempCelsius = 40,
                    LengthMm = 1250,
                    WidthWingspanMm = 1250,
                    HeightMm = 550,
                    BasePriceRub = 1850000.00m
                };
                context.UavModels.Add(model);

                var engineer = new Engineer
                {
                    FirstName = "Алексей",
                    LastName = "Смирнов",
                    MiddleName = "Игоревич",
                    QualificationCategory = "Инженер ТОиР 1-й категории",
                    CertificationNumber = "CERT-UAV-2024-001",
                    CertificationExpiryDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(2)),
                    Phone = "+7 (999) 111-22-33",
                    Email = "smirnov.a@fleet.ru",
                    IsActive = true
                };
                context.Engineers.Add(engineer);

                var owner = new UavOwner
                {
                    LegalType = OwnerLegalType.LE,
                    Name = "ООО \"Аэромониторинг\"",
                    Inn = "7701234567",
                    Ogrn = "1157746001234",
                    ContactEmail = "info@aeromonitoring.ru",
                    ContactPhone = "+7 (495) 789-01-23",
                    LegalAddress = "г. Москва, ул. Авиаконструкторов, д. 12",
                    CreatedAt = DateTime.UtcNow
                };
                context.UavOwners.Add(owner);
                await context.SaveChangesAsync();

                var instance = new UavInstance
                {
                    ModelId = model.ModelId,
                    OwnerId = owner.OwnerId,
                    SerialNumber = "GS-401-2024-0042",
                    RosaviatsiaNumber = "RA-0142A",
                    ProductionDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-6)),
                    TotalFlightTimeHours = 45.50m,
                    CurrentStatus = UavInstanceStatus.Active,
                    HullInsurancePolicy = "POLIS-RESO-UAV-991823",
                    InsuranceExpiryDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(6)),
                    PrimaryColor = "Белый / Оранжевый",
                    LastUpdate = DateTime.UtcNow
                };
                context.UavInstances.Add(instance);
                await context.SaveChangesAsync();
                Console.WriteLine("[DbInitializer] Стартовые справочники и борт успешно инициализированы.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DbInitializer] Замечание при сидировании справочников: {ex.Message}");
        }
    }
}
