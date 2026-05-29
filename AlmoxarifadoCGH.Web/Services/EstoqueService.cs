using Almoxarifado.Domain.Entities;
using Almoxarifado.Domain.Enums;
using Almoxarifado.Domain.ValueObjects;
using Almoxarifado.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AlmoxarifadoCGH.Web.Services;

public class EstoqueService
{
    // Agora injetamos a FACTORY, a solução oficial para Blazor Server
    private readonly IDbContextFactory<AppDbContext> _factory;

    public EstoqueService(IDbContextFactory<AppDbContext> factory)
    {
        _factory = factory;
    }

    // =========================================================================
    // MÉTODOS DE DOMÍNIO: MOCHILAS (DDD)
    // =========================================================================

    public async Task EntregarMochilaAsync(int mochilaId, string turno, int agente1Id, int agente2Id)
    {
        using var _context = await _factory.CreateDbContextAsync();

        // 1. Validações Iniciais
        if (agente1Id == 0 || agente2Id == 0) throw new InvalidOperationException("Você deve selecionar dois agentes para formar a dupla.");
        if (agente1Id == agente2Id) throw new InvalidOperationException("Selecione dois agentes diferentes. O mesmo agente não pode fazer a dupla sozinho.");

        var mochila = await _context.Set<Mochila>()
            .Include(m => m.Equipamentos)
            .FirstOrDefaultAsync(m => m.Id == mochilaId);

        if (mochila == null) throw new InvalidOperationException("Mochila não encontrada no sistema.");

        // 2. Busca os agentes reais no banco de dados
        var agente1 = await _context.Agentes.FindAsync(agente1Id);
        var agente2 = await _context.Agentes.FindAsync(agente2Id);

        if (agente1 == null || agente2 == null) throw new InvalidOperationException("Um ou mais agentes selecionados não existem no banco de dados.");

        // 3. Monta o nome da dupla e aplica a regra do domínio
        string nomeDupla = $"{agente1.Nome} e {agente2.Nome}";
        var operacao = new OperacaoTurno(turno, nomeDupla);
        mochila.EntregarParaDupla(operacao);

        // 4. GERA O HISTÓRICO COM A CHAVE ESTRANGEIRA PREENCHIDA
        foreach (var eq in mochila.Equipamentos)
        {
            var mov = new Movimentacao
            {
                DataHora = DateTime.Now,
                Tipo = TipoMovimentacao.Saida,
                CatalogoId = eq.CatalogoId,
                EquipamentoId = eq.Id,
                Quantidade = 1,
                AgenteId = agente1.Id, // <-- AQUI A MAGIA ACONTECE! O banco de dados fica feliz.
                NomeAlmoxarife = $"Kit {mochila.Numero} - Dupla: {nomeDupla}"
            };
            _context.Movimentacoes.Add(mov);
        }

        await _context.SaveChangesAsync();
    }

    public async Task DevolverMochilaAsync(int mochilaId)
    {
        using var _context = await _factory.CreateDbContextAsync();

        var mochila = await _context.Set<Mochila>()
            .Include(m => m.Equipamentos)
            .FirstOrDefaultAsync(m => m.Id == mochilaId);

        if (mochila == null) throw new InvalidOperationException("Mochila não encontrada no sistema.");

        // Guarda o nome da dupla antes de limpar a operação da mochila
        var duplaQueDevolveu = mochila.OperacaoAtual?.Dupla ?? "Desconhecida";

        mochila.Devolver();

        // GERA O HISTÓRICO DE DEVOLUÇÃO PARA CADA EQUIPAMENTO
        foreach (var eq in mochila.Equipamentos)
        {
            var mov = new Movimentacao
            {
                DataHora = DateTime.Now,
                Tipo = TipoMovimentacao.Entrada,
                CatalogoId = eq.CatalogoId,
                EquipamentoId = eq.Id,
                Quantidade = 1,
                NomeAlmoxarife = $"Kit {mochila.Numero} (Devolvido por {duplaQueDevolveu})"
            };
            _context.Movimentacoes.Add(mov);
        }

        await _context.SaveChangesAsync();
    }

    public async Task SubstituirEquipamentoQuebradoAsync(int mochilaId, string serieQuebrado, int idNovoEquipamento)
    {
        using var _context = await _factory.CreateDbContextAsync();
        var mochila = await _context.Set<Mochila>()
            .Include(m => m.Equipamentos)
            .FirstOrDefaultAsync(m => m.Id == mochilaId);

        if (mochila == null) throw new InvalidOperationException("Mochila não encontrada.");

        var novoEquipamento = await _context.Equipamentos.FindAsync(idNovoEquipamento);
        if (novoEquipamento == null) throw new InvalidOperationException("Equipamento reserva não encontrado.");

        mochila.SubstituirEquipamentoQuebrado(serieQuebrado, novoEquipamento);
        await _context.SaveChangesAsync();
    }

    public async Task<List<Mochila>> ListarTodasMochilasAsync()
    {
        using var _context = await _factory.CreateDbContextAsync();
        return await _context.Set<Mochila>()
            .Include(m => m.Equipamentos)
            .AsNoTracking()
            .OrderBy(m => m.Numero)
            .ToListAsync();
    }

    public async Task<List<Mochila>> ListarMochilasPorStatusAsync(StatusMochila status)
    {
        using var _context = await _factory.CreateDbContextAsync();
        return await _context.Set<Mochila>()
            .Include(m => m.Equipamentos)
            .Where(m => m.Status == status)
            .AsNoTracking()
            .OrderBy(m => m.Numero)
            .ToListAsync();
    }

    // =========================================================================
    // MÉTODOS ORIGINAIS DE INFRAESTRUTURA
    // =========================================================================

    public async Task<List<Agente>> BuscarAgentesAsync(string termoBusca, CancellationToken token = default)
    {
        using var _context = await _factory.CreateDbContextAsync(token);
        if (string.IsNullOrWhiteSpace(termoBusca)) return new List<Agente>();
        return await _context.Agentes.Where(a => a.Ativo && a.Nome.Contains(termoBusca)).Take(10).ToListAsync(token);
    }

    public async Task<List<Catalogo>> BuscarItensCatalogoAsync(string termoBusca, CancellationToken token = default)
    {
        using var _context = await _factory.CreateDbContextAsync(token);
        if (string.IsNullOrWhiteSpace(termoBusca)) return new List<Catalogo>();
        return await _context.Catalogos.Where(c => c.Nome.Contains(termoBusca)).Take(10).ToListAsync(token);
    }

    public async Task<List<Equipamento>> BuscarEquipamentosDisponiveisAsync(int catalogoId)
    {
        using var _context = await _factory.CreateDbContextAsync();
        return await _context.Equipamentos.Where(e => e.CatalogoId == catalogoId && e.Status == StatusEquipamento.Disponivel).ToListAsync();
    }

    public async Task RegistrarSaidaAsync(int agenteId, int catalogoId, int? equipamentoId, int quantidade, string nomeAlmoxarife)
    {
        if (string.IsNullOrWhiteSpace(nomeAlmoxarife)) throw new ArgumentException("A identificação do operador (Almoxarife) é obrigatória para registrar a saída.");
        using var _context = await _factory.CreateDbContextAsync();

        var movimentacao = new Movimentacao
        {
            DataHora = DateTime.Now,
            Tipo = TipoMovimentacao.Saida,
            AgenteId = agenteId,
            CatalogoId = catalogoId,
            EquipamentoId = equipamentoId,
            Quantidade = quantidade,
            NomeAlmoxarife = nomeAlmoxarife
        };
        _context.Movimentacoes.Add(movimentacao);

        var catalogo = await _context.Catalogos.FindAsync(catalogoId);
        if (catalogo != null && catalogo.IsConsumivel) catalogo.EstoqueAtual -= quantidade;

        if (equipamentoId.HasValue)
        {
            var equipamento = await _context.Equipamentos.FindAsync(equipamentoId);
            if (equipamento != null) equipamento.Status = StatusEquipamento.Emprestado;
        }
        await _context.SaveChangesAsync();
    }

    public async Task<List<Agente>> ListarAgentesAsync()
    {
        using var _context = await _factory.CreateDbContextAsync();
        return await _context.Agentes.ToListAsync();
    }

    public async Task SalvarAgenteAsync(Agente agente)
    {
        using var _context = await _factory.CreateDbContextAsync();
        if (agente.Id == 0) _context.Agentes.Add(agente);
        else _context.Agentes.Update(agente);
        await _context.SaveChangesAsync();
    }

    public async Task<List<Catalogo>> ListarCatalogoAsync()
    {
        using var _context = await _factory.CreateDbContextAsync();
        return await _context.Catalogos.Include(c => c.Equipamentos).AsNoTracking().ToListAsync();
    }

    public async Task SalvarItemCatalogoAsync(Catalogo item)
    {
        using var _context = await _factory.CreateDbContextAsync();
        if (!item.IsConsumivel && !string.IsNullOrWhiteSpace(item.Descricao))
        {
            bool serieExiste = await _context.Equipamentos.AnyAsync(e => e.NumeroSerie.ToLower() == item.Descricao.ToLower().Trim());
            if (serieExiste) throw new InvalidOperationException($"O patrimônio '{item.Descricao}' já está cadastrado.");
        }

        item.Equipamentos = null!;

        if (item.Id == 0)
        {
            _context.Catalogos.Add(item);
            await _context.SaveChangesAsync();

            if (item.IsConsumivel && item.EstoqueAtual > 0)
            {
                _context.Movimentacoes.Add(new Movimentacao { DataHora = DateTime.Now, Tipo = TipoMovimentacao.Entrada, Quantidade = item.EstoqueAtual, CatalogoId = item.Id, NomeAlmoxarife = "Operador_CGH" });
            }

            if (!item.IsConsumivel && !string.IsNullOrWhiteSpace(item.Descricao))
            {
                _context.Equipamentos.Add(new Equipamento { CatalogoId = item.Id, NumeroSerie = item.Descricao, Status = StatusEquipamento.Disponivel });
            }
        }
        else
        {
            var itemAntigo = await _context.Catalogos.AsNoTracking().FirstOrDefaultAsync(c => c.Id == item.Id);
            _context.Update(item); // Com a factory, o update direto sempre funciona pois a memória é limpa!

            if (itemAntigo != null && item.IsConsumivel && item.EstoqueAtual != itemAntigo.EstoqueAtual)
            {
                int diferenca = item.EstoqueAtual - itemAntigo.EstoqueAtual;
                _context.Movimentacoes.Add(new Movimentacao { DataHora = DateTime.Now, Tipo = diferenca > 0 ? TipoMovimentacao.Entrada : TipoMovimentacao.Saida, Quantidade = Math.Abs(diferenca), CatalogoId = item.Id, NomeAlmoxarife = "Operador_CGH" });
            }

            if (!item.IsConsumivel && !string.IsNullOrWhiteSpace(item.Descricao))
            {
                bool serieExiste = await _context.Equipamentos.AnyAsync(e => e.NumeroSerie == item.Descricao);
                if (!serieExiste)
                {
                    _context.Equipamentos.Add(new Equipamento { CatalogoId = item.Id, NumeroSerie = item.Descricao, Status = StatusEquipamento.Disponivel });
                    if (item.EstoqueAtual == 0) item.EstoqueAtual = 1;
                }
            }
        }
        await _context.SaveChangesAsync();
    }

    public async Task<List<Movimentacao>> ListarHistoricoAsync()
    {
        using var _context = await _factory.CreateDbContextAsync();
        return await _context.Movimentacoes.Include(m => m.Agente).Include(m => m.Catalogo).Include(m => m.Equipamento).OrderByDescending(m => m.DataHora).ToListAsync();
    }

    public async Task<Catalogo?> BuscarItemPorNomeAsync(string nome)
    {
        using var _context = await _factory.CreateDbContextAsync();
        if (string.IsNullOrWhiteSpace(nome)) return null;
        return await _context.Catalogos.FirstOrDefaultAsync(c => c.Nome.ToLower().Trim() == nome.ToLower().Trim());
    }

    public async Task<bool> ExcluirItemCatalogoAsync(int id)
    {
        using var _context = await _factory.CreateDbContextAsync();
        try
        {
            var item = await _context.Catalogos.FindAsync(id);
            if (item != null) { _context.Catalogos.Remove(item); await _context.SaveChangesAsync(); return true; }
            return false;
        }
        catch (DbUpdateException) { return false; }
    }

    public async Task<Movimentacao> RegistrarDevolucaoAsync(string numeroSerie, string nomeAlmoxarife)
    {
        if (string.IsNullOrWhiteSpace(nomeAlmoxarife)) throw new ArgumentException("A identificação do operador é obrigatória.");
        using var _context = await _factory.CreateDbContextAsync();

        var equipamento = await _context.Equipamentos.Include(e => e.Catalogo).FirstOrDefaultAsync(e => e.NumeroSerie == numeroSerie);
        if (equipamento == null) throw new InvalidOperationException("Equipamento não encontrado no sistema.");
        if (equipamento.Status != StatusEquipamento.Emprestado) throw new InvalidOperationException($"O equipamento {numeroSerie} não consta como emprestado.");

        var ultimaSaida = await _context.Movimentacoes.Include(m => m.Agente).Where(m => m.EquipamentoId == equipamento.Id && m.Tipo == TipoMovimentacao.Saida).OrderByDescending(m => m.DataHora).FirstOrDefaultAsync();
        if (ultimaSaida == null) throw new InvalidOperationException("Histórico de saída não encontrado para este equipamento.");

        equipamento.Status = StatusEquipamento.Disponivel;

        var novaEntrada = new Movimentacao { DataHora = DateTime.Now, Tipo = TipoMovimentacao.Entrada, Quantidade = 1, NomeAlmoxarife = nomeAlmoxarife, AgenteId = ultimaSaida.AgenteId, CatalogoId = equipamento.CatalogoId, EquipamentoId = equipamento.Id };
        _context.Movimentacoes.Add(novaEntrada);
        await _context.SaveChangesAsync();

        return novaEntrada;
    }

    public async Task<Catalogo?> BuscarCatalogoPorIdAsync(int id)
    {
        using var _context = await _factory.CreateDbContextAsync();
        return await _context.Catalogos.FindAsync(id);
    }

    public async Task<List<Equipamento>> ListarEquipamentosPorCatalogoAsync(int catalogoId)
    {
        using var _context = await _factory.CreateDbContextAsync();
        return await _context.Equipamentos.Where(e => e.CatalogoId == catalogoId).OrderBy(e => e.NumeroSerie).ToListAsync();
    }

    public async Task AdicionarEquipamentoAsync(Equipamento equipamento)
    {
        using var _context = await _factory.CreateDbContextAsync();
        bool serieExiste = await _context.Equipamentos.AnyAsync(e => e.NumeroSerie == equipamento.NumeroSerie);
        if (serieExiste) throw new InvalidOperationException($"O patrimônio {equipamento.NumeroSerie} já está cadastrado.");

        _context.Equipamentos.Add(equipamento);
        var catalogo = await _context.Catalogos.FindAsync(equipamento.CatalogoId);
        if (catalogo != null) { catalogo.EstoqueAtual++; _context.Catalogos.Update(catalogo); }
        await _context.SaveChangesAsync();
    }

    public async Task AtualizarStatusEquipamentoAsync(int equipamentoId, StatusEquipamento novoStatus)
    {
        using var _context = await _factory.CreateDbContextAsync();
        var equipamento = await _context.Equipamentos.FindAsync(equipamentoId);
        if (equipamento != null) { equipamento.Status = novoStatus; await _context.SaveChangesAsync(); }
    }

    public async Task<int> ContarItensEstoqueBaixoAsync()
    {
        using var _context = await _factory.CreateDbContextAsync();
        return await _context.Catalogos.Where(c => c.IsConsumivel && c.EstoqueAtual <= c.EstoqueMinimo).CountAsync();
    }

    public async Task<int> ContarEquipamentosAtrasadosAsync()
    {
        using var _context = await _factory.CreateDbContextAsync();
        var limiteTempo = DateTime.Now.AddHours(-12);
        return await _context.Equipamentos.Where(e => e.Status == StatusEquipamento.Emprestado).Where(e => _context.Movimentacoes.Where(m => m.EquipamentoId == e.Id && m.Tipo == TipoMovimentacao.Saida).OrderByDescending(m => m.DataHora).Select(m => m.DataHora).FirstOrDefault() < limiteTempo).CountAsync();
    }

    public async Task<int> ContarSaidasDoTurnoAsync()
    {
        using var _context = await _factory.CreateDbContextAsync();
        var inicioTurno = DateTime.Now.AddHours(-12);
        return await _context.Movimentacoes.Where(m => m.Tipo == TipoMovimentacao.Saida && m.DataHora >= inicioTurno).CountAsync();
    }

    public async Task<List<Catalogo>> ListarItensEstoqueBaixoAsync()
    {
        using var _context = await _factory.CreateDbContextAsync();
        return await _context.Catalogos.Where(c => c.IsConsumivel && c.EstoqueAtual <= c.EstoqueMinimo).OrderBy(c => c.Nome).ToListAsync();
    }

    public async Task<List<Movimentacao>> ListarEquipamentosAtrasadosAsync()
    {
        using var _context = await _factory.CreateDbContextAsync();
        var limiteTempo = DateTime.Now.AddHours(-12);
        return await _context.Movimentacoes.Include(m => m.Agente).Include(m => m.Catalogo).Include(m => m.Equipamento).Where(m => m.Tipo == TipoMovimentacao.Saida && m.Equipamento.Status == StatusEquipamento.Emprestado && m.DataHora < limiteTempo).OrderBy(m => m.DataHora).ToListAsync();
    }

    public async Task<List<Movimentacao>> ListarSaidasDoTurnoAsync()
    {
        using var _context = await _factory.CreateDbContextAsync();
        var inicioTurno = DateTime.Now.AddHours(-12);
        return await _context.Movimentacoes.Include(m => m.Agente).Include(m => m.Catalogo).Include(m => m.Equipamento).Where(m => m.Tipo == TipoMovimentacao.Saida && m.DataHora >= inicioTurno).OrderByDescending(m => m.DataHora).ToListAsync();
    }
    public async Task MontarKitNaMochilaAsync(int mochilaId, List<string> numerosSerie)
    {
        using var _context = await _factory.CreateDbContextAsync();

        var mochila = await _context.Set<Mochila>()
            .Include(m => m.Equipamentos)
            .FirstOrDefaultAsync(m => m.Id == mochilaId);

        if (mochila == null) throw new InvalidOperationException("Mochila não encontrada.");

        // Limpa espaços e remove duplicidades caso o operador tenha bipado o mesmo item duas vezes
        var seriaisLimpos = numerosSerie.Select(s => s.Trim()).Distinct().ToList();

        if (seriaisLimpos.Count != 4)
            throw new InvalidOperationException("Você deve informar exatamente 4 patrimônios distintos.");

        // Busca os equipamentos físicos no banco
        var equipamentos = await _context.Equipamentos
            .Where(e => seriaisLimpos.Contains(e.NumeroSerie))
            .ToListAsync();

        if (equipamentos.Count != 4)
        {
            var encontrados = equipamentos.Select(e => e.NumeroSerie).ToList();
            var naoEncontrados = seriaisLimpos.Except(encontrados);
            throw new InvalidOperationException($"Patrimônios não encontrados no sistema: {string.Join(", ", naoEncontrados)}");
        }

        // Verifica se os itens estão disponíveis na prateleira ou se já pertencem a esta própria mochila
        foreach (var eq in equipamentos)
        {
            if (eq.Status != StatusEquipamento.Disponivel && !mochila.Equipamentos.Any(me => me.Id == eq.Id))
                throw new InvalidOperationException($"O equipamento {eq.NumeroSerie} não está disponível (Status atual: {eq.Status}).");
        }

        // DELEGA PARA O DOMÍNIO: A Raiz de Agregação executa a regra de negócio
        mochila.MontarKit(equipamentos);

        await _context.SaveChangesAsync();
    }

    public async Task GarantirMochilasIniciaisAsync()
    {
        using var _context = await _factory.CreateDbContextAsync();

        var jaExiste = await _context.Set<Mochila>().AnyAsync();

        if (!jaExiste)
        {
            for (int i = 1; i <= 12; i++)
            {
                // Instanciando via Construtor DDD correto
                var mochila = new Mochila(i);
                _context.Set<Mochila>().Add(mochila);
            }
            await _context.SaveChangesAsync();
        }
    }

    public async Task AdicionarMochilaAsync(int numero)
    {
        using var _context = await _factory.CreateDbContextAsync();

        var existe = await _context.Set<Mochila>().AnyAsync(m => m.Numero == numero);
        if (existe) throw new InvalidOperationException($"A Mochila número {numero} já está cadastrada.");

        // Instanciando via Construtor DDD correto
        var novaMochila = new Mochila(numero);

        _context.Set<Mochila>().Add(novaMochila);
        await _context.SaveChangesAsync();
    }

    public async Task AtualizarNumeroSerieEquipamentoAsync(int equipamentoId, string novoSerial)
    {
        using var _context = await _factory.CreateDbContextAsync();

        // Verifica se o operador não digitou um serial que já existe em outro rádio/tablet
        var serialExiste = await _context.Equipamentos
            .AnyAsync(e => e.NumeroSerie.ToLower() == novoSerial.ToLower().Trim() && e.Id != equipamentoId);

        if (serialExiste) throw new InvalidOperationException($"O patrimônio '{novoSerial}' já pertence a outro equipamento.");

        var equipamento = await _context.Equipamentos.FindAsync(equipamentoId);
        if (equipamento == null) throw new InvalidOperationException("Equipamento não encontrado.");

        equipamento.NumeroSerie = novoSerial.Trim();
        await _context.SaveChangesAsync();
    }

    public async Task ExcluirEquipamentoBlindadoAsync(int equipamentoId)
    {
        using var _context = await _factory.CreateDbContextAsync();

        // A REGRA DE OURO: Verifica se já tem alguma movimentação no histórico
        var temHistorico = await _context.Movimentacoes.AnyAsync(m => m.EquipamentoId == equipamentoId);

        if (temHistorico)
            throw new InvalidOperationException("Acesso Negado: Este equipamento já possui histórico de operações na pista. Para tirá-lo de circulação, altere o status dele para 'Manutenção' ou 'Extraviado'.");

        var equipamento = await _context.Equipamentos.FindAsync(equipamentoId);
        if (equipamento != null)
        {
            // Se passou na regra, exclui fisicamente e diminui o número total do catálogo
            _context.Equipamentos.Remove(equipamento);

            var catalogo = await _context.Catalogos.FindAsync(equipamento.CatalogoId);
            if (catalogo != null && catalogo.EstoqueAtual > 0) catalogo.EstoqueAtual--;

            await _context.SaveChangesAsync();
        }

    }
    public async Task<List<Equipamento>> ListarEquipamentosDisponiveisPorTipoAsync(string tipo)
    {
        using var _context = await _factory.CreateDbContextAsync();

        // Busca os equipamentos que estão disponíveis E cujo nome no catálogo contenha a palavra-chave
        return await _context.Equipamentos
            .Include(e => e.Catalogo)
            .Where(e => e.Status == StatusEquipamento.Disponivel &&
                        e.Catalogo.Nome.ToLower().Contains(tipo.ToLower()))
            .OrderBy(e => e.NumeroSerie)
            .ToListAsync();
    }
}