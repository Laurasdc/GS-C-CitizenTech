using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BairroEmDia.Infrastructure.Data.Migrations;

public partial class AdicionaCidadeEUf : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // As ocorrências existentes são os registros de demonstração de São Paulo/SP.
        // As novas colunas são obrigatórias. O preenchimento legado não cria
        // um valor padrão permanente para novos cadastros.
        migrationBuilder.AddColumn<string>(
            name: "Cidade", table: "Ocorrencias", type: "TEXT",
            maxLength: 100, nullable: false, defaultValue: "");
        migrationBuilder.AddColumn<string>(
            name: "Uf", table: "Ocorrencias", type: "TEXT",
            maxLength: 2, nullable: false, defaultValue: "");
        migrationBuilder.Sql("UPDATE Ocorrencias SET Cidade = 'São Paulo', Uf = 'SP';");
        migrationBuilder.CreateIndex(
            name: "IX_Ocorrencias_Uf_Cidade_Bairro_Status",
            table: "Ocorrencias", columns: new[] { "Uf", "Cidade", "Bairro", "Status" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_Ocorrencias_Uf_Cidade_Bairro_Status", table: "Ocorrencias");
        migrationBuilder.DropColumn(name: "Cidade", table: "Ocorrencias");
        migrationBuilder.DropColumn(name: "Uf", table: "Ocorrencias");
    }
}
