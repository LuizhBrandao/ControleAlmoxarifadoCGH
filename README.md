#  Controle de Almoxarifado CGH 

Tech Stack & Arquitetura

![.NET 10](https://img.shields.io/badge/.NET%2010-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white)
![Blazor](https://img.shields.io/badge/Blazor-5C2D91?style=for-the-badge&logo=blazor&logoColor=white)
![MudBlazor](https://img.shields.io/badge/MudBlazor-594AE2?style=for-the-badge&logo=blazor&logoColor=white)
![SQLite](https://img.shields.io/badge/SQLite-07405E?style=for-the-badge&logo=sqlite&logoColor=white)
![EF Core](https://img.shields.io/badge/EF%20Core-336791?style=for-the-badge&logo=microsoft-sql-server&logoColor=white)
![DDD](https://img.shields.io/badge/DDD-Architecture-007ACC?style=for-the-badge)
![Git](https://img.shields.io/badge/Git-F05032?style=for-the-badge&logo=git&logoColor=white)

Sistema inteligente de gestão de inventário e rastreabilidade logística desenvolvido especificamente para a operação do aeroporto de Congonhas. Este sistema visa eliminar a dependência de planilhas manuais, garantindo auditoria, controle de estoque em tempo real e facilidade operacional para a equipe.

## O Problema que Resolvemos
Atualmente, o controle via planilhas manuais gera:

Erros de digitação e inconsistência: Dificuldade em rastrear quem está com qual equipamento.

Falta de Auditoria: Impossibilidade de verificar o histórico detalhado de movimentações em casos de avarias ou extravios.

Complexidade Tecnológica: Sistemas corporativos que exigem infraestrutura de TI pesada e instalação complexa.

## Funcionalidades Principais
1. Gestão de Kits (Mochilas)
Montagem de Kits: Sistema inteligente que exige a seleção exata de 4 equipamentos (Tablet, DWS e Rádios), evitando erros na formação das mochilas.

Check-out e Check-in: Fluxo simplificado para entrega e devolução dos kits, associando duplas operacionais aos equipamentos.

2. Controle de Estoque com Validação
Catálogo Inteligente: Cadastro de consumíveis (ex: bobinas) e equipamentos (ex: rádios), com validação rigorosa para evitar dados duplicados ou inconsistentes.

Alertas Críticos: Dashboards que indicam instantaneamente itens com estoque abaixo do mínimo exigido.

3. Rastreabilidade e Auditoria (Compliance)
Histórico Completo: Cada movimentação (saída/entrada) registra quem foi o agente, qual o horário, qual o kit e quais os números de série dos equipamentos.

Bloqueio de Exclusão: Patrimônios que já possuíram histórico de circulação não podem ser apagados, garantindo a integridade dos dados para auditorias.

4. Segurança e Integridade
Validações Rigorosas: O sistema utiliza Data Annotations (Regex) para bloquear caracteres inválidos em nomes, números de série e matrículas de agentes.

Campos Protegidos: Identificadores únicos (Matrículas e Patrimônios) são bloqueados para edição após o cadastro, prevenindo fraudes ou alterações acidentais.

## Detalhes Técnicos para TI e Operação
O sistema foi arquitetado para ser um produto "Plug and Play":

Arquitetura: ASP.NET Core Blazor (Server-Side) + MudBlazor (Interface moderna e responsiva).

Portabilidade: Utiliza o banco de dados SQLite, um ficheiro único que não requer instalação de servidores complexos (como SQL Server) ou configuração de rede.

Segurança: Baseado em princípios DDD (Domain-Driven Design), com validações que ocorrem tanto no ecrã (UI) quanto na base de dados (Infraestrutura).

Configuração: Zero infraestrutura. O sistema cria automaticamente a sua própria base de dados no primeiro acesso.

## Como Iniciar (Para Operadores)
Executar: Basta dar dois cliques no ficheiro executável do sistema.

Primeiro Acesso: Na primeira vez que abrir, o sistema configurará automaticamente todos os ficheiros necessários.

Interface: Navegue pelo menu lateral para acessar o Balcão, consultar o Histórico ou gerir o Catálogo.

Segurança: O sistema impede automaticamente a gravação de dados incompletos ou inválidos, guiando o utilizador em cada etapa.

## Compromisso de Integridade
Este software foi desenvolvido com a premissa de que a operação aeroportuária não admite erros. Cada equipamento está vinculado a uma dupla de agentes e cada saída/entrada é registrada permanentemente.

## Autor
Luiz Henrique Oliveira Brandão
Desenvolvedor de Software Júnior focado em criar soluções robustas e agregar valor através da tecnologia.
