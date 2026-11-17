package br.com.bairroemdia;

import br.com.bairroemdia.domain.Analise;
import br.com.bairroemdia.dto.Contratos.*;
import java.util.List;
import java.util.UUID;
import org.junit.jupiter.api.Test;
import static org.junit.jupiter.api.Assertions.*;

class AnaliseDomainTest {
    @Test void agregaStatusECategoriasSemConfundirAnaliseComEncerramento() {
        var dados = List.of(
            new OcorrenciaCSharp(UUID.randomUUID(), Situacao.Registrada, Categoria.Buraco),
            new OcorrenciaCSharp(UUID.randomUUID(), Situacao.EmAnalise, Categoria.Buraco),
            new OcorrenciaCSharp(UUID.randomUUID(), Situacao.EmAcompanhamento, Categoria.Iluminacao),
            new OcorrenciaCSharp(UUID.randomUUID(), Situacao.Encerrada, Categoria.Iluminacao));
        var resposta = new Analise(new FiltroRequest(" sp ", " São Paulo ", null), dados).resposta();
        assertEquals(4, resposta.total());
        assertEquals(3, resposta.abertas());
        assertEquals(1, resposta.encerradas());
        assertEquals(2, resposta.porCategoria().get(Categoria.Buraco));
        assertEquals(0, resposta.porCategoria().get(Categoria.LixoAcumulado));
        assertEquals("SP", resposta.filtros().uf());
        assertEquals("São Paulo", resposta.filtros().cidade());
    }
    @Test void listaVaziaEUmResultadoValidoComZeros() {
        var resultado = new Analise(new FiltroRequest(null, null, null), List.of()).resposta();
        assertEquals(0, resultado.total());
        assertEquals(0, resultado.abertas());
        assertEquals(4, resultado.porStatus().size());
    }
    @Test void rejeitaOcorrenciaDuplicadaOuIncompleta() {
        var o = new OcorrenciaCSharp(UUID.randomUUID(), Situacao.EmAnalise, Categoria.Iluminacao);
        var filtro = new FiltroRequest(null, null, null);
        assertThrows(IllegalArgumentException.class, () -> new Analise(filtro, List.of(o, o)));
        assertThrows(IllegalArgumentException.class, () -> new Analise(filtro, List.of(new OcorrenciaCSharp(null, null, null))));
    }
}
