# Projeto teste de API REST em C#

## Como rodar e testar

No terminal (na pasta PortfolioApi):
```
dotnet run
```

Acessar a URL exibida no console. Abra (http://localhost:5001)/swagger para testar visualmente os endpoints.

Os endpoints esperados (conforme README) foram implementados:
- GET /api/assets, 
- GET /api/assets/{id}, 
- GET /api/assets/search?symbol=, 
- POST /api/assets, 
- PUT /api/assets/{id}/price; 
- GET /api/portfolios, 
- POST /api/portfolios, 
- GET /api/portfolios/{id}, 
- POST /api/portfolios/{id}/positions, 
- PUT /api/portfolios/{id}/positions/{positionId}, 
- DELETE /api/portfolios/{id}/positions/{positionId}; 

Analytics: 
- /api/analytics/portfolios/{id}/performance, 
- /api/analytics/portfolios/{id}/risk-analysis, 
- /api/analytics/portfolios/{id}/rebalancing.
 
## Organizaçao dos arquivos

- Arquitetura em camadas SOLID

- **Controllers**: expõem endpoints REST — camada de apresentação (MVC).

- **Services**: encapsulam business logic (cálculo de performance, rebalanceamento, regras de transação) — separação de responsabilidades (SRP).

- **Repositories**: abstraem acesso a dados (Entity Framework). Isso permite trocar o provedor (InMemory → SQL Server) sem afetar a lógica de negócio (Dependency Inversion).

- **Data/DbContext**: EF Core para mapeamento objeto-relacional e SeedData para popular a base com SeedData.json.


## Cálculos e decisões técnicas

- **Retorno total**: (valorAtual - investido) / investido.

- **Retorno anualizado**: usa a fórmula de capitalização: (1 + totalReturn)^(1/years) - 1. Tratado casos com poucos dados evitando divisão por zero.

- **Volatilidade**: calculado std dev dos retornos do histórico de preço e anualizado por sqrt(252) (assumindo ~252 dias úteis; é uma aproximação padrão). Para portfólio, utilizado uma aproximação sqrt(sum(w_i^2 * sigma_i^2)) — isto evita calcular a matriz de covariância completa e é aceitável como primeira aproximação para o teste.

- **Sharpe ratio**: (annualReturn - selic) / volatility. A selic é parâmetro do endpoint (padrão 6.5% no código).

- **Rebalanceamento**: Recebe um dicionário { "PETR4": 30, "VALE3": 20, ... } (valores em porcentagem). Calcula diferença entre valor desejado e atual. Aplica taxa de transação 0.3% e ignora transações cujo valor estimado é < R$100 (requisito do README). Tenta minimizar transações sugerindo apenas operações acima do limiar.