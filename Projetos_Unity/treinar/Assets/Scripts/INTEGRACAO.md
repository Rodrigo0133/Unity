# 🛠️ Guia de Integração na Unity

Para que todas as novas mecânicas, bosses e base de dados funcionem perfeitamente no seu projeto da Unity, siga os passos abaixo para configurar os GameObjects, Prefabs e Componentes no Inspector.

---

## 1. Banco de Dados e Salvamento (`GameDatabase`)
O script `GameDatabase.cs` é um Singleton persistente (`DontDestroyOnLoad`).
* **Como configurar**:
  1. Na sua primeira cena (ex: Menu Principal), crie um GameObject vazio chamado `GameDatabase`.
  2. Adicione o componente `GameDatabase` a ele.
  3. O script tratará de carregar automaticamente os dados salvos do `PlayerPrefs` ao iniciar e salvará a posição do jogador, dinheiro, amuletos e espadas nas transições ou checkpoints.

---

## 2. Gerenciador da Loja (`ShopManager`)
* **Como configurar**:
  1. Crie um GameObject chamado `ShopManager` na sua cena de loja (ou na cena principal).
  2. Adicione o componente `ShopManager` a ele.
  3. No Inspector do `ShopManager`, configure os custos e danos das espadas:
     * **Custo do 3º Slot**: custo em moedas (Plets) para comprar o slot extra (padrão: 100).
     * **Sword Upgrade Costs**: Tamanho 4 (ex: `0`, `80`, `150`, `250`).
     * **Sword Damage Values**: Tamanho 5 (ex: `0`, `25`, `50`, `75`, `100`).
  4. Nos botões da sua interface de Loja (UI Buttons), aponte o evento `OnClick()` para chamar as funções públicas do `ShopManager`:
     * `UpgradeSword()` (Melhorar espada)
     * `BuyThirdSlot()` (Comprar o 3º slot)
     * Para comprar amuletos, configure um botão chamando `BuyAmulet(string id)` com um dos seguintes IDs:
       * `vitalidade` (Amuleto de Vitalidade)
       * `furia` (Amuleto de Fúria)
       * `ganancia` (Amuleto de Ganância)
       * `rapidez` (Amuleto de Rapidez)
       * `escudo` (Amuleto de Escudo)
     * Para equipar/desequipar, chame a função `EquipAmulet(string id)`.

---

## 3. Moedas Plets (`PletCoin`)
Os inimigos dropam moedas automaticamente ao morrer através do script. O código já cria as moedas visualmente de forma automática caso não queira configurar nada!
* **Se quiser customizar visualmente a moeda**:
  1. O código usa por padrão o sprite `"Knob.psd"` (círculo nativo da Unity) colorido de dourado.
  2. Se preferir usar um Prefab próprio, pode criar um objeto com um sprite de moeda, um `CircleCollider2D` (marcado como `isTrigger`) e um `Rigidbody2D` com gravidade.

---

## 4. Configuração do 3º Boss (Irmão do King Cube)
* **Como configurar**:
  1. Crie o GameObject para o Boss na cena.
  2. Adicione uma imagem/sprite (Sprite Renderer), um `Collider2D` (ex: BoxCollider2D para a física de colisão com o chão) e um `Rigidbody2D` (Defina a gravidade e em *Constraints* marque **Freeze Rotation Z**).
  3. Garanta que a **Tag** do GameObject está configurada como `Enemy`.
  4. Adicione o componente `BossIrmaoKingCube`.
  5. Crie um objeto vazio como filho do Boss posicionado na frente dele e associe-o no campo **Ponto Disparo** no Inspector (de onde as moedas serão atiradas).
  6. *(Opcional)* Se tiver um prefab de projétil de moeda customizado, associe no campo **Prefab Projetil Moeda** (caso contrário, o script criará círculos amarelos automáticos).

---

## 5. Configuração do Último Boss (Final Boss)
* **Como configurar**:
  1. Crie o GameObject para o Boss na cena.
  2. Adicione um `SpriteRenderer`, um `Collider2D` e um `Rigidbody2D` (gravidade ativa, **Freeze Rotation Z**).
  3. Defina a **Tag** do GameObject como `Enemy`.
  4. Adicione o script `UltimoBoss`.
  5. Crie um objeto vazio como filho do Boss e associe-o no campo **Visual Escudo Imunidade**. Esse objeto será ativado automaticamente quando o Boss invocar os inimigos e estiver imune.
  6. Associe os seguintes prefabs no Inspector:
     * **Prefab Bola Fogo**: O projétil de fogo (cai do céu na Etapa 1 e disparado na Etapa 2).
     * **Prefab Corrente**: O objeto horizontal que o jogador deve pular (Etapa 2).
     * **Prefabs Cabecas Lideres**: Um array de GameObjects com as cabeças flutuantes dos líderes supremos.
     * **Prefab Inimigo Invocado**: O prefab de inimigo comum que o jogador precisa derrotar para quebrar a imunidade do boss.
     * *(Nota: Se deixar os prefabs vazios, o script gerará elementos substitutos nativos automaticamente para o jogo nunca travar)*.

---

## 6. Como Fazer o Menu de Amuletos e o Inventário funcionar (UI)

Para implementar a interface visual onde o jogador compra, visualiza e equipa os amuletos, siga este passo a passo usando o sistema UI (Canvas) da Unity.

### Passo 1: Estrutura da UI no Canvas
1. No seu **Canvas**, crie um Painel (`Panel`) chamado `MenuAmuletos` (desative-o por padrão e ative ao pressionar uma tecla, ex: `I` ou `Esc`).
2. Adicione um título na parte superior ("Inventário de Amuletos").
3. Crie uma seção para mostrar os **Slots Equipados**:
   * Crie 3 slots visuais vazios (imagens quadradas). 
   * Se o jogador não comprou o 3º slot, mostre o 3º slot com um cadeado (bloqueado).
4. Crie uma seção para a **Grade de Amuletos** (Inventário/Loja):
   * Adicione um componente `GridLayoutGroup` a este container.
   * Crie um botão de slot para cada amuleto. Cada botão deve ter:
     * Uma imagem representativa do amuleto.
     * Um texto com o nome (Ex: "Amuleto de Vitalidade").
     * Um botão de ação (Ex: "Comprar / Equipar").

### Passo 2: Script Controlador da Interface (`AmuletUI.cs`)
Crie um script com o nome `AmuletUI.cs` na sua pasta de Scripts e cole o código abaixo. Ele servirá para atualizar a interface com base nos dados reais do jogo:

```csharp
using UnityEngine;
using UnityEngine.UI;

public class AmuletUI : MonoBehaviour
{
    [Header("=== ELEMENTOS DOS SLOTS EQUIPADOS ===")]
    public Image slot1Image;
    public Image slot2Image;
    public Image slot3Image;
    public GameObject cadeadoSlot3;

    [Header("=== TEXTOS DO INVENTÁRIO (OPCIONAL) ===")]
    public Text textoPlets;

    [Header("=== COMPONENTES DOS AMULETOS (LOJA/INVENTÁRIO) ===")]
    public Button botaoVitalidade;
    public Text textoVitalidade; 

    public Button botaoFuria;
    public Text textoFuria;

    public Button botaoGanancia;
    public Text textoGanancia;

    public Button botaoRapidez;
    public Text textoRapidez;

    public Button botaoEscudo;
    public Text textoEscudo;

    public Button botaoSlot3;
    public Text textoSlot3; 

    [Header("=== ELEMENTOS DA ESPADA ===")]
    public Button botaoEspada;
    public Text textoEspada;

    void OnEnable()
    {
        AtualizarUI();
    }

    public void AtualizarUI()
    {
        if (GameDatabase.Instance == null) return;
        SaveData data = GameDatabase.Instance.data;

        // 1. Atualiza dinheirinho
        if (textoPlets != null) textoPlets.text = "Plets: " + data.plets;

        // 2. Atualiza os slots equipados visuais
        AtualizarSlotEquipado(slot1Image, data.equippedAmulets.Count > 0 ? data.equippedAmulets[0] : null);
        AtualizarSlotEquipado(slot2Image, data.equippedAmulets.Count > 1 ? data.equippedAmulets[1] : null);
        
        if (data.hasUnlockedThirdSlot)
        {
            if (cadeadoSlot3 != null) cadeadoSlot3.SetActive(false);
            AtualizarSlotEquipado(slot3Image, data.equippedAmulets.Count > 2 ? data.equippedAmulets[2] : null);
        }
        else
        {
            if (cadeadoSlot3 != null) cadeadoSlot3.SetActive(true);
            if (slot3Image != null) slot3Image.color = Color.black; // Bloqueado
        }

        // 3. Atualiza estado dos botões da loja/inventário
        ConfigurarBotaoAmuleto("vitalidade", botaoVitalidade, textoVitalidade);
        ConfigurarBotaoAmuleto("furia", botaoFuria, textoFuria);
        ConfigurarBotaoAmuleto("ganancia", botaoGanancia, textoGanancia);
        ConfigurarBotaoAmuleto("rapidez", botaoRapidez, textoRapidez);
        ConfigurarBotaoAmuleto("escudo", botaoEscudo, textoEscudo);

        // Atualiza botão do 3º slot
        if (botaoSlot3 != null && textoSlot3 != null)
        {
            if (data.hasUnlockedThirdSlot)
            {
                textoSlot3.text = "3º Slot Desbloqueado";
                botaoSlot3.interactable = false;
            }
            else
            {
                textoSlot3.text = "Comprar 3º Slot (100 Plets)";
                botaoSlot3.interactable = data.plets >= 100;
            }
        }

        // Atualiza botão da Espada
        if (botaoEspada != null && textoEspada != null && ShopManager.Instance != null)
        {
            int nextLevel = data.swordLevel + 1;
            if (nextLevel > 4)
            {
                textoEspada.text = "Espada no Máx (Nível 4)";
                botaoEspada.interactable = false;
            }
            else
            {
                int custo = ShopManager.Instance.swordUpgradeCosts[nextLevel - 1];
                textoEspada.text = $"Melhorar Espada (Custo: {custo} Plets)";
                botaoEspada.interactable = data.plets >= custo;
            }
        }
    }

    private void AtualizarSlotEquipado(Image imgSlot, string amuletId)
    {
        if (imgSlot == null) return;

        if (string.IsNullOrEmpty(amuletId))
        {
            imgSlot.color = new Color(1, 1, 1, 0.2f); 
            imgSlot.sprite = null;
        }
        else
        {
            imgSlot.color = Color.white; 
        }
    }

    private void ConfigurarBotaoAmuleto(string id, Button btn, Text txt)
    {
        if (btn == null || txt == null) return;
        SaveData data = GameDatabase.Instance.data;
        AmuletInfo info = AmuletDatabase.GetById(id);

        if (info == null) return;

        if (data.ownedAmulets.Contains(id))
        {
            btn.interactable = true;
            if (data.equippedAmulets.Contains(id))
            {
                txt.text = "Desequipar";
            }
            else
            {
                txt.text = "Equipar";
                int maxSlots = data.hasUnlockedThirdSlot ? 3 : 2;
                if (data.equippedAmulets.Count >= maxSlots) btn.interactable = false;
            }
        }
        else 
        {
            txt.text = $"Comprar ({info.cost} Plets)";
            btn.interactable = data.plets >= info.cost;
        }
    }

    public void AcaoBotaoAmuleto(string id)
    {
        if (GameDatabase.Instance == null || ShopManager.Instance == null) return;
        SaveData data = GameDatabase.Instance.data;

        if (data.ownedAmulets.Contains(id))
        {
            ShopManager.Instance.EquipAmulet(id);
        }
        else
        {
            ShopManager.Instance.BuyAmulet(id);
        }
        AtualizarUI();
    }

    public void AcaoComprarSlot3()
    {
        if (ShopManager.Instance == null) return;
        ShopManager.Instance.BuyThirdSlot();
        AtualizarUI();
    }

    public void AcaoMelhorarEspada()
    {
        if (ShopManager.Instance == null) return;
        ShopManager.Instance.UpgradeSword();
        AtualizarUI();
    }
}
```

### Passo 3: Associação no Unity Inspector
1. Crie um objeto `AmuletUI` na cena e anexe o script `AmuletUI.cs` nele.
2. No Inspector, arraste os respectivos elementos da sua interface UI (Imagens dos Slots, textos e botões) para os campos do script.
3. Configure os eventos do botão no Inspector:
   * No botão do Amuleto de Vitalidade, adicione um evento `On Click()`, arraste o objeto com o script `AmuletUI` e escolha a função `AmuletUI -> AcaoBotaoAmuleto` escrevendo `"vitalidade"` no parâmetro de texto.
   * Repita para todos os botões de amuletos usando as respectivas IDs.
   * No botão do 3º slot, chame `AcaoComprarSlot3()`.
   * No botão de Melhorar Espada, chame `AcaoMelhorarEspada()`. Ele atualizará o texto do botão automaticamente mostrando o novo preço (ex: Custo: 80 Plets, depois 150 Plets, etc.).

---

## 7. Como Alterar ou Aumentar Preços da Loja
Se quiser alterar o preço para ficar mais alto ou mudar o dano de cada melhoria de espada, basta selecionar o GameObject **`ShopManager`** na sua hierarquia da Unity e alterar os valores no Inspector:
* Vá ao campo **Sword Upgrade Costs** no Inspector do `ShopManager` e digite os valores que desejar (Exemplo: altere de 80, 150, 250 para 150, 300, 500).
* O script `AmuletUI.cs` lerá automaticamente esses novos valores do Inspector e mostrará o texto do botão atualizado na hora!

---

## 7. Configurações Globais recomendadas
* **Tags**: Garanta que o jogador tem a Tag `Player` e os inimigos/bosses têm a Tag `Enemy`.
* **Layers**: Configure uma Layer para o chão (ex: `Ground` ou `Default`) e certifique-se de que está devidamente selecionada nos campos de colisão do jogador (`groundlayer` e `groundlayer2`).

---

## 8. Como Ganhar Dinheiro ao Clicar (Testes/Gameplay) e Exibir Dinheiro no HUD Principal

### Como ganhar dinheiro clicando (Cheat/Debug ou Clicker):
Para criar um botão que adiciona Plets toda vez que clicas nele (excelente para testar a loja ou se o teu jogo tiver mecânicas de clique):
1. Cria um botão na UI chamado `BotaoGanharDinheiro`.
2. No evento `On Click()` do botão no Unity Inspector, clica no símbolo `+`.
3. Arraste o GameObject `ShopManager` (onde colocaste o script `ShopManager.cs`) para o campo de objeto.
4. Na lista de funções, escolhe `ShopManager -> AddPlets(int)`.
5. No campo numérico que aparece abaixo, escreve a quantidade de dinheiro que queres que o jogador ganhe por clique (ex: `10` ou `50`).
6. Se quiseres que a interface do Menu de Amuletos seja atualizada imediatamente após o clique, adiciona um segundo evento `On Click()` no mesmo botão e aponta para o teu GameObject `AmuletUI` escolhendo a função `AmuletUI -> AtualizarUI`.

### Como fazer o dinheiro aparecer na tela permanentemente (HUD de Jogo):
Se queres que o teu saldo de Plets apareça constantemente no topo da tela enquanto jogas (e não apenas quando abres o inventário/loja):
1. No seu Canvas da HUD principal (que está sempre ativo na tela), cria um elemento de Texto (`Text` ou `TextMeshProUGUI`) e nomeia-o como `TextoPletsHUD`.
2. Cria um script chamado `HUDManager.cs` e cola o seguinte código simples nele:

```csharp
using UnityEngine;
using UnityEngine.UI;

public class HUDManager : MonoBehaviour
{
    public Text textoMoedas; // Arraste o seu TextoPletsHUD para aqui no Inspector

    void Update()
    {
        if (GameDatabase.Instance != null && textoMoedas != null)
        {
            // Lê constantemente a base de dados atualizada
            textoMoedas.text = "Plets: " + GameDatabase.Instance.data.plets;
        }
    }
}
```
3. Anexa este script num GameObject na sua cena e arrasta o elemento de texto para a variável `textoMoedas` no Inspector. Ele atualizará automaticamente e em tempo real toda vez que recolheres uma moeda (`PletCoin`) ou gastares dinheiro!
