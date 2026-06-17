using UnityEngine;

public class inventarioeloja : MonoBehaviour
{
    [Header("Painéis")]
    public GameObject painel;
    public GameObject menu;

    [Header("Menu Armas")]
    public GameObject Menuarmas;

    [Header("Menu Amuletos")]
    public GameObject amuletospainel;
    public GameObject Shopamuletos;

    [Header("Amuletos")]
    public GameObject vitalidade;
    public GameObject furia;
    public GameObject rapidez;
    public GameObject ganacia;
    public GameObject escudo;

  

    public void Sairdomenu()
    {
        painel.SetActive(false);
    }

    public void Shoparmas()
    {
        menu.SetActive(false);
        Menuarmas.SetActive(true);
    }

    public void SairdaShopArmas()
    {
        Menuarmas.SetActive(false);
        menu.SetActive(true);
    }

    public void ShopAmuletos()
    {
        menu.SetActive(false);
        Shopamuletos.SetActive(true);

    }

   

    public void Vitalidade()
    {
        Shopamuletos.SetActive(false);
        vitalidade.SetActive(true);
    }

    public void Rapidez()
    {
        Shopamuletos.SetActive(false);
        rapidez.SetActive(true);
    }

    public void Furia()
    {
        Shopamuletos.SetActive(false);
        furia.SetActive(true);
    }

    public void Escudo()
    {
        Shopamuletos.SetActive(false);
        escudo.SetActive(true);
    }

    public void Ganacia()
    {
        Shopamuletos.SetActive(false);
        ganacia.SetActive(true);
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
        painelamuleto.SetActive(false);
        Shopamuletos.SetActive(true);
    }



    public void Comprar_Vitalidade(){
         ComprarAmulet("Vitalidade");
    }
    public void Comprar_Ganacia(){
    ComprarAmulet("Ganacia");
    }
    public void Comprar_Escudo()
    {
        ComprarAmulet("Escudo");
    }
    public void Comprar_Rapidez()
    {
        ComprarAmulet("Rapidez");
    }
    public void Comprar_Furia()
    {
        ComprarAmulet("Furia");
    }

    private void ComprarAmulet(string nomeAmulet)
    {
        if (Temdinheiro())
        {
            GameDatabase.Instance.data.plets -= 250;
            GameDatabase.Instance.AddOwnedAmulet(nomeAmulet);
            GameDatabase.Instance.SaveGame();
        }
        else
        {
           Debug.Log($"ERROOOOOOO");
           
        }
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
}