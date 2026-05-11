# Functional Spec – MVP v0.2

## 1. Objetivo do MVP
Permitir que um desenvolvedor gere conteúdo técnico curto e relevante baseado em artigos do ecossistema Microsoft, exibidos como um bloco único para revisão manual antes da publicação.

---

## 2. Funcionalidades Incluídas

### 2.1 Coleta manual de artigos
- Busca em fontes: DevBlogs, .NET Blog, Azure Updates, Learn, Medium (.NET/Azure).
- Limite 3–5 artigos por chamada.
- Fluxo totalmente síncrono realizado pelo usuário.

### 2.2 Processamento técnico
Para cada artigo selecionado, gerar:
- Mini‑resumo técnico (2–3 parágrafos)
- 3 insights técnicos
- Explicação casual (2–3 frases)
- 3 sugestões de post (1.200–1.800 caracteres)
- Todo o conteúdo é gerado em dois idiomas: PT-BR e EN-US, com qualidade equivalente em ambos.

### 2.3 Exibição consolidada
Exibe tudo em um único bloco de texto.
Internamente, cada parte é armazenada separada para evolução futura.

---

## 3. Fora do MVP
- Agendamento periódico
- Notificações
- Publicação automática
- Multiusuário / login
- Dashboard
- Histórico
- Métricas
- Filtragem avançada

---

## 4. Regras de Negócio
1. Uma geração = 1 artigo.
2. Conteúdo sempre inclui as quatro partes.
3. Sugestões entre 1.200 e 1.800 caracteres (incluindo hashtags).
4. Cada post deve obrigatoriamente conter: hook na primeira linha, hashtags (3–5) ao final, e call-to-action ou pergunta de encerramento.
5. Nada é publicado automaticamente.
6. 100% sob demanda.

---

## 5. Casos de Uso

### UC01 – Buscar artigos
1. Usuário chama endpoint.
2. Sistema busca fontes.
3. Retorna lista.
4. Usuário escolhe um artigo.

### UC02 – Gerar conteúdo
1. Usuário envia o ID.
2. Sistema extrai texto.
3. IA gera conteúdo.
4. Tudo é salvo.
5. Retorna ReviewPackage.

### UC03 – Revisão
1. Usuário lê texto.
2. Edita manualmente se quiser.
3. Copia para o LinkedIn.

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