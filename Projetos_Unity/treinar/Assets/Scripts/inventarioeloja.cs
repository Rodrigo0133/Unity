using UnityEngine;
using TMPro;
using System.Collections;

public class inventarioeloja : MonoBehaviour
{   
    private bool Comprado = false;
    private string amuletoAtual;
    private bool maximo = false;
    void Start()
    {
        AtualizarPrecoArma();
    }

    private void AtualizarPrecoArma()
    {
        int swordLevel = Mathf.Clamp(GameDatabase.Instance.data.swordLevel, 1, PlayerMovement.MaxSwordLevel);
        GameDatabase.Instance.data.swordLevel = swordLevel;

        if(swordLevel >= PlayerMovement.MaxSwordLevel){
            preco.text = "MAXIMO";
            maximo = true;
        }
        else
        {
            preco.text = precos[swordLevel - 1].ToString();
            maximo = false;
        }
    }
    [Header("Painéis")]
    public GameObject painel;
    public GameObject menu;

    [Header("Menu Armas")]
    public GameObject Menuarmas;
    public TextMeshProUGUI preco;
    private int[] precos = { 150, 200, 250, 350 };

    [Header("Menu Amuletos")]
    public GameObject amuletospainel;
    public GameObject Shopamuletos;
   

    [Header("Amuletos")]
    public GameObject vitalidade;
    public GameObject furia;
    public GameObject rapidez;
    public GameObject ganacia;
    public GameObject escudo;
    public TextMeshProUGUI butao;

    [Header("Texto")]
    public TextMeshProUGUI texto;
    public int segundos;
    public TextMeshProUGUI vatalidade;
    public TextMeshProUGUI feria;
    public TextMeshProUGUI rrpidez;
    public TextMeshProUGUI gfnacia;
    public TextMeshProUGUI egcudo;
  
    public void Abrirloja()
    {
        painel.SetActive(true);
    }
    public void Sairdomenu()
    {
        painel.SetActive(false);
    }

    public void Shoparmas()
    {
        menu.SetActive(false);
        Menuarmas.SetActive(true);
    }
    public void Rapidez()
    {
        
        Shopamuletos.SetActive(false);
        rapidez.SetActive(true);
        Rapidez_Verificar();
        amuletoAtual = "Rapidez";
    }
    public void Melhorararma()
    {
        AtualizarPrecoArma();
        if (!maximo)
        {
            if (Temarmasdinheiro()){
                GameDatabase.Instance.data.plets -= precos[GameDatabase.Instance.data.swordLevel - 1];
                GameDatabase.Instance.data.swordLevel++;
                AtualizarPrecoArma();
                PlayerMovement player = FindObjectOfType<PlayerMovement>();
                if (player != null) player.AtualizarDanoEspada();
                GameDatabase.Instance.SaveGame();
                StartCoroutine(Melhorado(segundos));
            }
            else
            {
                texto.text = "Não tens dinheiro suficiente!";
            }
        }
    }
    public void SairdaShopArmas()
    {
        texto.gameObject.SetActive(false);
        Menuarmas.SetActive(false);
        menu.SetActive(true);
    }

    public void ShopAmuletos()
    {
        menu.SetActive(false);
        Shopamuletos.SetActive(true);

    }
    public void SairdaShopAmuletos()
    {
        menu.SetActive(true);
        Shopamuletos.SetActive(false);
    }

   

    public void Vitalidade()
    {
        Shopamuletos.SetActive(false);
        vitalidade.SetActive(true);
        Vitalidade_Verificar();
        amuletoAtual = "Vitalidade";
    }
    public void Vitalidade_Verificar()
    {
        if (GameDatabase.Instance.data.ownedAmulets.Contains("Vitalidade")) 
        { 
            Comprado = true; 
            
            if (GameDatabase.Instance.data.equippedAmulets.Contains("Vitalidade")) 
            { 
                vatalidade.text = "Desequipar"; 
            } 
            else 
            { 
                vatalidade.text = "Equipar"; 
            } 
        } 
        else 
        { 
            Comprado = false; 
            vatalidade.text = "Comprar por 250 plets!"; 
        }
    }
    public void Furia_Verificar()
    { 
        if (GameDatabase.Instance.data.ownedAmulets.Contains("Furia")) 
        { 
            Comprado = true; 
            
            if (GameDatabase.Instance.data.equippedAmulets.Contains("Furia")) 
            { 
                feria.text = "Desequipar"; 
            } 
            else 
            { 
                feria.text = "Equipar"; 
            } 
        } 
        else 
        { 
            Comprado = false; 
            feria.text = "Comprar por 250 plets!"; 
        }
    }
    public void Rapidez_Verificar()
    {
        if (GameDatabase.Instance.data.ownedAmulets.Contains("Rapidez")) 
        { 
            Comprado = true; 
            
            if (GameDatabase.Instance.data.equippedAmulets.Contains("Rapidez")) 
            { 
                rrpidez.text = "Desequipar"; 
            } 
            else 
            { 
                rrpidez.text = "Equipar"; 
            } 
        } 
        else 
        { 
            Comprado = false; 
            rrpidez.text = "Comprar por 250 plets!"; 
        }
    }
    public void ganacia_vereficar()
    {
        if (GameDatabase.Instance.data.ownedAmulets.Contains("Ganacia")) 
        { 
            Comprado = true; 
            
            if (GameDatabase.Instance.data.equippedAmulets.Contains("Ganacia")) 
            { 
                gfnacia.text = "Desequipar"; 
            } 
            else 
            { 
                gfnacia.text = "Equipar"; 
            } 
        } 
        else 
        { 
            Comprado = false; 
            gfnacia.text = "Comprar por 250 plets!"; 
        }
    }
    public void Escudo_vereficar()
    {
        if (GameDatabase.Instance.data.ownedAmulets.Contains("Escudo")) 
        { 
            Comprado = true; 
            
            if (GameDatabase.Instance.data.equippedAmulets.Contains("Escudo")) 
            { 
                egcudo.text = "Desequipar"; 
            } 
            else 
            { 
                egcudo.text = "Equipar"; 
            } 
        } 
        else 
        { 
            Comprado = false; 
            egcudo.text = "Comprar por 250 plets!"; 
        }
    }
    public void Escudo()
    {
        Shopamuletos.SetActive(false);
        escudo.SetActive(true);
        Escudo_vereficar();
        amuletoAtual = "Escudo";
    }

    public void Furia()
    {
        Shopamuletos.SetActive(false);
        furia.SetActive(true);
        Furia_Verificar();
        amuletoAtual = "Furia";
    }
    public void testes()
    {
        GameDatabase.Instance.data.plets+= 250;
        Debug.Log("mais 250");
    }

    public void Ganacia()
    {
        Shopamuletos.SetActive(false);
        ganacia.SetActive(true);
        ganacia_vereficar();
        amuletoAtual = "Ganacia";
    }

    
    public void Sair_Vitalidade()
    {
        VoltarParaShopAmuletos(vitalidade);
    }
    public void Sair_Escudo()
    {
        VoltarParaShopAmuletos(escudo);
    }
    public void Sair_Furia()
    {
        VoltarParaShopAmuletos(furia);
    }
    public void Sair_Ganacia()
    {
        VoltarParaShopAmuletos(ganacia);
    } 
    public void Sair_Rapidez()
    {
        VoltarParaShopAmuletos(rapidez);

    } 
    private void VoltarParaShopAmuletos(GameObject painelamuleto)
    {
        texto.gameObject.SetActive(false);
        painelamuleto.SetActive(false);
        Shopamuletos.SetActive(true);
    }
   private void ComprarOuEquipar(string id)
    {
        if (GameDatabase.Instance.data.ownedAmulets.Contains(id))
        {
            equiparedesequipar(id);
        }
        else
        {
            ComprarAmulet(id);
        }
    }
    public void Comprar_Vitalidade() => ComprarOuEquipar("Vitalidade");
    public void Comprar_Ganacia()    => ComprarOuEquipar("Ganacia");
    public void Comprar_Escudo()     => ComprarOuEquipar("Escudo");
    public void Comprar_Rapidez()    => ComprarOuEquipar("Rapidez");
    public void Comprar_Furia()      => ComprarOuEquipar("Furia");

    private void ComprarAmulet(string nomeAmulet)
    {
        if (Temdinheiro())
        {
            GameDatabase.Instance.data.plets -= 250;
            GameDatabase.Instance.AddOwnedAmulet(nomeAmulet);
            GameDatabase.Instance.SaveGame();
            StartCoroutine(Dinheirosuficiente(segundos));
        }
        else
        {
           StartCoroutine(Dinheiroinsuficiente(segundos));
           GameDatabase.Instance.SaveGame();
        }
    }
    private IEnumerator Dinheiroinsuficiente(float segundos)
    {
        texto.text = "Não tens dinheiro suficiente!";
        texto.gameObject.SetActive(true);
        yield return new WaitForSeconds(segundos);
        texto.gameObject.SetActive(false);
    }
    private IEnumerator Dinheirosuficiente(float segundos)
    {
        texto.text = "Amuleto Comprado!";
        texto.gameObject.SetActive(true);
        yield return new WaitForSeconds(segundos);
        texto.gameObject.SetActive(false);
    }
    private IEnumerator Melhorado(float segundos)
    {
        texto.text = "Melhoração Comprada!";
        texto.gameObject.SetActive(true);
        yield return new WaitForSeconds(segundos);
        texto.gameObject.SetActive(false);
    }
    private IEnumerator MaxiEquipado(float segundos)
    {
        texto.text= "Máximo de Amuletos Equipados";
        texto.gameObject.SetActive(true);
        yield return new WaitForSeconds(segundos);
        texto.gameObject.SetActive(false);
    }
    public bool Temdinheiro()
    {
        if(GameDatabase.Instance.data.plets >= 250)
        {
            return true;
        }
        else
        {
            return false;
        }
        
    }
    public bool Temarmasdinheiro()
    {
        int swordLevel = Mathf.Clamp(GameDatabase.Instance.data.swordLevel, 1, PlayerMovement.MaxSwordLevel);
        if(swordLevel >= PlayerMovement.MaxSwordLevel)
        {
            return false;
        }

        if(GameDatabase.Instance.data.plets >= precos[swordLevel - 1])
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public void maximos()
    {
        preco.text = "MAXIMO";
    }
   
    public void equiparedesequipar(string id)
    {
        if (GameDatabase.Instance.data.equippedAmulets.Contains(id))
        {
            GameDatabase.Instance.data.equippedAmulets.Remove(id);
            GameDatabase.Instance.SaveGame();
        }
        else
        {
            
            int limiteMaximo = GameDatabase.Instance.data.hasUnlockedThirdSlot ? 3 : 2;

            if (GameDatabase.Instance.data.equippedAmulets.Count < limiteMaximo)
            {
                GameDatabase.Instance.data.equippedAmulets.Add(id);
                GameDatabase.Instance.SaveGame();
            }
            else
            {
                StartCoroutine(MaxiEquipado(segundos));
            }   
        }

        PlayerMovement player = FindObjectOfType<PlayerMovement>();
        if (player != null)
            player.AtualizarEfeitosAmuletos();
    }
}
