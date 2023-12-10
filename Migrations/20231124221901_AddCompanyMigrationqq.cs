using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataBaseForMovie.Migrations
{
    /// <inheritdoc />
    public partial class AddCompanyMigrationqq : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HumanMovie_Humans_actorsSetConnectionname",
                table: "HumanMovie");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Humans",
                table: "Humans");

            migrationBuilder.DropPrimaryKey(
                name: "PK_HumanMovie",
                table: "HumanMovie");

            migrationBuilder.DropColumn(
                name: "actorsSetConnectionname",
                table: "HumanMovie");

            migrationBuilder.AddColumn<int>(
                name: "id",
                table: "Humans",
                type: "INTEGER",
                nullable: false,
                defaultValue: 1)
                .Annotation("Sqlite:Autoincrement", true);

            migrationBuilder.AddColumn<int>(
                name: "actorsSetConnectionid",
                table: "HumanMovie",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Humans",
                table: "Humans",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_HumanMovie",
                table: "HumanMovie",
                columns: new[] { "actorsSetConnectionid", "currentMoviesConnectiontittle" });

            migrationBuilder.AddForeignKey(
                name: "FK_HumanMovie_Humans_actorsSetConnectionid",
                table: "HumanMovie",
                column: "actorsSetConnectionid",
                principalTable: "Humans",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HumanMovie_Humans_actorsSetConnectionid",
                table: "HumanMovie");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Humans",
                table: "Humans");

            migrationBuilder.DropPrimaryKey(
                name: "PK_HumanMovie",
                table: "HumanMovie");

            migrationBuilder.DropColumn(
                name: "id",
                table: "Humans");

            migrationBuilder.DropColumn(
                name: "actorsSetConnectionid",
                table: "HumanMovie");

            migrationBuilder.AddColumn<string>(
                name: "actorsSetConnectionname",
                table: "HumanMovie",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Humans",
                table: "Humans",
                column: "name");

            migrationBuilder.AddPrimaryKey(
                name: "PK_HumanMovie",
                table: "HumanMovie",
                columns: new[] { "actorsSetConnectionname", "currentMoviesConnectiontittle" });

            migrationBuilder.AddForeignKey(
                name: "FK_HumanMovie_Humans_actorsSetConnectionname",
                table: "HumanMovie",
                column: "actorsSetConnectionname",
                principalTable: "Humans",
                principalColumn: "name",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
