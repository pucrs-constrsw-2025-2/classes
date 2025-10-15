# Classe Microservice API

Este projeto é uma API back-end desenvolvida em .NET 8, que funciona como um microserviço para gerenciar entidades de classe. A API utiliza MongoDB como banco de dados principal e implementa operações CRUD para a entidade Classe.

## Estrutura do Projeto

O projeto é organizado da seguinte forma:

- **src/ClasseMicroservice.API**: Contém a implementação da API.
  - **Controllers**: Gerencia as requisições HTTP.
    - `ClasseController.cs`: Controlador para operações relacionadas às classes.
  - **Models**: Define as entidades do sistema.
    - `Classe.cs`: Representa a entidade Classe.
  - **Data**: Interação com o banco de dados.
    - `ClasseRepository.cs`: Classe responsável pelas operações de acesso a dados.
  - `appsettings.json`: Configurações da aplicação, incluindo a string de conexão com o MongoDB.
  - `Program.cs`: Ponto de entrada da aplicação.

- **src/ClasseMicroservice.Domain**: Contém definições de domínio.
  - `ClasseSchema.json`: JSON Schema que define a estrutura e validações das classes.

- **ClasseMicroservice.sln**: Solução do projeto que agrupa os diferentes projetos e arquivos do sistema.

## Como Configurar e Executar

1. **Clone o repositório**:
   ```bash
   git clone <url-do-repositorio>
   cd ClasseMicroservice
   ```

2. **Instale as dependências**:
   ```bash
   dotnet restore
   ```

3. **Configure o MongoDB**:
   - Atualize a string de conexão no arquivo `src/ClasseMicroservice.API/appsettings.json` com as informações do seu banco de dados MongoDB.

4. **Execute a aplicação**:
   ```bash
   dotnet run --project src/ClasseMicroservice.API
   ```

5. **Acesse a API**:
   - A API estará disponível em `http://localhost:5000` (ou outra porta configurada).

## Contribuições

Contribuições são bem-vindas! Sinta-se à vontade para abrir issues ou pull requests.

## Licença

Este projeto está licenciado sob a MIT License. Veja o arquivo LICENSE para mais detalhes.