# FoodStore

Este é um projeto de Aplicativo Universal do Windows (UWP) desenvolvido em C# para a Windows Store chamado **FoodStore**. Este documento irá guiá-lo através dos passos necessários para configurar e rodar a aplicação.

## Requisitos

Para rodar esta aplicação em modo de depuração (debug) sem problemas, é necessário que o Visual Studio esteja devidamente configurado com os componentes necessários.

### Componentes do Visual Studio

Certifique-se de que os seguintes componentes estão instalados no Visual Studio:

1. **Universal Windows Platform development**
   - Desenvolvimento de UWP
2. **.NET desktop development**
   - Desenvolvimento para desktop com .NET

### Pacotes NuGet

Além da configuração do Visual Studio, você também precisará baixar os seguintes pacotes NuGet:

1. `Microsoft.Data.Sqlite`
2. `Microsoft.Data.Sqlite.Core`

Para instalar esses pacotes, siga os passos abaixo:

1. Abra o **Gerenciador de Pacotes NuGet** no Visual Studio.
2. Clique em **Procurar** e pesquise por `Microsoft.Data.Sqlite` e `Microsoft.Data.Sqlite.Core`.
3. Instale os pacotes em seu projeto.

O banco de dados SQLite será baixado para o local do projeto.

## Executando a Aplicação

Após configurar o Visual Studio e instalar os pacotes NuGet, você estará pronto para rodar a aplicação. Siga os passos abaixo:

1. Abra a solução do projeto no Visual Studio.
2. Selecione a configuração de depuração **Debug**.
3. Pressione **F5** para iniciar a aplicação em modo de depuração.

## Conclusão

Com esses passos, você deve conseguir rodar a aplicação UWP **FoodStore** em modo de depuração sem problemas. Se encontrar qualquer dificuldade, verifique se todos os componentes e pacotes foram corretamente instalados e configurados.
