CREATE TABLE analises_ocorrencias (
    id UUID PRIMARY KEY,
    gerada_em TIMESTAMP WITH TIME ZONE NOT NULL,
    uf VARCHAR(2),
    cidade VARCHAR(100),
    bairro VARCHAR(100),
    total INTEGER NOT NULL CHECK (total >= 0)
);
CREATE INDEX ix_analises_gerada_em ON analises_ocorrencias (gerada_em DESC);

CREATE TABLE analise_status (
    analise_id UUID NOT NULL REFERENCES analises_ocorrencias(id) ON DELETE CASCADE,
    status VARCHAR(255) NOT NULL,
    quantidade INTEGER NOT NULL CHECK (quantidade >= 0),
    PRIMARY KEY (analise_id, status)
);
CREATE TABLE analise_categoria (
    analise_id UUID NOT NULL REFERENCES analises_ocorrencias(id) ON DELETE CASCADE,
    categoria VARCHAR(255) NOT NULL,
    quantidade INTEGER NOT NULL CHECK (quantidade >= 0),
    PRIMARY KEY (analise_id, categoria)
);
