using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BairroEmDia.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AdicionaRecuperacaoSenha : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RecuperacoesSenha",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TokenHash = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ExpiraEm = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecuperacoesSenha", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecuperacoesSenha_ContasUsuario_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "ContasUsuario",
                        principalColumn: "UsuarioId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RecuperacoesSenha_ExpiraEm",
                table: "RecuperacoesSenha",
                column: "ExpiraEm");

            migrationBuilder.CreateIndex(
                name: "IX_RecuperacoesSenha_TokenHash",
                table: "RecuperacoesSenha",
                column: "TokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RecuperacoesSenha_UsuarioId",
                table: "RecuperacoesSenha",
                column: "UsuarioId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RecuperacoesSenha");
        }
    }
}
