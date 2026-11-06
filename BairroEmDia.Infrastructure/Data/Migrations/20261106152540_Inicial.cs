using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BairroEmDia.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Nome = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Ocorrencias",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Titulo = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    Descricao = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    Bairro = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Localizacao = table.Column<string>(type: "TEXT", maxLength: 250, nullable: false),
                    Categoria = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "TEXT", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ocorrencias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ocorrencias_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Confirmacoes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    OcorrenciaId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Confirmacoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Confirmacoes_Ocorrencias_OcorrenciaId",
                        column: x => x.OcorrenciaId,
                        principalTable: "Ocorrencias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Confirmacoes_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HistoricosAtualizacao",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    OcorrenciaId = table.Column<Guid>(type: "TEXT", nullable: false),
                    StatusAnterior = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    NovoStatus = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Justificativa = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoricosAtualizacao", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HistoricosAtualizacao_Ocorrencias_OcorrenciaId",
                        column: x => x.OcorrenciaId,
                        principalTable: "Ocorrencias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Confirmacoes_OcorrenciaId_UsuarioId",
                table: "Confirmacoes",
                columns: new[] { "OcorrenciaId", "UsuarioId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Confirmacoes_UsuarioId",
                table: "Confirmacoes",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_HistoricosAtualizacao_OcorrenciaId",
                table: "HistoricosAtualizacao",
                column: "OcorrenciaId");

            migrationBuilder.CreateIndex(
                name: "IX_Ocorrencias_Bairro_Status",
                table: "Ocorrencias",
                columns: new[] { "Bairro", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Ocorrencias_Categoria",
                table: "Ocorrencias",
                column: "Categoria");

            migrationBuilder.CreateIndex(
                name: "IX_Ocorrencias_UsuarioId",
                table: "Ocorrencias",
                column: "UsuarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Confirmacoes");

            migrationBuilder.DropTable(
                name: "HistoricosAtualizacao");

            migrationBuilder.DropTable(
                name: "Ocorrencias");

            migrationBuilder.DropTable(
                name: "Usuarios");
        }
    }
}
