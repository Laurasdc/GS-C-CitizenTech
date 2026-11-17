package br.com.bairroemdia.service;

import br.com.bairroemdia.dto.Contratos.*;
import br.com.bairroemdia.exception.FonteIndisponivelException;
import java.net.URI;
import java.util.Arrays;
import java.util.List;
import java.util.Optional;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.http.client.SimpleClientHttpRequestFactory;
import org.springframework.stereotype.Component;
import org.springframework.web.client.RestClient;
import org.springframework.web.client.RestClientException;

@Component
public class CSharpClient {
    private final RestClient client;

    @org.springframework.beans.factory.annotation.Autowired
    public CSharpClient(RestClient.Builder builder,
            @Value("${csharp.base-url}") String baseUrl) {
        var uri = URI.create(baseUrl);
        if (uri.getHost() == null || !("http".equals(uri.getScheme()) || "https".equals(uri.getScheme())))
            throw new IllegalArgumentException("CSHARP_API_URL deve ser uma URL HTTP(S) absoluta.");
        var factory = new SimpleClientHttpRequestFactory();
        factory.setConnectTimeout(3000);
        factory.setReadTimeout(8000);
        client = builder.baseUrl(baseUrl.replaceAll("/+$", "")).requestFactory(factory).build();
    }

    // Construtor usado nos testes de contrato HTTP, sem abrir uma conexão real.
    CSharpClient(RestClient client) { this.client = client; }

    public List<OcorrenciaCSharp> listar(FiltroRequest filtro) {
        try {
            var dados = client.get().uri(builder -> builder.path("/api/ocorrencias")
                .queryParamIfPresent("uf", Optional.ofNullable(filtro.uf()))
                .queryParamIfPresent("cidade", Optional.ofNullable(filtro.cidade()))
                .queryParamIfPresent("bairro", Optional.ofNullable(filtro.bairro()))
                .build()).retrieve().body(OcorrenciaCSharp[].class);
            if (dados == null || Arrays.stream(dados).anyMatch(o -> o == null || o.id() == null || o.status() == null || o.categoria() == null))
                throw new FonteIndisponivelException();
            return List.of(dados);
        } catch (RestClientException | IllegalArgumentException e) {
            throw new FonteIndisponivelException();
        }
    }
}
