#  Controle de Almoxarifado CGH 

![.NET](https://img.shields.io/badge/.NET%2010-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white)
![Blazor](https://img.shields.io/badge/Blazor-5C2D91?style=for-the-badge&logo=blazor&logoColor=white)
![MudBlazor](https://img.shields.io/badge/MudBlazor-594AE2?style=for-the-badge&logo=blazor&logoColor=white)
![EF Core](https://img.shields.io/badge/EF%20Core-336791?style=for-the-badge&logo=microsoft-sql-server&logoColor=white)

Sistema corporativo de ponta a ponta desenvolvido para a gestão inteligente de ativos operacionais, montagem de kits e controle de fluxo de estoque (check-in/check-out) de mochilas e equipamentos.

##  Objetivo do Projeto
Este projeto foi construído para resolver um problema real de logística operacional: garantir que equipes recebam seus equipamentos (tablets, rádios, etc.) organizados em kits (Mochilas) de forma rastreável, rápida e segura. 

Além de resolver uma dor de negócio, este repositório serve como uma **vitrine técnica** da minha capacidade de construir aplicações escaláveis utilizando as melhores práticas da plataforma .NET.

##  Arquitetura e Padrões (Domain-Driven Design)
A solução foi desenhada utilizando **Domain-Driven Design (DDD)** para garantir a separação de responsabilidades, testabilidade e evolução contínua do software. A solução está dividida em três camadas principais:

* **`Almoxarifado.Domain`**: O coração da aplicação. Contém as Regras de Negócio, Entidades Ricas (ex: `Mochila`, `Equipamento`), Enums e *Value Objects* (`OperacaoTurno`). Nenhuma dependência externa entra aqui.
* **`Almoxarifado.Infrastructure`**: Responsável pela persistência de dados. Implementa o `AppDbContext` do Entity Framework Core, mapeamento de tabelas e as *Migrations*.
* **`AlmoxarifadoCGH.Web`**: Camada de apresentação reativa e em tempo real utilizando **Blazor Server**. A interface foi desenhada com componentes **MudBlazor** para um aspecto moderno (Material Design) e responsivo.

##  Principais Funcionalidades
* **Gestão de Kits (Mochilas)**: Criação de mochilas e vinculação física de equipamentos (através de números de série) garantindo a integridade do kit.
* **Rastreamento de Operações**: Histórico de saídas e entradas, vinculando o kit a uma dupla operacional e turno de trabalho.
* **Dashboard em Tempo Real**: Painel com indicadores de equipamentos atrasados, estoque crítico e métricas de saída do turno.
* **Gestão de Catálogo e Patrimônio**: Controle de itens consumíveis (estoque) e itens de patrimônio único (seriais).

##  Destaques Técnicos
* **C# 14 & .NET 10**: Utilização das versões mais recentes do ecossistema Microsoft.
* **Entity Framework Core**: Mapeamento Objeto-Relacional avançado, utilizando `IDbContextFactory` para lidar com a concorrência assíncrona do Blazor Server.
* **Componentização Blazor**: Criação de modais dinâmicos, tabelas assíncronas e formulários validados sem escrever uma única linha de JavaScript.

##  Como executar o projeto localmente

**Pré-requisitos:**
* [.NET 10 SDK](https://dotnet.microsoft.com/download)
* Visual Studio 2022+ ou VS Code
* SQL Server (LocalDB ou instância Docker)

**Passo a passo:**
1. Clone o repositório:
   ```bash
   git clone [https://github.com/LuizhBrandao/ControleAlmoxarifadoCGH.git](https://github.com/LuizhBrandao/ControleAlmoxarifadoCGH.git)
   ```

2. Navegue até a pasta do projeto de Infraestrutura e aplique as migrações para criar o banco de dados:

````Bash
cd ControleAlmoxarifadoCGH/Almoxarifado.Infrastructure
dotnet ef database update --startup-project ../AlmoxarifadoCGH.Web
````
3. Execute o projeto Web:

````Bash
cd ../AlmoxarifadoCGH.Web
dotnet run
````
4. Acesse no navegador através de https://localhost:7029 (ou a porta indicada no terminal).

 Autor
Luiz Henrique Oliveira Brandão
Desenvolvedor de Software Júnior focado em criar soluções robustas e agregar valor através da tecnologia.
