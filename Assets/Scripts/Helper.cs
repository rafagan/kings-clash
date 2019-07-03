using UnityEngine;

/*
# Classe responsável por reunir métodos "genéricos" para auxiliar em funções
# básicas não implementadas pela Unity, a fim de facilitar o uso da mesma.
*/

public static class Extensions
{
	// Método responsável por fazer a busca de Transforms filhos, pela string nome do filho
	public static Transform SearchChild(this Transform target, string name)
	{
		if (target.name == name) return target;

		for (int i = 0; i < target.childCount; i++)
		{
			var result = SearchChild(target.GetChild(i), name);
			if (result != null) return result;
		}

		return null;
	}
}