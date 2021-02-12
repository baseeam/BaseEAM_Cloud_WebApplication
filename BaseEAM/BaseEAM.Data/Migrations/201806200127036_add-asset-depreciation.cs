namespace BaseEAM.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addassetdepreciation : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Asset", "NoDepreciation", c => c.Boolean(nullable: false));
            AddColumn("dbo.Asset", "DepreciationType", c => c.Int());
            AddColumn("dbo.Asset", "DepreciationStartDate", c => c.DateTime(precision: 0));
            AddColumn("dbo.Asset", "DepreciationLifeSpan", c => c.Decimal(precision: 19, scale: 4));
            AddColumn("dbo.Asset", "DepreciationPeriodType", c => c.Int());
            AddColumn("dbo.Asset", "DepreciationPeriodCount", c => c.Int());
            AddColumn("dbo.Asset", "DepreciationOriginalValue", c => c.Decimal(precision: 19, scale: 4));
            AddColumn("dbo.Asset", "DepreciationEndValue", c => c.Decimal(precision: 19, scale: 4));
            AddColumn("dbo.Asset", "AccumulatedDepreciation", c => c.Decimal(precision: 19, scale: 4));
            AddColumn("dbo.Asset", "CurrentPeriodDepreciation", c => c.Decimal(precision: 19, scale: 4));
            AddColumn("dbo.Asset", "UndepreciatedBalance", c => c.Decimal(precision: 19, scale: 4));
            AddColumn("dbo.Asset", "DepreciationCalculatedDateTime", c => c.DateTime(precision: 0));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Asset", "DepreciationCalculatedDateTime");
            DropColumn("dbo.Asset", "UndepreciatedBalance");
            DropColumn("dbo.Asset", "CurrentPeriodDepreciation");
            DropColumn("dbo.Asset", "AccumulatedDepreciation");
            DropColumn("dbo.Asset", "DepreciationEndValue");
            DropColumn("dbo.Asset", "DepreciationOriginalValue");
            DropColumn("dbo.Asset", "DepreciationPeriodCount");
            DropColumn("dbo.Asset", "DepreciationPeriodType");
            DropColumn("dbo.Asset", "DepreciationLifeSpan");
            DropColumn("dbo.Asset", "DepreciationStartDate");
            DropColumn("dbo.Asset", "DepreciationType");
            DropColumn("dbo.Asset", "NoDepreciation");
        }
    }
}
