# Testes

Como executar:

```bash
dotnet test ../ClasseMicroservice.sln -v minimal
```

Para cobertura (exemplo, via coverlet integrado):

```bash
dotnet test ./ClasseMicroservice.Tests/ClasseMicroservice.Tests.csproj \
  /p:CollectCoverage=true \
  /p:CoverletOutput=./TestResults/coverage/ \
  /p:MergeWith=./TestResults/coverage/coverage.json \
  /p:CoverletOutputFormat="json,lcov,opencover"
```

Os relatórios serão gerados em `tests/ClasseMicroservice.Tests/TestResults/coverage/`.
