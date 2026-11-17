package br.com.bairroemdia.dto;

import com.fasterxml.jackson.annotation.JsonIgnoreProperties;
import jakarta.validation.constraints.Pattern;
import jakarta.validation.constraints.Size;
import java.time.OffsetDateTime;
import java.util.Map;
import java.util.UUID;

public final class Contratos {
    private Contratos() {}

    public enum Situacao { Registrada, EmAnalise, EmAcompanhamento, Encerrada }
    public enum Categoria { Iluminacao, LixoAcumulado, Buraco, CalcadaDanificada }

    @JsonIgnoreProperties(ignoreUnknown = true)
    public record OcorrenciaCSharp(UUID id, Situacao status, Categoria categoria) {}

    public record FiltroRequest(
        @Pattern(regexp = "AC|AL|AP|AM|BA|CE|DF|ES|GO|MA|MT|MS|MG|PA|PB|PR|PE|PI|RJ|RN|RS|RO|RR|SC|SP|SE|TO", message = "Informe uma UF brasileira válida.") String uf,
        @Size(max = 100) String cidade,
        @Size(max = 100) String bairro
    ) {
        public FiltroRequest {
            uf = limpar(uf);
            if (uf != null) uf = uf.toUpperCase(java.util.Locale.ROOT);
            cidade = limpar(cidade);
            bairro = limpar(bairro);
        }
        private static String limpar(String texto) {
            return texto == null || texto.isBlank() ? null : texto.trim();
        }
    }

    public record AnaliseResponse(UUID id, OffsetDateTime geradaEm, String origem,
        FiltroRequest filtros, int total, int abertas, int encerradas,
        Map<Situacao, Integer> porStatus, Map<Categoria, Integer> porCategoria) {}
}
