using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace BookBot.Migrations
{
    public partial class Start : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Authors",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Authors", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Books",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Year = table.Column<long>(type: "bigint", nullable: true),
                    Path = table.Column<string>(type: "longtext", nullable: false),
                    Title = table.Column<string>(type: "longtext", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Books", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Genres",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Genres", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "link_statistic",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    link = table.Column<string>(type: "longtext", nullable: false),
                    description = table.Column<string>(type: "longtext", nullable: false),
                    reg_count = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_link_statistic", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "AuthorBook",
                columns: table => new
                {
                    Authorsid = table.Column<long>(type: "bigint", nullable: false),
                    Booksid = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthorBook", x => new { x.Authorsid, x.Booksid });
                    table.ForeignKey(
                        name: "FK_AuthorBook_Authors_Authorsid",
                        column: x => x.Authorsid,
                        principalTable: "Authors",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AuthorBook_Books_Booksid",
                        column: x => x.Booksid,
                        principalTable: "Books",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    telegram_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    parent_user_id = table.Column<long>(type: "bigint", nullable: true),
                    registered_date = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    last_activity = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    login = table.Column<string>(type: "longtext", nullable: true),
                    firstname = table.Column<string>(type: "longtext", nullable: true),
                    lastname = table.Column<string>(type: "longtext", nullable: true),
                    is_ban = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    activity = table.Column<long>(type: "bigint", nullable: false),
                    link = table.Column<string>(type: "longtext", nullable: false),
                    current_book_id = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.telegram_id);
                    table.ForeignKey(
                        name: "FK_users_Books_current_book_id",
                        column: x => x.current_book_id,
                        principalTable: "Books",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_users_users_parent_user_id",
                        column: x => x.parent_user_id,
                        principalTable: "users",
                        principalColumn: "telegram_id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "BookGenre",
                columns: table => new
                {
                    Booksid = table.Column<long>(type: "bigint", nullable: false),
                    Genresid = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookGenre", x => new { x.Booksid, x.Genresid });
                    table.ForeignKey(
                        name: "FK_BookGenre_Books_Booksid",
                        column: x => x.Booksid,
                        principalTable: "Books",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BookGenre_Genres_Genresid",
                        column: x => x.Genresid,
                        principalTable: "Genres",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "settings_user",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    book_id = table.Column<long>(type: "bigint", nullable: false),
                    repeat_time = table.Column<TimeSpan>(type: "time(6)", nullable: false),
                    next_notify_time = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    current_page = table.Column<int>(type: "int", nullable: false),
                    is_repeat = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_settings_user", x => x.id);
                    table.ForeignKey(
                        name: "FK_settings_user_Books_book_id",
                        column: x => x.book_id,
                        principalTable: "Books",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_settings_user_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "telegram_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "users_books",
                columns: table => new
                {
                    Booksid = table.Column<long>(type: "bigint", nullable: false),
                    UsersTelegramId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users_books", x => new { x.Booksid, x.UsersTelegramId });
                    table.ForeignKey(
                        name: "FK_users_books_Books_Booksid",
                        column: x => x.Booksid,
                        principalTable: "Books",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_users_books_users_UsersTelegramId",
                        column: x => x.UsersTelegramId,
                        principalTable: "users",
                        principalColumn: "telegram_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuthorBook_Booksid",
                table: "AuthorBook",
                column: "Booksid");

            migrationBuilder.CreateIndex(
                name: "IX_BookGenre_Genresid",
                table: "BookGenre",
                column: "Genresid");

            migrationBuilder.CreateIndex(
                name: "IX_settings_user_book_id",
                table: "settings_user",
                column: "book_id");

            migrationBuilder.CreateIndex(
                name: "IX_settings_user_user_id",
                table: "settings_user",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_users_current_book_id",
                table: "users",
                column: "current_book_id");

            migrationBuilder.CreateIndex(
                name: "IX_users_parent_user_id",
                table: "users",
                column: "parent_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_users_books_UsersTelegramId",
                table: "users_books",
                column: "UsersTelegramId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuthorBook");

            migrationBuilder.DropTable(
                name: "BookGenre");

            migrationBuilder.DropTable(
                name: "link_statistic");

            migrationBuilder.DropTable(
                name: "settings_user");

            migrationBuilder.DropTable(
                name: "users_books");

            migrationBuilder.DropTable(
                name: "Authors");

            migrationBuilder.DropTable(
                name: "Genres");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "Books");
        }
    }
}
