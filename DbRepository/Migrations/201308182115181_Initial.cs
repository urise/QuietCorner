namespace DbLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Users",
                c => new
                    {
                        UserId = c.Int(nullable: false, identity: true),
                        Login = c.String(nullable: false, maxLength: 128),
                        Password = c.String(maxLength: 1024),
                        Email = c.String(maxLength: 128),
                        UserFio = c.String(maxLength: 128),
                        RegistrationCode = c.String(maxLength: 10),
                        IsActive = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.UserId);
            
            CreateTable(
                "dbo.Instances",
                c => new
                    {
                        InstanceId = c.Int(nullable: false, identity: true),
                        InstanceName = c.String(maxLength: 256),
                    })
                .PrimaryKey(t => t.InstanceId);
            
            CreateTable(
                "dbo.UserInstances",
                c => new
                    {
                        UserInstanceId = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        InstanceId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.UserInstanceId)
                .ForeignKey("dbo.Users", t => t.UserId)
                .ForeignKey("dbo.Instances", t => t.InstanceId)
                .Index(t => t.UserId)
                .Index(t => t.InstanceId);
            
            CreateTable(
                "dbo.InstanceUsages",
                c => new
                    {
                        InstanceUsageId = c.Int(nullable: false, identity: true),
                        InstanceId = c.Int(nullable: false),
                        UserId = c.Int(nullable: false),
                        LoginDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.InstanceUsageId)
                .ForeignKey("dbo.Instances", t => t.InstanceId)
                .ForeignKey("dbo.Users", t => t.UserId)
                .Index(t => t.InstanceId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.TemporaryCodes",
                c => new
                    {
                        TemporaryCodeId = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        Code = c.String(nullable: false, maxLength: 10),
                        ExpireDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.TemporaryCodeId)
                .ForeignKey("dbo.Users", t => t.UserId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.DataLogs",
                c => new
                    {
                        DataLogId = c.Int(nullable: false, identity: true),
                        InstanceId = c.Int(),
                        UserId = c.Int(),
                        OperationTime = c.DateTime(nullable: false),
                        TableName = c.String(maxLength: 128),
                        RecordId = c.Int(nullable: false),
                        Operation = c.String(maxLength: 1),
                        Details = c.String(storeType: "xml"),
                        TransactionNumber = c.Int(),
                    })
                .PrimaryKey(t => t.DataLogId)
                .ForeignKey("dbo.Instances", t => t.InstanceId)
                .ForeignKey("dbo.Users", t => t.UserId)
                .Index(t => t.InstanceId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.Components",
                c => new
                    {
                        ComponentId = c.Int(nullable: false),
                        ComponentName = c.String(nullable: false, maxLength: 128),
                        IsReadOnlyAccess = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.ComponentId);
            
            CreateTable(
                "dbo.Roles",
                c => new
                    {
                        RoleId = c.Int(nullable: false, identity: true),
                        RoleName = c.String(nullable: false, maxLength: 128),
                        RoleType = c.Int(nullable: false),
                        InstanceId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.RoleId)
                .ForeignKey("dbo.Instances", t => t.InstanceId)
                .Index(t => t.InstanceId);
            
            CreateTable(
                "dbo.UserRoles",
                c => new
                    {
                        UserRoleId = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        RoleId = c.Int(nullable: false),
                        InstanceId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.UserRoleId)
                .ForeignKey("dbo.Users", t => t.UserId)
                .ForeignKey("dbo.Roles", t => t.RoleId)
                .ForeignKey("dbo.Instances", t => t.InstanceId)
                .Index(t => t.UserId)
                .Index(t => t.RoleId)
                .Index(t => t.InstanceId);
            
            CreateTable(
                "dbo.ComponentRoles",
                c => new
                    {
                        ComponentRoleId = c.Int(nullable: false, identity: true),
                        ComponentId = c.Int(nullable: false),
                        RoleId = c.Int(nullable: false),
                        AccessLevel = c.Int(nullable: false),
                        InstanceId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ComponentRoleId)
                .ForeignKey("dbo.Components", t => t.ComponentId)
                .ForeignKey("dbo.Roles", t => t.RoleId)
                .ForeignKey("dbo.Instances", t => t.InstanceId)
                .Index(t => t.ComponentId)
                .Index(t => t.RoleId)
                .Index(t => t.InstanceId);
            
        }
        
        public override void Down()
        {
            DropIndex("dbo.ComponentRoles", new[] { "InstanceId" });
            DropIndex("dbo.ComponentRoles", new[] { "RoleId" });
            DropIndex("dbo.ComponentRoles", new[] { "ComponentId" });
            DropIndex("dbo.UserRoles", new[] { "InstanceId" });
            DropIndex("dbo.UserRoles", new[] { "RoleId" });
            DropIndex("dbo.UserRoles", new[] { "UserId" });
            DropIndex("dbo.Roles", new[] { "InstanceId" });
            DropIndex("dbo.DataLogs", new[] { "UserId" });
            DropIndex("dbo.DataLogs", new[] { "InstanceId" });
            DropIndex("dbo.TemporaryCodes", new[] { "UserId" });
            DropIndex("dbo.InstanceUsages", new[] { "UserId" });
            DropIndex("dbo.InstanceUsages", new[] { "InstanceId" });
            DropIndex("dbo.UserInstances", new[] { "InstanceId" });
            DropIndex("dbo.UserInstances", new[] { "UserId" });
            DropForeignKey("dbo.ComponentRoles", "InstanceId", "dbo.Instances");
            DropForeignKey("dbo.ComponentRoles", "RoleId", "dbo.Roles");
            DropForeignKey("dbo.ComponentRoles", "ComponentId", "dbo.Components");
            DropForeignKey("dbo.UserRoles", "InstanceId", "dbo.Instances");
            DropForeignKey("dbo.UserRoles", "RoleId", "dbo.Roles");
            DropForeignKey("dbo.UserRoles", "UserId", "dbo.Users");
            DropForeignKey("dbo.Roles", "InstanceId", "dbo.Instances");
            DropForeignKey("dbo.DataLogs", "UserId", "dbo.Users");
            DropForeignKey("dbo.DataLogs", "InstanceId", "dbo.Instances");
            DropForeignKey("dbo.TemporaryCodes", "UserId", "dbo.Users");
            DropForeignKey("dbo.InstanceUsages", "UserId", "dbo.Users");
            DropForeignKey("dbo.InstanceUsages", "InstanceId", "dbo.Instances");
            DropForeignKey("dbo.UserInstances", "InstanceId", "dbo.Instances");
            DropForeignKey("dbo.UserInstances", "UserId", "dbo.Users");
            DropTable("dbo.ComponentRoles");
            DropTable("dbo.UserRoles");
            DropTable("dbo.Roles");
            DropTable("dbo.Components");
            DropTable("dbo.DataLogs");
            DropTable("dbo.TemporaryCodes");
            DropTable("dbo.InstanceUsages");
            DropTable("dbo.UserInstances");
            DropTable("dbo.Instances");
            DropTable("dbo.Users");
        }
    }
}
