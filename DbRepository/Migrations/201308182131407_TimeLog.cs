namespace DbLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TimeLog : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Activities",
                c => new
                    {
                        ActivityId = c.Int(nullable: false, identity: true),
                        InstanceId = c.Int(nullable: false),
                        ActivityName = c.String(nullable: false, maxLength: 128),
                        IsActive = c.Boolean(nullable: false),
                        Color = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ActivityId)
                .ForeignKey("dbo.Instances", t => t.InstanceId)
                .Index(t => t.InstanceId);
            
            CreateTable(
                "dbo.TimeLogs",
                c => new
                    {
                        TimeLogId = c.Int(nullable: false, identity: true),
                        InstanceId = c.Int(nullable: false),
                        UserId = c.Int(nullable: false),
                        ActivityId = c.Int(nullable: false),
                        TimeFrom = c.DateTime(nullable: false),
                        TimeTo = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.TimeLogId)
                .ForeignKey("dbo.Instances", t => t.InstanceId)
                .ForeignKey("dbo.Users", t => t.UserId)
                .ForeignKey("dbo.Activities", t => t.ActivityId)
                .Index(t => t.InstanceId)
                .Index(t => t.UserId)
                .Index(t => t.ActivityId);
            
        }
        
        public override void Down()
        {
            DropIndex("dbo.TimeLogs", new[] { "ActivityId" });
            DropIndex("dbo.TimeLogs", new[] { "UserId" });
            DropIndex("dbo.TimeLogs", new[] { "InstanceId" });
            DropIndex("dbo.Activities", new[] { "InstanceId" });
            DropForeignKey("dbo.TimeLogs", "ActivityId", "dbo.Activities");
            DropForeignKey("dbo.TimeLogs", "UserId", "dbo.Users");
            DropForeignKey("dbo.TimeLogs", "InstanceId", "dbo.Instances");
            DropForeignKey("dbo.Activities", "InstanceId", "dbo.Instances");
            DropTable("dbo.TimeLogs");
            DropTable("dbo.Activities");
        }
    }
}
