using Npgsql;

namespace UavFleetManagement.Infrastructure.Persistence;

public class CustomEnumNameTranslator : INpgsqlNameTranslator
{
    public string TranslateTypeName(string clrName) => clrName;

    public string TranslateMemberName(string clrName) => clrName switch
    {
        "FixedWing" => "Fixed-wing",
        "Multirotor" => "Multirotor",
        "VTOL" => "VTOL",
        "Helicopter" => "Helicopter",
        "Under150g" => "Under_150g",
        "Range150gTo30kg" => "150g_to_30kg",
        "Over30kg" => "Over_30kg",
        "Active" => "Active",
        "Maintenance" => "Maintenance",
        "Storage" => "Storage",
        "Decommissioned" => "Decommissioned",
        "PreFlight" => "Pre-flight",
        "PostFlight" => "Post-flight",
        "TO1" => "TO-1",
        "TO2" => "TO-2",
        "Seasonal" => "Seasonal",
        "Individual" => "Individual",
        "IP" => "IP",
        "LE" => "LE",
        "Autonomous" => "Autonomous",
        "SemiAutonomous" => "Semi-Autonomous",
        "ManualRadio" => "Manual_Radio",
        "ManualFPV" => "Manual_FPV",
        "New" => "New",
        "Refurbished" => "Refurbished",
        "Used" => "Used",
        _ => clrName
    };
}
