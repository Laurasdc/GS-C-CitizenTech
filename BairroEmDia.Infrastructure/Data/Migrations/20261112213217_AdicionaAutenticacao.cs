using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BairroEmDia.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AdicionaAutenticacao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ContasUsuario",
                columns: table => new
                {
                    UsuarioId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 254, nullable: false),
                    SenhaHash = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: false),
                    Perfil = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContasUsuario", x => x.UsuarioId);
                    table.ForeignKey(
                        name: "FK_ContasUsuario_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SessoesUsuario",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TokenHash = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    ExpiraEm = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessoesUsuario", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SessoesUsuario_ContasUsuario_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "ContasUsuario",
                        principalColumn: "UsuarioId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContasUsuario_Email",
                table: "ContasUsuario",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SessoesUsuario_ExpiraEm",
                table: "SessoesUsuario",
                column: "ExpiraEm");

            migrationBuilder.CreateIndex(
                name: "IX_SessoesUsuario_TokenHash",
                table: "SessoesUsuario",
                column: "TokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SessoesUsuario_UsuarioId",
                table: "SessoesUsuario",
                column: "UsuarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SessoesUsuario");

            migrationBuilder.DropTable(
                name: "ContasUsuario");
        }
    }
}
