package br.com.bairroemdia.domain;

import br.com.bairroemdia.dto.Contratos.*;
import jakarta.persistence.*;
import java.time.OffsetDateTime;
import java.time.ZoneOffset;
import java.util.EnumMap;
import java.util.List;
import java.util.Map;
import java.util.UUID;

@Entity
@Table(name = "analises_ocorrencias")
public class Analise {
    @Id private UUID id;
    @Column(name = "gerada_em", nullable = false) private OffsetDateTime geradaEm;
    @Column(length = 2) private String uf;
    @Column(length = 100) private String cidade;
    @Column(length = 100) private String bairro;
    @Column(nullable = false) private int total;
    @ElementCollection(fetch = FetchType.EAGER)
    @CollectionTable(name = "analise_status", joinColumns = @JoinColumn(name = "analise_id"))
    @MapKeyColumn(name = "status") @MapKeyEnumerated(EnumType.STRING)
    @Column(name = "quantidade", nullable = false)
    private Map<Situacao, Integer> porStatus = new EnumMap<>(Situacao.class);
    @ElementCollection(fetch = FetchType.EAGER)
    @CollectionTable(name = "analise_categoria", joinColumns = @JoinColumn(name = "analise_id"))
    @MapKeyColumn(name = "categoria") @MapKeyEnumerated(EnumType.STRING)
    @Column(name = "quantidade", nullable = false)
    private Map<Categoria, Integer> porCategoria = new EnumMap<>(Categoria.class);

    protected Analise() {}

    public Analise(FiltroRequest filtros, List<OcorrenciaCSharp> ocorrencias) {
        id = UUID.randomUUID();
        geradaEm = OffsetDateTime.now(ZoneOffset.UTC);
        uf = filtros.uf(); cidade = filtros.cidade(); bairro = filtros.bairro();
        for (var status : Situacao.values()) porStatus.put(status, 0);
        for (var categoria : Categoria.values()) porCategoria.put(categoria, 0);
        var ids = new java.util.HashSet<UUID>();
        for (var o : ocorrencias) {
            if (o == null || o.id() == null || o.status() == null || o.categoria() == null || !ids.add(o.id()))
                throw new IllegalArgumentException("Contrato de ocorrências inválido.");
            porStatus.merge(o.status(), 1, Integer::sum);
            porCategoria.merge(o.categoria(), 1, Integer::sum);
        }
        total = ocorrencias.size();
    }

    public UUID getId() { return id; }
    public AnaliseResponse resposta() {
        int encerradas = porStatus.getOrDefault(Situacao.Encerrada, 0);
        return new AnaliseResponse(id, geradaEm, "CSharp", new FiltroRequest(uf, cidade, bairro),
            total, total - encerradas, encerradas, Map.copyOf(porStatus), Map.copyOf(porCategoria));
    }
}
