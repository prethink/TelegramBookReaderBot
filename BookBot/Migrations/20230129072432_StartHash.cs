using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookBot.Migrations
{
    public partial class StartHash : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Hash",
                table: "Books",
                type: "longtext",
                nullable: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Hash",
                table: "Books");
        }
    }
}
