package br.com.bairroemdia.controller;

import br.com.bairroemdia.dto.Contratos.*;
import br.com.bairroemdia.service.AnaliseService;
import io.swagger.v3.oas.annotations.Operation;
import jakarta.validation.Valid;
import java.net.URI;
import java.util.List;
import java.util.Map;
import java.util.UUID;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

@RestController
@RequestMapping("/api")
public class AnaliseController {
    private final AnaliseService service;
    public AnaliseController(AnaliseService service) { this.service = service; }

    @GetMapping("/health")
    public Map<String, String> health() { return Map.of("status", "UP", "service", "bairro-em-dia-soa"); }

    @PostMapping("/analises")
    @Operation(summary = "Consulta ocorrências na API C# e grava uma análise no PostgreSQL")
    public ResponseEntity<AnaliseResponse> gerar(@Valid @RequestBody FiltroRequest filtros) {
        var resultado = service.gerar(filtros);
        return ResponseEntity.created(URI.create("/api/analises/" + resultado.id())).body(resultado);
    }

    @GetMapping("/analises")
    @Operation(summary = "Consulta as 20 análises mais recentes já persistidas")
    public List<AnaliseResponse> listar() { return service.listar(); }

    @GetMapping("/analises/{id}")
    public AnaliseResponse buscar(@PathVariable UUID id) { return service.buscar(id); }
}
