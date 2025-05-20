using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeploobmenStenkaIIIWeb.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BioCoeffs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Bio = table.Column<double>(type: "REAL", nullable: false),
                    Mu = table.Column<double>(type: "REAL", nullable: false),
                    MuSquared = table.Column<double>(type: "REAL", nullable: false),
                    Np = table.Column<double>(type: "REAL", nullable: false),
                    Pp = table.Column<double>(type: "REAL", nullable: false),
                    M = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BioCoeffs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BioCoeffs_Bio",
                table: "BioCoeffs",
                column: "Bio",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BioCoeffs");
        }
    }
}
