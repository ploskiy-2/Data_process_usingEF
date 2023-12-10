using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataBaseForMovie.Migrations
{
    /// <inheritdoc />
    public partial class AddCompanyMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HumanMovie",
                columns: table => new
                {
                    actorsSetname = table.Column<string>(type: "TEXT", nullable: false),
                    currentMoviestittle = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HumanMovie", x => new { x.actorsSetname, x.currentMoviestittle });
                    table.ForeignKey(
                        name: "FK_HumanMovie_Humans_actorsSetname",
                        column: x => x.actorsSetname,
                        principalTable: "Humans",
                        principalColumn: "name",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HumanMovie_Movies_currentMoviestittle",
                        column: x => x.currentMoviestittle,
                        principalTable: "Movies",
                        principalColumn: "tittle",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HumanMovie_currentMoviestittle",
                table: "HumanMovie",
                column: "currentMoviestittle");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HumanMovie");
        }
    }
}
