using BairroEmDia.Domain.Enums;
using BairroEmDia.Domain.Exceptions;

namespace BairroEmDia.Domain.Entities;

public class Ocorrencia
{
    private readonly List<HistoricoAtualizacao> _historico = new();
    private readonly List<Confirmacao> _confirmacoes = new();

    public Guid Id { get; private set; }
    public Guid UsuarioId { get; private set; }

    public string Titulo { get; private set; } = string.Empty;
    public string Descricao { get; private set; } = string.Empty;
    public string Cidade { get; private set; } = string.Empty;
    public string Uf { get; private set; } = string.Empty;
    public string Bairro { get; private set; } = string.Empty;
    public string Localizacao { get; private set; } = string.Empty;

    public CategoriaOcorrencia Categoria { get; private set; }
    public StatusOcorrencia Status { get; private set; }

    public DateTime CriadoEm { get; private set; }
    public DateTime AtualizadoEm { get; private set; }

    public IReadOnlyCollection<HistoricoAtualizacao> Historico
        => _historico.AsReadOnly();

    public IReadOnlyCollection<Confirmacao> Confirmacoes
        => _confirmacoes.AsReadOnly();

    private Ocorrencia()
    {
    }

    public Ocorrencia(
        Guid usuarioId,
        string titulo,
        string descricao,
        string bairro,
        string localizacao,
        CategoriaOcorrencia categoria,
        string cidade,
        string uf)
    {
        if (usuarioId == Guid.Empty)
            throw new RegraDeNegocioException(
                "O usuário responsável é obrigatório.");

        ValidarDados(titulo, descricao, bairro, localizacao, categoria, cidade, uf);

        Id = Guid.NewGuid();
        UsuarioId = usuarioId;
        Titulo = titulo.Trim();
        Descricao = descricao.Trim();
        Cidade = cidade.Trim();
        Uf = uf.Trim().ToUpperInvariant();
        Bairro = bairro.Trim();
        Localizacao = localizacao.Trim();
        Categoria = categoria;
        Status = StatusOcorrencia.Registrada;
        CriadoEm = DateTime.UtcNow;
        AtualizadoEm = CriadoEm;
    }

    public void Editar(
        string titulo,
        string descricao,
        string bairro,
        string localizacao,
        CategoriaOcorrencia categoria,
        string cidade,
        string uf)
    {
        if (Status == StatusOcorrencia.Encerrada)
            throw new RegraDeNegocioException(
                "Uma ocorrência encerrada não pode ser editada.");

        ValidarDados(titulo, descricao, bairro, localizacao, categoria, cidade, uf);

        Titulo = titulo.Trim();
        Descricao = descricao.Trim();
        Cidade = cidade.Trim();
        Uf = uf.Trim().ToUpperInvariant();
        Bairro = bairro.Trim();
        Localizacao = localizacao.Trim();
        Categoria = categoria;
        AtualizadoEm = DateTime.UtcNow;
    }

    public void AlterarStatus(
        StatusOcorrencia novoStatus,
        string justificativa)
    {
        if (!Enum.IsDefined(typeof(StatusOcorrencia), novoStatus))
            throw new RegraDeNegocioException(
                "O status informado é inválido.");

        if (novoStatus == Status)
            throw new RegraDeNegocioException(
                "A ocorrência já está nesse status.");

        // Uma ocorrência encerrada deve voltar para análise
        // antes de seguir para qualquer outra situação.
        if (Status == StatusOcorrencia.Encerrada &&
            novoStatus != StatusOcorrencia.EmAnalise)
        {
            throw new RegraDeNegocioException(
                "Para reabrir uma ocorrência, altere o status para EmAnalise.");
        }

        // Valida a justificativa antes de modificar a ocorrência.
        var atualizacao = new HistoricoAtualizacao(
            Id,
            Status,
            novoStatus,
            justificativa);

        _historico.Add(atualizacao);
        Status = novoStatus;
        AtualizadoEm = atualizacao.CriadoEm;
    }

    public void Confirmar(Guid usuarioId)
    {
        if (usuarioId == Guid.Empty)
            throw new RegraDeNegocioException(
                "O usuário é obrigatório para confirmar uma ocorrência.");

        if (usuarioId == UsuarioId)
            throw new RegraDeNegocioException(
                "Você não pode confirmar a própria ocorrência.");

        if (_confirmacoes.Any(c => c.UsuarioId == usuarioId))
            throw new RegraDeNegocioException(
                "Você já confirmou esta ocorrência.");

        _confirmacoes.Add(new Confirmacao(Id, usuarioId));
    }

    private static void ValidarDados(
        string titulo,
        string descricao,
        string bairro,
        string localizacao,
        CategoriaOcorrencia categoria,
        string cidade,
        string uf)
    {
        ValidarTexto(cidade, "A cidade", 100);
        Localidade.ValidarUf(uf);
        ValidarTexto(titulo, "O título", 120);
        ValidarTexto(descricao, "A descrição", 2000);
        ValidarTexto(bairro, "O bairro", 100);
        ValidarTexto(localizacao, "A localização", 250);

        if (!Enum.IsDefined(typeof(CategoriaOcorrencia), categoria))
            throw new RegraDeNegocioException(
                "A categoria informada é inválida.");
    }

    private static void ValidarTexto(
        string valor,
        string campo,
        int tamanhoMaximo)
    {
        if (string.IsNullOrWhiteSpace(valor))
            throw new RegraDeNegocioException(
                $"{campo} deve ser preenchido(a).");

        if (valor.Trim().Length > tamanhoMaximo)
            throw new RegraDeNegocioException(
                $"{campo} deve ter no máximo {tamanhoMaximo} caracteres.");
    }
}