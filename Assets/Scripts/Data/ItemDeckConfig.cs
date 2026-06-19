using System.Collections.Generic;
using UnityEngine;

namespace ChronosAndCards.Data
{
    /// <summary>
    /// Configuración del mazo de objetos. Define qué ítems están disponibles,
    /// cuántas copias de cada uno, y su peso de aparición.
    /// </summary>
    [CreateAssetMenu(fileName = "ItemDeckConfig", menuName = "ChronosAndCards/ItemDeckConfig")]
    public class ItemDeckConfig : ScriptableObject
    {
        /// <summary>Lista de entradas que definen el contenido del mazo.</summary>
        [SerializeField] private List<ItemDeckEntry> _entries = new();

        /// <summary>Si true, re-baraja el mazo cuando se agota. Si false, notifica agotamiento.</summary>
        [SerializeField] private bool _reshuffleOnEmpty = true;

        public IReadOnlyList<ItemDeckEntry> Entries => _entries.AsReadOnly();
        public bool ReshuffleOnEmpty => _reshuffleOnEmpty;
    }

    /// <summary>Entrada del mazo: define un tipo de ítem, sus copias y su peso.</summary>
    [System.Serializable]
    public class ItemDeckEntry
    {
        /// <summary>Referencia al ScriptableObject del ítem.</summary>
        [SerializeField] private ItemDefinition _itemDefinition;

        /// <summary>Número de copias de este ítem en el mazo.</summary>
        [SerializeField, Range(1, 10)] private int _copies = 1;

        /// <summary>Peso de aparición relativo (mayor peso = más probable).</summary>
        [SerializeField, Range(0.1f, 10f)] private float _weight = 1f;

        public ItemDefinition ItemDefinition => _itemDefinition;
        public int Copies => _copies;
        public float Weight => _weight;
    }
}
