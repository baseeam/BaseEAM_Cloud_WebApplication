namespace BaseEAM.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updatewoequipmentdown : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.WorkOrder", "IsEquipmentDown", c => c.Boolean(nullable: false));
            AddColumn("dbo.WorkOrder", "EquipmentDownStartDateTime", c => c.DateTime(precision: 0));
            AddColumn("dbo.WorkOrder", "EquipmentDownEndDateTime", c => c.DateTime(precision: 0));
        }
        
        public override void Down()
        {
            DropColumn("dbo.WorkOrder", "EquipmentDownEndDateTime");
            DropColumn("dbo.WorkOrder", "EquipmentDownStartDateTime");
            DropColumn("dbo.WorkOrder", "IsEquipmentDown");
        }
    }
}
