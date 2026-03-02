namespace BookStore.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class converteddecimalpricetofloatorderItem : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.OrderItems", "Price", c => c.Single(nullable: false));
            AlterColumn("dbo.Orders", "TotalAmount", c => c.Single(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Orders", "TotalAmount", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.OrderItems", "Price", c => c.Decimal(nullable: false, precision: 18, scale: 2));
        }
    }
}
