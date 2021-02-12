namespace BaseEAM.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updatereportcascade : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.ReportColumn", "ReportId", "dbo.Report");
            DropForeignKey("dbo.ReportFilter", "ReportId", "dbo.Report");
            AddForeignKey("dbo.ReportColumn", "ReportId", "dbo.Report", "Id", cascadeDelete: true);
            AddForeignKey("dbo.ReportFilter", "ReportId", "dbo.Report", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ReportFilter", "ReportId", "dbo.Report");
            DropForeignKey("dbo.ReportColumn", "ReportId", "dbo.Report");
            AddForeignKey("dbo.ReportFilter", "ReportId", "dbo.Report", "Id");
            AddForeignKey("dbo.ReportColumn", "ReportId", "dbo.Report", "Id");
        }
    }
}
