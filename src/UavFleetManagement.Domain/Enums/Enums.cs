namespace UavFleetManagement.Domain.Enums;

public enum UavAerodynamicSchema
{
    FixedWing,
    Multirotor,
    VTOL,
    Helicopter
}

public enum UavWeightCategory
{
    Under150g,
    Range150gTo30kg,
    Over30kg
}

public enum UavInstanceStatus
{
    Active,
    Maintenance,
    Storage,
    Decommissioned
}

public enum MaintenanceType
{
    PreFlight,
    PostFlight,
    TO1,
    TO2,
    Seasonal
}

public enum OwnerLegalType
{
    Individual,
    IP,
    LE
}

public enum UavControlType
{
    Autonomous,
    SemiAutonomous,
    ManualRadio,
    ManualFPV
}

public enum ComponentCondition
{
    New,
    Refurbished,
    Used
}
