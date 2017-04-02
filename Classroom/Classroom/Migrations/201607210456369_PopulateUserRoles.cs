namespace Classroom.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class PopulateUserRoles : DbMigration
    {
        public override void Up()
        {
            Sql("INSERT INTO AspNetRoles (Id, Name) VALUES (1, 'Admin')");
            Sql("INSERT INTO AspNetRoles (Id, Name) VALUES (2, 'Instructor')");
            Sql("INSERT INTO AspNetRoles (Id, Name) VALUES (3, 'Student')");
        }
        
        public override void Down()
        {
            Sql("DELETE FROM AspNetRoles WHERE Id IN (1, 2, 3)");
        }
    }
}
