namespace BaseEAM.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addassetcategory : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Asset", "AssetCategoryId", c => c.Long());
            CreateIndex("dbo.Asset", "AssetCategoryId");
            AddForeignKey("dbo.Asset", "AssetCategoryId", "dbo.ValueItem", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Asset", "AssetCategoryId", "dbo.ValueItem");
            DropIndex("dbo.Asset", new[] { "AssetCategoryId" });
            DropColumn("dbo.Asset", "AssetCategoryId");
        }
    }
}
