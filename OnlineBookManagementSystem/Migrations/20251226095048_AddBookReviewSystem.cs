using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineBookManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddBookReviewSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BookRatingCache",
                columns: table => new
                {
                    BookId = table.Column<int>(type: "INTEGER", nullable: false),
                    AverageRating = table.Column<double>(type: "real", nullable: false),
                    TotalReviews = table.Column<int>(type: "INTEGER", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "DateTime('now')")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookRatingCache", x => x.BookId);
                    table.ForeignKey(
                        name: "FK_BookRatingCache_Books_BookId",
                        column: x => x.BookId,
                        principalTable: "Books",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BookReviews",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BookId = table.Column<int>(type: "INTEGER", nullable: false),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    Rating = table.Column<int>(type: "INTEGER", nullable: false),
                    ReviewText = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    RejectionReason = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    ModeratedBy = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "DateTime('now')"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "DateTime('now')"),
                    ModeratedAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookReviews", x => x.Id);
                    table.CheckConstraint("CK_BookReview_Rating", "Rating >= 1 AND Rating <= 5");
                    table.CheckConstraint("CK_BookReview_ReviewText_Length", "LENGTH(ReviewText) >= 10 AND LENGTH(ReviewText) <= 1000");
                    table.ForeignKey(
                        name: "FK_BookReviews_AspNetUsers_ModeratedBy",
                        column: x => x.ModeratedBy,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_BookReviews_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BookReviews_Books_BookId",
                        column: x => x.BookId,
                        principalTable: "Books",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BookReviews_BookId_Status",
                table: "BookReviews",
                columns: new[] { "BookId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_BookReviews_BookId_UserId",
                table: "BookReviews",
                columns: new[] { "BookId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BookReviews_ModeratedBy",
                table: "BookReviews",
                column: "ModeratedBy");

            migrationBuilder.CreateIndex(
                name: "IX_BookReviews_Rating",
                table: "BookReviews",
                column: "Rating");

            migrationBuilder.CreateIndex(
                name: "IX_BookReviews_Status_CreatedAt",
                table: "BookReviews",
                columns: new[] { "Status", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_BookReviews_UserId",
                table: "BookReviews",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BookRatingCache");

            migrationBuilder.DropTable(
                name: "BookReviews");
        }
    }
}
