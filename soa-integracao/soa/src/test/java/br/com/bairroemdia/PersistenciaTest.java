package br.com.bairroemdia;

import br.com.bairroemdia.dto.Contratos.*;
import br.com.bairroemdia.exception.FonteIndisponivelException;
import br.com.bairroemdia.repository.AnaliseRepository;
import br.com.bairroemdia.service.*;
import java.util.List;
import java.util.UUID;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.boot.test.context.SpringBootTest;
import org.springframework.test.context.ActiveProfiles;
import org.springframework.test.context.bean.override.mockito.MockitoBean;
import static org.mockito.Mockito.*;
import static org.junit.jupiter.api.Assertions.*;

@SpringBootTest(webEnvironment = SpringBootTest.WebEnvironment.NONE)
@ActiveProfiles("test")
class PersistenciaTest {
    @Autowired AnaliseService service;
    @Autowired AnaliseRepository repository;
    @MockitoBean CSharpClient client;
    @BeforeEach void limpar() { repository.deleteAll(); }

    @Test void migraGravaERelêAnaliseMesmoComCSharpIndisponivelDepois() {
        var filtro = new FiltroRequest("SP", "São Paulo", "Chácara Klabin");
        when(client.listar(filtro)).thenReturn(List.of(new OcorrenciaCSharp(UUID.randomUUID(),Situacao.EmAnalise,Categoria.Buraco)));
        var gerada = service.gerar(filtro);
        when(client.listar(filtro)).thenThrow(new FonteIndisponivelException());
        var salva = service.buscar(gerada.id());
        assertEquals(1,salva.total());
        assertEquals(1,salva.porStatus().get(Situacao.EmAnalise));
        assertEquals("Chácara Klabin",salva.filtros().bairro());
        assertEquals(1,service.listar().size());
        verify(client,times(1)).listar(filtro);
    }
    @Test void erroDaApiNaoGravaResultadoFalso() {
        var filtro = new FiltroRequest(null,null,null);
        when(client.listar(filtro)).thenThrow(new FonteIndisponivelException());
        assertThrows(FonteIndisponivelException.class,()->service.gerar(filtro));
        assertEquals(0,repository.count());
    }
}
