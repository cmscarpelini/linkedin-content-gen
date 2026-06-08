# Functional Spec – MVP v0.3

## 1. Objetivo do MVP
Permitir que um desenvolvedor gere conteúdo técnico curto e relevante baseado em artigos do ecossistema Microsoft, exibidos como um bloco único para revisão manual antes da publicação.

---

## 2. Funcionalidades Incluídas

### 2.1 Coleta manual de artigos
- Busca em fontes RSS configuráveis via `appsettings.json` (array `RssSources`).
- Limite configurável via `RssMaxArticles` (padrão: 5 artigos por chamada).
- Fluxo totalmente síncrono realizado pelo usuário.
- Artigos já conhecidos são reutilizados pela URL; novos são persistidos automaticamente.

### 2.2 Processamento técnico
Para cada artigo selecionado, gerar:
- Mini‑resumo técnico (2–3 parágrafos)
- 3 insights técnicos
- Explicação casual (2–3 frases)
- 3 sugestões de post (1.200–1.800 caracteres), cada uma com um **ângulo distinto**: (1) aprofundamento técnico, (2) storytelling/opinião, (3) lição prática aplicável
- Todo o conteúdo é gerado em dois idiomas: PT-BR e EN-US, com qualidade equivalente em ambos.

### 2.3 Visualização e histórico
- Interface web (React + Vite + TypeScript + Tailwind CSS) acessível em `http://localhost:5173`.
- **Buscar Artigos**: busca RSS e lista artigos disponíveis com botão "✨ Gerar".
- **Artigos Salvos**: lista todos os artigos já gravados no banco, indicando quais já têm conteúdo gerado, com botões "✨ Gerar" ou "Ver Posts".
- **Conteúdo Gerado**: lista histórico de todo conteúdo gerado, com link para detalhe.
- **Detalhe do Conteúdo**: abas PT-BR / EN-US com resumo técnico, insights, explicação casual e posts com contador de caracteres, botão de copiar e botão "marcar/desmarcar publicado" por post.
- **Marcação de publicação**: cada post (idioma + posição) pode ser marcado como publicado no LinkedIn, ajudando o usuário a acompanhar o que já foi ao ar. Nada é publicado automaticamente — apenas o registro manual do status.

---

## 3. Fora do MVP
- Agendamento periódico
- Notificações
- Publicação automática
- Multiusuário / login
- Métricas de engajamento
- Filtragem avançada

---

## 4. Regras de Negócio
1. Uma geração = 1 artigo.
2. Conteúdo sempre inclui as quatro partes.
3. Sugestões entre 1.200 e 1.800 caracteres (incluindo hashtags). O backend valida o tamanho e regenera automaticamente os posts fora da faixa (até `MaxRegenerationAttempts`, padrão 2); não convergindo, retorna o melhor resultado obtido.
4. Cada post deve obrigatoriamente conter: hook na primeira linha, hashtags (3–5) ao final, e call-to-action ou pergunta de encerramento.
5. Nada é publicado automaticamente.
6. 100% sob demanda.

---

## 5. Casos de Uso

### UC01 – Buscar artigos
1. Usuário clica em "Buscar Artigos".
2. Sistema consulta fontes RSS configuradas.
3. Novos artigos são salvos no banco; existentes são reutilizados pela URL.
4. Retorna lista com até `RssMaxArticles` artigos.

### UC02 – Listar artigos salvos
1. Usuário acessa "Artigos Salvos".
2. Sistema retorna todos os artigos do banco com flag `hasContent`.
3. Artigos com conteúdo exibem botão "Ver Posts"; sem conteúdo, botão "✨ Gerar".

### UC03 – Gerar conteúdo
1. Usuário clica em "✨ Gerar" para um artigo.
2. Sistema verifica se já existe conteúdo gerado — se sim, retorna sem chamar a IA.
3. Se não, extrai texto do artigo e chama a IA duas vezes (PT-BR e EN-US).
4. Conteúdo é persistido e ReviewPackage é retornado.
5. Frontend navega automaticamente para a tela de detalhe.

### UC04 – Listar conteúdo gerado
1. Usuário acessa "Conteúdo Gerado".
2. Sistema retorna lista de todos os conteúdos com título, URL e data de geração.
3. Cada item é um link para o detalhe.

### UC05 – Visualizar detalhe do conteúdo
1. Usuário acessa o detalhe de um conteúdo.
2. Sistema retorna ReviewPackage completo.
3. Usuário alterna entre abas PT-BR e EN-US.
4. Usuário copia o post desejado para o LinkedIn.

### UC06 – Revisão manual
1. Usuário lê os posts gerados.
2. Copia o texto de sua preferência.
3. Cola e edita manualmente no LinkedIn antes de publicar.

### UC07 – Marcar post como publicado
1. Após publicar um post no LinkedIn, o usuário clica em "marcar publicado" naquele post.
2. Sistema registra a publicação (idempotente) e o post passa a exibir o selo "✓ Publicado".
3. O usuário pode desmarcar a qualquer momento.

---

## 6. Critérios de Aceitação
- Buscar = até 5 segundos.
- Conteúdo coerente com artigo.
- Geração das 4 partes obrigatória, em PT-BR e EN-US.
- Cada PostSuggestion deve ter entre 1.200 e 1.800 caracteres.
- Texto consolidado legível.
- Usuário consegue realizar todo o fluxo manualmente.

---

## 7. Glossário
- Artigo bruto: conteúdo original.
- Conteúdo processado: os 4 blocos.
- Consolidado: bloco único retornado.
- Post: texto adaptado para LinkedIn.
- ContentBlock: conjunto das 4 partes geradas em um idioma específico.