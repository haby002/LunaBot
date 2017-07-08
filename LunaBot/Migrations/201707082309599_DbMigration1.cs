namespace LunaBot.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DbMigration1 : DbMigration
    {
        public override void Up()
        {
            DropPrimaryKey("dbo.User");
            AlterColumn("dbo.User", "ID", c => c.Long(nullable: false, identity: true));
            AddPrimaryKey("dbo.User", "ID");
        }
        
        public override void Down()
        {
            DropPrimaryKey("dbo.User");
            AlterColumn("dbo.User", "ID", c => c.Int(nullable: false, identity: true));
            AddPrimaryKey("dbo.User", "ID");
        }
    }
}
