package br.com.bairroemdia.service;

import br.com.bairroemdia.dto.Contratos.*;
import br.com.bairroemdia.exception.FonteIndisponivelException;
import java.io.IOException;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;
import org.springframework.http.HttpStatus;
import org.springframework.http.MediaType;
import org.springframework.test.web.client.MockRestServiceServer;
import org.springframework.web.client.RestClient;
import static org.springframework.test.web.client.match.MockRestRequestMatchers.*;
import static org.springframework.test.web.client.response.MockRestResponseCreators.*;
import static org.junit.jupiter.api.Assertions.*;

class CSharpClientTest {
    private MockRestServiceServer server;
    private CSharpClient client;
    @BeforeEach void preparar() {
        var builder = RestClient.builder().baseUrl("http://csharp.local");
        server = MockRestServiceServer.bindTo(builder).build();
        client = new CSharpClient(builder.build());
    }
    @Test void aceitaOContratoRealDoCSharpEEncaminhaFiltrosComAcentos() {
        server.expect(requestTo("http://csharp.local/api/ocorrencias?uf=SP&cidade=S%C3%A3o%20Paulo&bairro=Ch%C3%A1cara%20Klabin"))
            .andRespond(withSuccess("""
                [{"id":"fa95dd14-ec5c-4ac3-a3c0-c381266c567e","titulo":"Poste apagado","bairro":"Chácara Klabin","cidade":"São Paulo","uf":"SP","categoria":"Iluminacao","status":"EmAnalise","criadoEm":"2026-11-17T10:00:00","atualizadoEm":"2026-11-17T10:00:00"}]
                """, MediaType.APPLICATION_JSON));
        var dados = client.listar(new FiltroRequest("SP", "São Paulo", "Chácara Klabin"));
        assertEquals(Situacao.EmAnalise, dados.getFirst().status());
        assertEquals(Categoria.Iluminacao, dados.getFirst().categoria());
        server.verify();
    }
    @Test void propagaIndisponibilidadeSemTransformarEmListaVazia() {
        server.expect(requestTo("http://csharp.local/api/ocorrencias")).andRespond(withStatus(HttpStatus.INTERNAL_SERVER_ERROR));
        assertThrows(FonteIndisponivelException.class, () -> client.listar(new FiltroRequest(null,null,null)));
    }
    @Test void falhaDeConexaoETratada() {
        server.expect(requestTo("http://csharp.local/api/ocorrencias")).andRespond(withException(new IOException("offline")));
        assertThrows(FonteIndisponivelException.class, () -> client.listar(new FiltroRequest(null,null,null)));
    }
    @Test void contratoIncompletoNaoViraUmaAnaliseComZero() {
        server.expect(requestTo("http://csharp.local/api/ocorrencias")).andRespond(withSuccess("[{\"status\":\"ABERTA\"}]",MediaType.APPLICATION_JSON));
        assertThrows(FonteIndisponivelException.class, () -> client.listar(new FiltroRequest(null,null,null)));
    }
}
