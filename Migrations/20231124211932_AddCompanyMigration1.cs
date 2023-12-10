using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataBaseForMovie.Migrations
{
    /// <inheritdoc />
    public partial class AddCompanyMigration1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HumanMovie_Humans_actorsSetname",
                table: "HumanMovie");

            migrationBuilder.DropForeignKey(
                name: "FK_HumanMovie_Movies_currentMoviestittle",
                table: "HumanMovie");

            migrationBuilder.RenameColumn(
                name: "currentMoviestittle",
                table: "HumanMovie",
                newName: "currentMoviesConnectiontittle");

            migrationBuilder.RenameColumn(
                name: "actorsSetname",
                table: "HumanMovie",
                newName: "actorsSetConnectionname");

            migrationBuilder.RenameIndex(
                name: "IX_HumanMovie_currentMoviestittle",
                table: "HumanMovie",
                newName: "IX_HumanMovie_currentMoviesConnectiontittle");

            migrationBuilder.AddForeignKey(
                name: "FK_HumanMovie_Humans_actorsSetConnectionname",
                table: "HumanMovie",
                column: "actorsSetConnectionname",
                principalTable: "Humans",
                principalColumn: "name",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_HumanMovie_Movies_currentMoviesConnectiontittle",
                table: "HumanMovie",
                column: "currentMoviesConnectiontittle",
                principalTable: "Movies",
                principalColumn: "tittle",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HumanMovie_Humans_actorsSetConnectionname",
                table: "HumanMovie");

            migrationBuilder.DropForeignKey(
                name: "FK_HumanMovie_Movies_currentMoviesConnectiontittle",
                table: "HumanMovie");

            migrationBuilder.RenameColumn(
                name: "currentMoviesConnectiontittle",
                table: "HumanMovie",
                newName: "currentMoviestittle");

            migrationBuilder.RenameColumn(
                name: "actorsSetConnectionname",
                table: "HumanMovie",
                newName: "actorsSetname");

            migrationBuilder.RenameIndex(
                name: "IX_HumanMovie_currentMoviesConnectiontittle",
                table: "HumanMovie",
                newName: "IX_HumanMovie_currentMoviestittle");

            migrationBuilder.AddForeignKey(
                name: "FK_HumanMovie_Humans_actorsSetname",
                table: "HumanMovie",
                column: "actorsSetname",
                principalTable: "Humans",
                principalColumn: "name",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_HumanMovie_Movies_currentMoviestittle",
                table: "HumanMovie",
                column: "currentMoviestittle",
                principalTable: "Movies",
                principalColumn: "tittle",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
