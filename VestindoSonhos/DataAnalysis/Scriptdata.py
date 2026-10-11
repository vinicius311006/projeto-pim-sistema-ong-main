import json
import pandas as pd
import seaborn as sns
import matplotlib.pyplot as plt
import os
from pathlib import Path

# Ler o arquivo JSON
pasta_script = os.path.dirname(os.path.abspath(__file__))
caminho_json = os.path.join(pasta_script, "data.json")
print(caminho_json)
with open(caminho_json, "r", encoding="utf-8") as arquivo:
    dados_dict = json.load(arquivo)

# Converter para DataFrames
df_doacoesMateriais = pd.DataFrame(dados_dict["estatisticasRoupas"])
df_Financeiro = pd.DataFrame(dados_dict["estatisticasFinanceiras"])
df_FDoacoes = pd.DataFrame(dados_dict["estatisticasFinanceirasDoacao"])

# Formatação das datas para simplificar a legenda
df_doacoesMateriais["Data"] = pd.to_datetime(
    df_doacoesMateriais["Data"], format="mixed"
).dt.date

df_Financeiro["Data"] = pd.to_datetime(df_Financeiro["Data"], format="mixed").dt.date

df_FDoacoes["Data"] = pd.to_datetime(df_FDoacoes["Data"], format="mixed").dt.date

# Gerar os Gráficos
sns.set_theme(style="darkgrid")

fig, axes = plt.subplots(2, 2, figsize=(16, 12))

# 1. Fluxo de Valores
sns.barplot(data=df_Financeiro, x="Data", y="Valor", hue="Razao", ax=axes[0, 0])
axes[0, 0].set_title("Fluxo Financeiro (Valor Total)")
axes[0, 0].tick_params(axis="x", rotation=45)

# 2. Doações Financeiras
sns.barplot(data=df_FDoacoes, x="Data", y="Valor", ax=axes[0, 1], palette="magma")
axes[0, 1].set_title("Entradas de Doações (R$)")

# 3. Doações de Itens
sns.barplot(
    data=df_doacoesMateriais, x="Data", y="Quantidade", ax=axes[1, 0], palette="viridis"
)
axes[1, 0].set_title("Quantidade de Itens Doados")

# 4. GRÁFICO HORIZONTAL - QUANTIDADE POR DATA E CATEGORIA
# y='Razao' torna horizontal, hue='Data' separa as datas por cor
sns.countplot(data=df_Financeiro, y="Razao", hue="Data", ax=axes[1, 1])
axes[1, 1].set_title("Qtd de Movimentações por Categoria e Data")
axes[1, 1].set_xlabel("Número de Operações Realizadas")
axes[1, 1].set_ylabel("Razão / Categoria")
axes[1, 1].legend(title="Data", loc="lower right", fontsize="small")

# Ajusta auto. o layout
plt.tight_layout()

# Verifica se ja existe um arquivo de imagem gerado pelo grafico
arquivo = Path(".\\wwwroot\\images\\data\\grafico.jpeg")
if arquivo.exists() and arquivo.is_file():
    arquivo.unlink()  # deleta a imagem
    print(f"O arquivo {arquivo} foi excluído.")

else:
    print(f"O arquivo {arquivo} não existe.")

# Salva a img do graf em arquivo .jpg e define qualidade
plt.savefig(".\\wwwroot\\images\\data\\grafico.jpeg", dpi=300)
