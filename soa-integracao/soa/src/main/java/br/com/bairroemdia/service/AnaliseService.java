package br.com.bairroemdia.service;

import br.com.bairroemdia.domain.Analise;
import br.com.bairroemdia.dto.Contratos.*;
import br.com.bairroemdia.exception.FonteIndisponivelException;
import br.com.bairroemdia.repository.AnaliseRepository;
import java.util.List;
import java.util.UUID;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;
import org.springframework.web.server.ResponseStatusException;
import org.springframework.http.HttpStatus;

@Service
public class AnaliseService {
    private final CSharpClient csharp;
    private final AnaliseRepository repository;
    public AnaliseService(CSharpClient csharp, AnaliseRepository repository) {
        this.csharp = csharp; this.repository = repository;
    }

    @Transactional
    public AnaliseResponse gerar(FiltroRequest filtros) {
        var ocorrencias = csharp.listar(filtros);
        Analise analise;
        try { analise = new Analise(filtros, ocorrencias); }
        catch (IllegalArgumentException e) { throw new FonteIndisponivelException(); }
        return repository.saveAndFlush(analise).resposta();
    }

    @Transactional(readOnly = true)
    public List<AnaliseResponse> listar() {
        return repository.findTop20ByOrderByGeradaEmDescIdDesc().stream().map(Analise::resposta).toList();
    }

    @Transactional(readOnly = true)
    public AnaliseResponse buscar(UUID id) {
        return repository.findById(id).orElseThrow(() -> new ResponseStatusException(HttpStatus.NOT_FOUND, "Análise não encontrada.")).resposta();
    }
}
