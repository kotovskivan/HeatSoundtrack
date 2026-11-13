HEAT SOUNDTRACK  v1.0
Suas próprias músicas no Need for Speed Heat — como faixas novas, junto das do jogo.
Ferramenta gratuita feita por fãs: Kitivski + Claude. SEM vínculo com a Electronic Arts ou a Ghost Games.

English — README.txt ao lado do programa.
Traduções feitas com IA (Claude).

=====================================================================
O QUE FAZ
  - Liga/desliga as faixas do jogo: garagem, cidade de dia, cidade à noite, corridas de dia, corridas à noite.
  - Adiciona suas músicas (mp3, flac, wav, ogg, m4a) como faixas NOVAS — você marca onde cada uma toca.
    As faixas do jogo continuam como estão.
  - Músicas do YouTube / YT Music: link de uma música, playlist ou álbum, vários links de uma vez ou uma lista .txt.
    Se o link for de um clipe, o programa pega a mesma música no YT Music — sem as introduções e esquetes do clipe.
  - Largada: na contagem toca a faixa de abertura do jogo, suas músicas vêm depois. Largada com música sua — na próxima versão.
  - Perfis: conjuntos diferentes de faixas, troque com um clique.
  - Gera UM mod para o Frosty Mod Manager. Os arquivos do jogo não mudam.
  - A janela fala 13 idiomas.

VOCÊ PRECISA DE
  - Windows 10 ou 11 (o Microsoft Edge já vem no sistema — a janela funciona com ele).
  - Need for Speed Heat (Steam ou EA app).
  - Frosty Mod Manager, versão do HarGabt para NFS Heat:
    https://github.com/HarGabt/FrostyToolsuite/releases
    Abra uma vez e escolha o jogo antes de usar esta ferramenta.
    Na primeira vez ele pede a chave de criptografia do jogo — NFS Heat Frosty Key (Nexus Mods).
    https://www.nexusmods.com/needforspeedheat/mods/74   (NFSHEAT.key -> Frosty Mod Manager\)
    No primeiro Launch (e depois que a pasta ModData for apagada) o Frosty pede permissão de administrador
    para criar links para os dados do jogo ("New installation detected") — é normal, aceite.

COMO USAR
  1. Descompacte o zip INTEIRO numa pasta comum (Área de Trabalho, Documentos, D:\Mods…),
     não em Program Files.
  2. Abra "Heat Soundtrack.exe".
  3. Escolha o idioma e aceite os termos. Na primeira vez ele lê o jogo — alguns minutos, só uma vez.
  4. + ADICIONAR MÚSICA → escolha um arquivo → marque onde toca → Salvar.
     Ou ▶ DO YT MUSIC → cole um link (música, playlist, álbum) → Baixar.
     Todas as músicas ficam na pasta "Music" ao lado do programa, cada uma numa pasta numerada:
     Music\1\, Music\2\… — a ordem na janela. + ADICIONAR MÚSICA cria a próxima pasta sozinho.
     À mão: uma nova pasta numerada com o arquivo da música + uma imagem (jpg, png) para a capa.
     Apagou a pasta — a música some do programa.
     Imagem: qualquer nome se for a única na pasta; várias — vale a cover.jpg. Qualquer tamanho.
  5. GERAR MOD. O mod aparece na lista do Frosty Mod Manager.
  6. No Frosty Mod Manager: marque "Heat Soundtrack", desative outros mods de música, clique em Launch.

SÓ OFFLINE
  Mods são para o modo single-player. Antes de entrar Online, abra o jogo sem mods.
  Jogar Online com mods pode fazer sua conta EA ser banida.

"O WINDOWS PROTEGEU O COMPUTADOR" / "EDITOR DESCONHECIDO"
  O programa não é assinado com um certificado pago, por isso o Windows SmartScreen avisa.
  Clique em "Mais informações" → "Executar assim mesmo". O código-fonte está na pasta "source".

O QUE TEM NA PASTA
  Heat Soundtrack.exe      o programa
  app\web                  a janela (páginas, textos em 13 idiomas)
  app\frosty               bibliotecas do Frosty Toolsuite — leem o jogo, gravam o mod (GPL v3)
  app\encoder              codificador de áudio da EA, do plugin Sound Editor do Frosty Editor
  app\yt-dlp.exe           baixa músicas do YouTube / YT Music (Unlicense)
  source\                  código-fonte deste programa (GPL v3)
  README\                  este texto em outros idiomas
  data\                    criada na primeira vez: suas configurações, músicas, imagens lidas do SEU jogo, log
  Music\                   suas músicas, cada uma numa pasta numerada: arquivo, capa, song.json (artista, título)
  THIRD-PARTY.txt          o que usamos, autores, licenças, links
  Nenhum arquivo do jogo ou da EA vem junto: os fundos são lidos do seu próprio jogo.

SE ALGO NÃO FUNCIONAR
  - O log fica em data\log.txt (botão "Log" na janela).
  - Depois de uma atualização do jogo: a ferramenta relê o jogo sozinha. Se gerar o mod falhar com
    "O jogo mudou" — procure uma versão mais nova da ferramenta.
  - Para começar do zero: feche o programa e apague a pasta "data".

REMOVER
  Desative o mod no Frosty Mod Manager e apague a pasta do programa.

---------------------------------------------------------------------
Need for Speed e NFS Heat são marcas comerciais da Electronic Arts Inc.
Esta ferramenta não foi feita, aprovada nem é apoiada pela Electronic Arts ou pela Ghost Games.
Créditos e licenças: THIRD-PARTY.txt e o botão "Créditos" no programa.
