namespace BookStore.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemovePriceRequired : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Books", "Price", c => c.Single());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Books", "Price", c => c.Single(nullable: false));
        }
    }
}
