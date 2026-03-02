namespace BookStore.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addedimagetoautor : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Authors", "ImageUrl", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Authors", "ImageUrl");
        }
    }
}
