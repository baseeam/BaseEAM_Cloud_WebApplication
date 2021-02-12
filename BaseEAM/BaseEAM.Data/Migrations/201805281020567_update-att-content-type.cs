namespace BaseEAM.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updateattcontenttype : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Attachment", "ContentType", c => c.String(maxLength: 128, storeType: "nvarchar"));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Attachment", "ContentType", c => c.String(maxLength: 64, storeType: "nvarchar"));
        }
    }
}
