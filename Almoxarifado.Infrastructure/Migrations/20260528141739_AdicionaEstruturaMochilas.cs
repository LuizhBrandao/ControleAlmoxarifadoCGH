using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Almoxarifado.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionaEstruturaMochilas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MochilaId",
                table: "Equipamentos",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Mochilas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Numero = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    TurnoAtual = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    DuplaAtual = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mochilas", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Equipamentos_MochilaId",
                table: "Equipamentos",
                column: "MochilaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Equipamentos_Mochilas_MochilaId",
                table: "Equipamentos",
                column: "MochilaId",
                principalTable: "Mochilas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Equipamentos_Mochilas_MochilaId",
                table: "Equipamentos");

            migrationBuilder.DropTable(
                name: "Mochilas");

            migrationBuilder.DropIndex(
                name: "IX_Equipamentos_MochilaId",
                table: "Equipamentos");

            migrationBuilder.DropColumn(
                name: "MochilaId",
                table: "Equipamentos");
        }
    }
}
